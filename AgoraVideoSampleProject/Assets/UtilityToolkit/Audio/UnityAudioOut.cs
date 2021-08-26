using System;
using UnityEngine;

namespace UtilityToolkit.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class UnityAudioOut : MonoBehaviour
    {
        [SerializeField] private int _channels = 1; // Mono:1, Stereo:2
        [SerializeField] private int _samplingFrequency = 48000; // 48[kHz]
        
        private AudioSource _audioSource;
        private int _audioClipSamples;
        private int _headPosition = 0;
        
        private void Awake()
        {
            _audioClipSamples = (int)(1.0f * _samplingFrequency); // 1.0[sec]
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples, (int)_channels, (int)_samplingFrequency, false);
            _audioSource.loop = true;
        }
        
        private void OnEnable()
        {
            _audioSource.Play();
        }
        
        private void OnDisable()
        {
            _audioSource.Stop();
        }
        
        public void StartOutput()
        {
            _audioSource.Play();
        }
        
        public void StopOutput()
        {
            _audioSource.Stop();
        }
        
        public void PushAudioFrame(float[] pcm)
        {
            _audioSource.clip.SetData(pcm, _headPosition);
            _headPosition += pcm.Length;
            _headPosition %= _audioClipSamples;
        }
    }
}