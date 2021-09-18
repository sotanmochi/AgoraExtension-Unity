// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UniRx;
using agora_gaming_rtc;
using AudioUtilityToolkit;

namespace AgoraExtension
{
    public class AudioFrameReceiver : IDisposable
    {
        public IObservable<float[]> OnProcessFrame => _processFrameSubject;
        private Subject<float[]> _processFrameSubject = new Subject<float[]>();

        private readonly int _frameDurationMs = 20; // 20[ms]

        private bool _initialized;

        private IRtcEngine _rtcEngine;
        private IAudioRawDataManager _audioRawDataManager;

        private float[] _frameBuffer;
        private RingBuffer<float> _ringBuffer;
        private int _sampleRate;
        private int _channelCount;

        private Thread _pullAudioFrameThread;
        private bool _pullAudioFrame;

        private IDisposable _disposable;

        public AudioFrameReceiver()
        {
            UnityEngine.Application.quitting += Dispose;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Initialize()
        {
            _rtcEngine = IRtcEngine.QueryEngine();

            if (_rtcEngine != null)
            {
                _audioRawDataManager = AudioRawDataManager.GetInstance(_rtcEngine);
                _initialized = true;
            }
            else
            {
                _initialized = false;
            }
        }

        public void Start(int sampleRate = 48000, int channelCount = 2)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (_pullAudioFrame)
            {
                return;
            }

            _sampleRate = sampleRate;
            _channelCount = channelCount;
            
            _pullAudioFrame = true;
            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start();
            
            var frameBufferSize = (int)(_frameDurationMs / 1000.0f * _channelCount * _sampleRate);
            _frameBuffer = new float[frameBufferSize];

            _disposable = Observable.Interval(TimeSpan.FromMilliseconds(20))
            .Subscribe(_ => 
            {
                PullAudioFrame();
            });
        }

        public void Stop()
        {
            _disposable?.Dispose();

            _pullAudioFrame = false;
            _pullAudioFrameThread?.Join();
            _pullAudioFrameThread = null;
        }

        private void PullAudioFrame()
        {
            while (_ringBuffer.Count > _frameBuffer.Length)
            {
                lock(_ringBuffer)
                {
                    _ringBuffer.Dequeue(new Span<float>(_frameBuffer));
                }
                _processFrameSubject.OnNext(_frameBuffer);
            }
        }

        unsafe void PullAudioFrameThread()
        {
            int sampleRate = _sampleRate;
            int channels = _channelCount;

            var frameType = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var bytesPerSample = 2; // PCM16

            var samplesPerChannel = (int)(sampleRate * _frameDurationMs / 1000.0f);

            var frameBufferPointer = Marshal.AllocHGlobal(samplesPerChannel * channels * bytesPerSample);
            var pcmBuffer = new float[samplesPerChannel * channels];

            _ringBuffer = new RingBuffer<float>(samplesPerChannel * channels * 4); // 4[frames]

            var tic = new TimeSpan(DateTime.Now.Ticks);

            while (_pullAudioFrame)
            {
                var toc = new TimeSpan(DateTime.Now.Ticks);
                if (toc.Subtract(tic).Duration().Milliseconds >= _frameDurationMs)
                {
                    tic = new TimeSpan(DateTime.Now.Ticks);
                    _audioRawDataManager.PullAudioFrame(frameBufferPointer, (int)frameType, samplesPerChannel, bytesPerSample, channels, sampleRate, 0, 0);

                    var span = new ReadOnlySpan<Byte>((void*)frameBufferPointer, samplesPerChannel * channels * bytesPerSample);

                    // Convert 16bit PCM data bytes to 32bit float PCM data.
                    for (var i = 0; i < pcmBuffer.Length; i++)
                    {
                        pcmBuffer[i] = ConvertBytesToInt16(span.Slice(2 * i)) / 32767f; // Int16.MaxValue is 32767;
                    }

                    lock (_ringBuffer)
                    {
                        _ringBuffer.Enqueue(pcmBuffer);
                    }
                }
            }

            Marshal.FreeHGlobal(frameBufferPointer);
        }

        // Converts a span into a short
        // https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/BitConverter.cs#L248
        unsafe static short ConvertBytesToInt16(ReadOnlySpan<byte> value)
        {
            // if (value.Length < sizeof(short))
                // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<short>(ref MemoryMarshal.GetReference(value));
        }
    }
}