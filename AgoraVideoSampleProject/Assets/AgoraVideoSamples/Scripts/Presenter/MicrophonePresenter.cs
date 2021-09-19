using System.Linq;
using UnityEngine;
using UniRx;
using AudioUtilityToolkit.SoundIOExtension;
using AudioUtilityToolkit.UnityAudioExtension;

namespace AgoraExtension.Samples
{
    public class MicrophonePresenter : MonoBehaviour
    {
        [SerializeField] MicrophoneControlView _microphoneControlView;
        [SerializeField] UnityAudioOut _loopbackAudioOut;
        [SerializeField] AgoraClientContext _agoraContext;
        
        private UnityMicrophone _microphone;
        private InputStream _inputStream;
        
        private void Awake()
        {
            var inputDeviceList = AudioDeviceDriver.InputDeviceList.Select(device => device.DeviceName).ToList();
            _microphoneControlView.UpdateSelectDropdown(inputDeviceList);

            _microphoneControlView.CurrentDevice
            .SkipLatestValueOnSubscribe()
            .Subscribe(device => 
            {
                if (_inputStream != null) _inputStream.OnProcessFrame -= OnProcessFrame;

                _inputStream = AudioDeviceDriver.GetInputDevice(device);
                _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);

                _inputStream.OnProcessFrame += OnProcessFrame;
            })
            .AddTo(this);
            
            _microphoneControlView.LoopbackIsActive
            .SkipLatestValueOnSubscribe()
            .Subscribe(loopback =>
            {
                if (loopback)
                {
                    _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);
                }
                else
                {
                    _loopbackAudioOut.StopOutput();
                }
            })
            .AddTo(this);
        }
        
        private void OnDestroy()
        {
            _inputStream.OnProcessFrame -= OnProcessFrame;
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
