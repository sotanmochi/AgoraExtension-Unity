using System.Linq;
using UnityEngine;
using UniRx;

namespace AgoraExtension.Samples.BasicBroadcasting360
{
    public class VideoReceivingPresenter : MonoBehaviour
    {
        [SerializeField] Material _material;
        [SerializeField] ReceiverControlView _controlView;
        [SerializeField] AgoraClientContext _clientContext;
        [SerializeField] VideoFrameReceiver _videoFrameReceiver;

        void Awake()
        {
            _videoFrameReceiver.OnReceivedVideoFrame()
            .Subscribe(texture => 
            {
                _material.mainTexture = texture;
            })
            .AddTo(this);

            _controlView.OnTriggeredStartReceivingAsObservable()
            .Subscribe(_ =>
            {
                _videoFrameReceiver.StartReceiving(_controlView.SenderId, _clientContext.VideoWidth, _clientContext.VideoHeight);
            })
            .AddTo(this);

            _controlView.OnTriggeredStopReceivingAsObservable()
            .Subscribe(_ =>
            {
                _videoFrameReceiver.StopReceiving();
            })
            .AddTo(this);


            _clientContext.RemoteUsers.ObserveAdd()
            .Subscribe(user => 
            {
                _controlView.UpdateRemoteUserList(
                    _clientContext.RemoteUsers.Select(kv => (kv.Key, kv.Value)).ToList()
                );
            })
            .AddTo(this);

            _clientContext.RemoteUsers.ObserveRemove()
            .Subscribe(user => 
            {
                _controlView.UpdateRemoteUserList(
                    _clientContext.RemoteUsers.Select(kv => (kv.Key, kv.Value)).ToList()
                );
            })
            .AddTo(this);
        }
    }
}
