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

        VideoFrameReceiverV2 _frameReceiver = new VideoFrameReceiverV2();

        void Awake()
        {
        //     _videoFrameReceiver.OnReceivedVideoFrame()
        //     .Subscribe(texture => 
        //     {
        //         _material.mainTexture = texture;
        //         _controlView.UpdateResolutionText(texture.width, texture.height);
        //     })
        //     .AddTo(this);

            _frameReceiver.OnUpdateTexture += (texture) =>
            {
                _material.mainTexture = texture;
                _controlView.UpdateResolutionText(texture.width, texture.height);
            };

            _controlView.OnTriggeredStartReceivingAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Start");
                // _videoFrameReceiver.StartReceiving(_controlView.SenderId, _clientContext.VideoWidth, _clientContext.VideoHeight);
                _frameReceiver.Start(_controlView.SenderId, _clientContext.VideoWidth, _clientContext.VideoHeight);
            })
            .AddTo(this);

            _controlView.OnTriggeredStopReceivingAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Stop");
                // _videoFrameReceiver.StopReceiving();
                _frameReceiver.Stop();
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
