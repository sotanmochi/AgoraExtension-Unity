using UnityEngine;
using UniRx;
using AudioUtilityToolkit.UnityAudioExtension;

namespace AgoraExtension.Samples
{
    public class AudioReceivingPresenter : MonoBehaviour
    {
        [SerializeField] UnityAudioOut _AudioOut;
        [SerializeField] ReceiverControlView _ControlView;
        [SerializeField] AgoraClientContext _ClientContext;

        private AudioFrameReceiver _AudioFrameReceiver;

        void Awake()
        {
            _AudioFrameReceiver = new AudioFrameReceiver();

            _AudioFrameReceiver.OnProcessFrameAsObservable()
            .Subscribe(audioFrame =>
            {
                _AudioOut.PushAudioFrame(audioFrame);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStartReceivingAsObservable()
            .Where(_ => _ClientContext.ExternalAudioSink)
            .Subscribe(_ =>
            {
                _AudioFrameReceiver.Start(_ClientContext.SampleRate, _ClientContext.ChannelCount);
                _AudioOut.StartOutput(_ClientContext.ChannelCount, _ClientContext.SampleRate);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStopReceivingAsObservable()
            .Where(_ => _ClientContext.ExternalAudioSink)
            .Subscribe(_ =>
            {
                _AudioFrameReceiver.Stop();
                _AudioOut.StopOutput();
            })
            .AddTo(this);
        }
    }
}
