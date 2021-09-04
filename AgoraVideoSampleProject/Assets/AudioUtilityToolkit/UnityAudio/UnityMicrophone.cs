using System;
using UnityEngine;
using UniRx;

namespace AudioUtilityToolkit.UnityAudioExtension
{
    public class UnityMicrophone : IDisposable
    {
        public bool IsMute = false;

        public string CurrentDevice => _currentDevice;
        private string _currentDevice;

        public event Action<float[]> OnProcessFrame;

        private readonly int _samplingFrequency = 48000; // 48[kHz]
        private readonly int _frameDuration = 20; // 20[ms]
        private readonly int _micLengthSec = 1; // 1.0[sec]

        private readonly float[] _microphoneBuffer;
        private readonly float[] _frameBuffer;

        private AudioClip _audioClip;
        private int _headPosition = 0;

        private IDisposable _disposable;

        public UnityMicrophone()
        {
            var frameSize = (int)(_frameDuration / 1000.0f * _samplingFrequency);
            _microphoneBuffer = new float[_samplingFrequency * _micLengthSec];
            _frameBuffer = new float[frameSize];
        }

        public void Dispose()
        {
            Stop();
        }

        public bool Start(string deviceName = null, int updateRate = 30)
        {
            _currentDevice = deviceName;

            _audioClip = Microphone.Start(_currentDevice, true, _micLengthSec, _samplingFrequency);
            if (_audioClip is null) { return false; }

            _disposable = Observable.Interval(TimeSpan.FromMilliseconds(1000.0f / updateRate))
                                    .Subscribe(_ => Update());
            return true;
        }

        public void Stop()
        {
            _disposable?.Dispose();
            Microphone.End(_currentDevice);
        }

        private void Update()
        {
            var samplePosition = Microphone.GetPosition(_currentDevice);
            if (samplePosition < 0 || _headPosition == samplePosition)
            {
                return;
            }

            if (!IsMute)
            {
                _audioClip.GetData(_microphoneBuffer, 0);
                while (GetDataSize(_microphoneBuffer.Length, _headPosition, samplePosition) > _frameBuffer.Length)
                {
                    var remain = _microphoneBuffer.Length - _headPosition;
                    if (remain < _frameBuffer.Length)
                    {
                        Array.Copy(_microphoneBuffer, _headPosition, _frameBuffer, 0, remain);
                        Array.Copy(_microphoneBuffer, 0, _frameBuffer, remain, _frameBuffer.Length - remain);
                    }
                    else
                    {
                        Array.Copy(_microphoneBuffer, _headPosition, _frameBuffer, 0, _frameBuffer.Length);
                    }
                   
                    _headPosition = (_headPosition + _frameBuffer.Length) % _microphoneBuffer.Length;

                    OnProcessFrame?.Invoke(_frameBuffer);
                }
            }
        }

        private static int GetDataSize(int bufferLength, int head, int tail)
        {
            return (tail > head) ? (tail - head) : (tail + bufferLength - head);
        }
    }
}