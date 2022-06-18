// This code is based on InputDeviceHandle from Lasp.
// https://github.com/keijiro/Lasp/blob/v2/Packages/jp.keijiro.lasp/Runtime/Internal/InputDeviceHandle.cs

using System;
using System.Runtime.InteropServices;
using InvalidOp = System.InvalidOperationException;
using PInvokeCallbackAttribute = AOT.MonoPInvokeCallbackAttribute;

namespace AudioUtilityToolkit.SoundIOExtension
{
    public sealed class InputStream : IDisposable
    {
        #region Public properties and methods

        public string DeviceID => _device.ID;
        public string DeviceName => _device.Name;

        public bool IsValid
          => _stream != null && !_stream.IsInvalid && !_stream.IsClosed;

        public int ChannelCount => _stream.Layout.ChannelCount;
        public int SampleRate => _stream.SampleRate;

        public event Action<float[]> OnProcessFrame;

        public void Update()
        {
            // Nothing to do when the stream is inactive.
            if (!IsValid) return;

            while (_ring.Count > _frameBuffer.Length)
            {
                lock (_ring)
                {
                    _ring.Dequeue(new Span<float>(_frameBuffer));
                }
                OnProcessFrame?.Invoke(_frameBuffer);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;

            _device?.Dispose();
            _device = null;

            if (_self.IsAllocated) _self.Free();
        }

        #endregion

        #region Constructor

        public InputStream(SoundIO.Device device)
        {
            _self = GCHandle.Alloc(this);
            _device = device;
            OpenStream();
        }

        #endregion

        #region Internal objects

        // A GC handle used to share 'this' pointer with unmanaged code
        GCHandle _self;

        // SoundIO objects
        SoundIO.Device _device;
        SoundIO.InStream _stream;

        // Input stream ring buffer
        // This object will be accessed from both the main/callback thread.
        // Must be locked when accessing it.
        RingBuffer<float> _ring;

        // Frame buffer
        float[] _frameBuffer;
        readonly int _frameDurationMs = 20; // 20[ms]
        readonly float _bufferingTimeSec = 0.5f; // 0.5[sec]

        #endregion

        #region Stream initialization

        void OpenStream()
        {
            if (_device is null) throw new InvalidOp("Invalid device");
            
            try
            {
                _stream = SoundIO.InStream.Create(_device);

                if (_stream.IsInvalid)
                    throw new InvalidOp("Stream allocation error");

                if (_device.Layouts.Length == 0)
                    throw new InvalidOp("No channel layout");

                // Calculate the best latency.
                // TODO: Should we use the target frame rate instead of 1/60?
                var bestLatency = Math.Max(1.0 / 60, _device.SoftwareLatencyMin);

                // Stream properties
                _stream.Format = SoundIO.Format.Float32LE;
                _stream.Layout = _device.Layouts[0];
                _stream.SoftwareLatency = bestLatency;
                _stream.ReadCallback = _readCallback;
                _stream.OverflowCallback = _overflowCallback;
                _stream.ErrorCallback = _errorCallback;
                _stream.UserData = GCHandle.ToIntPtr(_self);

                var err = _stream.Open();

                if (err != SoundIO.Error.None)
                    throw new InvalidOp($"Stream initialization error ({err})");

                // Calculate buffer size
                var ringBufferSize = (int)(_stream.Layout.ChannelCount * _stream.SampleRate * _bufferingTimeSec);
                var frameBufferSize = (int)(_frameDurationMs / 1000.0f * _stream.Layout.ChannelCount * _stream.SampleRate);

                // Ring/frame buffer allocation
                _ring = new RingBuffer<float>(ringBufferSize);
                _frameBuffer = new float[frameBufferSize];

                // Start streaming.
                _stream.Start();
            }
            catch
            {
                // Dispose the stream on an exception.
                _stream?.Dispose();
                _stream = null;
                _device?.Dispose();
                _device = null;
                throw;
            }
        }

        #endregion

        #region SoundIO callback delegates

        static SoundIO.InStream.ReadCallbackDelegate
          _readCallback = OnReadInStream;

        static SoundIO.InStream.OverflowCallbackDelegate
          _overflowCallback = OnOverflowInStream;

        static SoundIO.InStream.ErrorCallbackDelegate
          _errorCallback = OnErrorInStream;

        [PInvokeCallback(typeof(SoundIO.InStream.ReadCallbackDelegate))]
        unsafe static void OnReadInStream
          (ref SoundIO.InStreamData stream, int min, int left)
        {
            // Recover the 'this' reference from the UserData pointer.
            var self = (InputStream)
              GCHandle.FromIntPtr(stream.UserData).Target;

            while (left > 0)
            {
                // Start reading the buffer.
                var count = left;
                SoundIO.ChannelArea* areas;
                stream.BeginRead(out areas, ref count);

                // When getting count == 0, we must stop reading
                // immediately without calling InStream.EndRead.
                if (count == 0) break;

                if (areas == null)
                {
                    // We must do zero-fill when receiving a null pointer.
                    lock (self._ring)
                      self._ring.EnqueueDefault(stream.BytesPerFrame * count);
                }
                else
                {
                    // Determine the memory span of the input data with
                    // assuming the data is tightly packed.
                    // TODO: Is this assumption always true?
                    var span = new ReadOnlySpan<Byte>
                      ((void*)areas[0].Pointer, areas[0].Step * count);

                    // Transfer the data to the ring buffer.
                    lock (self._ring) self._ring.Enqueue(span);
                }

                stream.EndRead();

                left -= count;
            }
        }

        [PInvokeCallback(typeof(SoundIO.InStream.OverflowCallbackDelegate))]
        static void OnOverflowInStream(ref SoundIO.InStreamData stream)
          => UnityEngine.Debug.LogWarning("InStream overflow");

        [PInvokeCallback(typeof(SoundIO.InStream.ErrorCallbackDelegate))]
        static void OnErrorInStream
          (ref SoundIO.InStreamData stream, SoundIO.Error error)
          => UnityEngine.Debug.LogWarning($"InStream error ({error})");

        #endregion
    }
}