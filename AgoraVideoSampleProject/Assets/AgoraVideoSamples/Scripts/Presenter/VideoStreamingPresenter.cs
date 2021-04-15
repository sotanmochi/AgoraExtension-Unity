using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class VideoStreamingPresenter : MonoBehaviour
    {
        [SerializeField] WebCamSelectView _WebCamSelectView;
        [SerializeField] RawImage _LocalPreviewImage;
        [SerializeField] StreamerControlView _ControlView;
        [SerializeField] VideoFrameStreamer _VideoFrameStreamer;

        private WebCamTexture _WebCamTexture;

        void Awake()
        {
            _WebCamSelectView.OnSelectedWebCamAsObservable()
            .Subscribe(device => 
            {
                if (_WebCamTexture != null)
                {
                    _WebCamTexture.Stop();
                    _WebCamTexture = null;
                }

                _WebCamTexture = new WebCamTexture(device.name);
                _WebCamTexture.Play();

                _LocalPreviewImage.texture = _WebCamTexture;
                _VideoFrameStreamer.SetStreamingSource(_WebCamTexture);
                _VideoFrameStreamer.SetPixelFormat(VideoPixelFormat.ARGB32);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStartStreamingEventAsObservable()
            .Subscribe(_ => 
            {
                _VideoFrameStreamer.StartStreaming();
            })
            .AddTo(this);

            _ControlView.OnTriggeredStopStreamingEventAsObservable()
            .Subscribe(_ => 
            {
                _VideoFrameStreamer.StopStreaming();
            })
            .AddTo(this);
        }
    }
}
