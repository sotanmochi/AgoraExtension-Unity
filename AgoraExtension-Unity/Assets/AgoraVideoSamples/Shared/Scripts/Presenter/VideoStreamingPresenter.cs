using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class VideoStreamingPresenter : MonoBehaviour
    {
        [SerializeField] bool _UseUnityWebCam;
        [SerializeField] WebCamSelectView _WebCamSelectView;
        [SerializeField] RawImage _WebCamImage;
        [SerializeField] StreamerControlView _ControlView;
        [SerializeField] VideoFrameStreamer _VideoFrameStreamer;
        [SerializeField] AgoraClientContext _Context;
        private WebCamTexture _WebCamTexture;

        void Awake()
        {
            if (_UseUnityWebCam)
            {
                InitializeUnityWebCamStreaming();
            }
            else
            {
                InitializeAgoraNativeVideoStreaming();
            }
        }

        private void InitializeAgoraNativeVideoStreaming()
        {
            _Context.SetExternalVideoSource(false);

            _Context.OnJoinedAsObservable()
            .Subscribe(_ => 
            {
                var devices = _Context.GetVideoDevices();
                _WebCamSelectView.UpdateSelectDropdown(devices);
            })
            .AddTo(this);

            _WebCamSelectView.OnSelectedWebCamAsObservable()
            .Subscribe(device => 
            {
                int deviceIndex = device.Index;
                _Context.SetVideoDevice(deviceIndex);
            })
            .AddTo(this);

            _ControlView.OnTriggeredStartStreamingEventAsObservable()
            .Subscribe(_ => 
            {
                _Context.EnableVideo();
            })
            .AddTo(this);

            _ControlView.OnTriggeredStopStreamingEventAsObservable()
            .Subscribe(_ => 
            {
                _Context.DisableVideo();
            })
            .AddTo(this);
        }

        private void InitializeUnityWebCamStreaming()
        {
            _Context.SetExternalVideoSource(true);

            var devices = WebCamTexture.devices.Select(device => device.name).ToList();
            _WebCamSelectView.UpdateSelectDropdown(devices);

            _WebCamSelectView.OnSelectedWebCamAsObservable()
            .Subscribe(device => 
            {
                if (_WebCamTexture != null)
                {
                    _WebCamTexture.Stop();
                    _WebCamTexture = null;
                }

                _WebCamTexture = new WebCamTexture(device.Name);
                _WebCamTexture.Play();

                _WebCamImage.gameObject.SetActive(true);
                _WebCamImage.texture = _WebCamTexture;
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
