using System;
using UnityEngine;

namespace AudioUtilityToolkit.UnityAudioExtension
{
    [RequireComponent(typeof(AudioSource))]
    public class UnityAudioOut : MonoBehaviour
    {
        [SerializeField] private int _channels = 1; // Mono:1, Stereo:2
        [SerializeField] private int _samplingFrequency = 48000; // [kHz]

        private AudioSource _audioSource;
        private int _audioClipSamples;
        private float _bufferingTimeSec = 1.0f; // 1.0[sec]

        private RingBuffer<float> _ringBuffer;

        private void Awake()
        {
            _audioClipSamples = (int)(_bufferingTimeSec * _samplingFrequency);

            _ringBuffer = new RingBuffer<float>((int)(_channels * _samplingFrequency * _bufferingTimeSec));

            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples, (int)_channels, (int)_samplingFrequency, true, OnAudioRead);
            _audioSource.loop = true;
        }

        private void OnAudioRead(float[] data)
        {
            lock (_ringBuffer)
            {
                _ringBuffer.Dequeue(new Span<float>(data));
            }
        }

        public void PushAudioFrame(float[] pcm)
        {
            lock (_ringBuffer)
            {
                _ringBuffer.Enqueue(pcm);
            }
        }

        public void StartOutput(int channels = 1, int samplingFrequency = 48000)
        {
            if (channels != _channels || samplingFrequency != _samplingFrequency)
            {
                _channels = channels;
                _samplingFrequency = samplingFrequency;
                _audioClipSamples = (int)(_bufferingTimeSec * _samplingFrequency);

                _ringBuffer = new RingBuffer<float>((int)(_channels * _samplingFrequency * _bufferingTimeSec));

                _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples, (int)_channels, (int)_samplingFrequency, true, OnAudioRead);
                _audioSource.loop = true;
            }
            _audioSource.Play();
        }

        public void StopOutput()
        {
            _audioSource.Stop();
        }
    }
}