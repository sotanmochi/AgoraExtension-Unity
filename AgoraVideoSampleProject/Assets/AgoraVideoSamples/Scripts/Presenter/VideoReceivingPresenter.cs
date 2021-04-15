using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class VideoReceivingPresenter : MonoBehaviour
    {
        [SerializeField] RawImage _VideoDisplay;
        [SerializeField] ReceiverControlView _ControlView;
        [SerializeField] AgoraClientContext _ClientContext;
        [SerializeField] VideoFrameReceiver _VideoFrameReceiver;

        void Awake()
        {
            _VideoFrameReceiver.OnReceivedVideoFrame()
            .Subscribe(texture => 
            {
                _VideoDisplay.texture = texture;
            })
            .AddTo(this);

            _ControlView.OnTriggeredStartReceivingAsObservable()
            .Subscribe(_ =>
            {
                _VideoFrameReceiver.StartReceiving(_ControlView.SenderId, _ClientContext.VideoWidth, _ClientContext.VideoHeight);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStopReceivingAsObservable()
            .Subscribe(_ =>
            {
                _VideoFrameReceiver.StopReceiving();
            })
            .AddTo(this);


            _ClientContext.RemoteUsers.ObserveAdd()
            .Subscribe(user => 
            {
                _ControlView.UpdateRemoteUserList(
                    _ClientContext.RemoteUsers.Select(kv => (kv.Key, kv.Value)).ToList()
                );
            })
            .AddTo(this);

            _ClientContext.RemoteUsers.ObserveRemove()
            .Subscribe(user => 
            {
                _ControlView.UpdateRemoteUserList(
                    _ClientContext.RemoteUsers.Select(kv => (kv.Key, kv.Value)).ToList()
                );
            })
            .AddTo(this);
        }
    }
}
