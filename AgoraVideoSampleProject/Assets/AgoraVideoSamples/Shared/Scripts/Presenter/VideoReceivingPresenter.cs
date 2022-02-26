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

        VideoFrameReceiverV2 _frameReceiver = new VideoFrameReceiverV2();

        void Awake()
        {
            // _VideoFrameReceiver.OnReceivedVideoFrame()
            // .Subscribe(texture => 
            // {
            //     _VideoDisplay.texture = texture;
            // })
            // .AddTo(this);

            _frameReceiver.OnUpdateTexture += (texture) =>
            {
                _VideoDisplay.texture = texture;
            };

            _ControlView.OnTriggeredStartReceivingAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Start");
                // _VideoFrameReceiver.StartReceiving(_ControlView.SenderId, _ClientContext.VideoWidth, _ClientContext.VideoHeight);
                _frameReceiver.Start(_ControlView.SenderId, _ClientContext.VideoWidth, _ClientContext.VideoHeight);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStopReceivingAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Stop");
                // _VideoFrameReceiver.StopReceiving();
                _frameReceiver.Stop();
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
