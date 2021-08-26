using UnityEngine;
using UniRx;
using UtilityToolkit.Audio;

namespace AgoraExtension.Samples
{
    public class MicrophonePresenter : MonoBehaviour
    {
        [SerializeField] MicrophoneControlView _microphoneControlView;
        [SerializeField] UnityAudioOut _loopbackAudioOut;
        [SerializeField] AgoraClientContext _agoraContext;
        
        private UnityMicrophone _microphone;
        
        private void Awake()
        {
            _microphone = new UnityMicrophone();
            
            _microphoneControlView.CurrentDevice
            .SkipLatestValueOnSubscribe()
            .Subscribe(device => 
            {
                _microphone.Stop();
                _microphone.Start(device);
            })
            .AddTo(this);
            
            _microphoneControlView.LoopbackIsActive
            .SkipLatestValueOnSubscribe()
            .Subscribe(loopback =>
            {
                if (loopback)
                {
                    _loopbackAudioOut.StartOutput();
                }
                else
                {
                    _loopbackAudioOut.StopOutput();
                }
            })
            .AddTo(this);

            _microphone.OnProcessFrame += OnProcessFrame;
        }
        
        private void OnDestroy()
        {
            _microphone.OnProcessFrame -= OnProcessFrame;
            _microphone.Dispose();
        }
        
        private void OnProcessFrame(float[] data)
        {
            if (_microphoneControlView.LoopbackIsActive.Value)
            {
                _loopbackAudioOut.PushAudioFrame(data);
            }
            
            if (_agoraContext.IsJoined)
            {
                _agoraContext.SendAudioFrame(data);
            }
        }
    }
}
