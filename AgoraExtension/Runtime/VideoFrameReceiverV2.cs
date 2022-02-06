// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UniRx;
using agora_gaming_rtc;
using UnityCustomTextureRenderer;

namespace AgoraExtension
{
    public sealed class VideoFrameReceiverV2
    {
        public event Action<Texture> OnUpdateTexture;

        private IDisposable _disposable;

        private uint _senderId;
        private VideoRender _videoRender;

        private Texture2D _nativeTexture;
        private CustomTextureRenderer _customTextureRenderer;

        public VideoFrameReceiverV2(bool autoDispose = true)
        {
            if (autoDispose) { UnityEngine.Application.quitting += Stop; }
        }

        public void Start(ulong senderId, int frameWidth, int frameHeight) 
        {
            Stop();

            _senderId = (uint)senderId;

            IRtcEngine engine = IRtcEngine.QueryEngine();
            if (engine != null)
            {
                _videoRender = (VideoRender)engine.GetVideoRender();
                _videoRender.SetVideoRenderMode(VIDEO_RENDER_MODE.RENDER_RAWDATA);
                _videoRender.AddUserVideoInfo(_senderId, 0);
            }

            _nativeTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
            OnUpdateTexture?.Invoke(_nativeTexture);

            _customTextureRenderer = new CustomTextureRenderer(UpdateRawTextureDataFunction, _nativeTexture);

            _disposable = Observable.EveryUpdate().Subscribe(_ => 
            {
                _customTextureRenderer.Update();
            });
        }

        public void Stop()
        {
            _disposable?.Dispose();
            _disposable = null;

            _customTextureRenderer?.Dispose();
            _customTextureRenderer = null;

            if (_nativeTexture != null)
            {
                UnityEngine.Object.Destroy(_nativeTexture);
                _nativeTexture = null;
            }

            if (_videoRender != null && IRtcEngine.QueryEngine() != null)
            {
                _videoRender.RemoveUserVideoInfo(_senderId);
                _senderId = 0;
            }
        }

        /// <summary>
        /// This function is called in Render Thread.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="frameCount"></param>
        private void UpdateRawTextureDataFunction(IntPtr data, int width, int height, uint frameCount)
        {
            _videoRender.UpdateVideoRawData(_senderId, data, ref width, ref height);
        }
    }    
}
