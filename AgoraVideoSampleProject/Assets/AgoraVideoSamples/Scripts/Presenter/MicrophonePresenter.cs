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
            _microphone = new UnityMicrophone();

            // var inputDeviceList = UnityEngine.Microphone.devices.ToList();
            // _microphoneControlView.UpdateSelectDropdown(inputDeviceList);

            var inputDeviceList = AudioDeviceDriver.InputDeviceList.Select(device => device.DeviceName).ToList();
            _microphoneControlView.UpdateSelectDropdown(inputDeviceList);

            _microphoneControlView.CurrentDevice
            .SkipLatestValueOnSubscribe()
            .Subscribe(device => 
            {
                // _microphone.Stop();
                // _microphone.Start(device);

                _inputStream = AudioDeviceDriver.GetInputDevice(device);
                _inputStream.OnProcessFrame += OnProcessFrame;                _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);
                _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);
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
            // _microphone.OnProcessFrame -= OnProcessFrame;
            // _microphone.Dispose();
            _inputStream.OnProcessFrame -= OnProcessFrame;
            _inputStream.Dispose();
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
