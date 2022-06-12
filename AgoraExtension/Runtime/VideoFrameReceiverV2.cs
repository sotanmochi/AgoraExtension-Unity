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

        private uint _senderId;
        private VideoRender _videoRender;

        private PluginTextureRenderer _pluginTextureRenderer;

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

            _pluginTextureRenderer = new PluginTextureRenderer(UpdateRawTextureDataFunction, frameWidth, frameHeight);
            OnUpdateTexture?.Invoke(_pluginTextureRenderer.TargetTexture);

            CustomTextureRenderSystem.Instance.AddRenderer(_pluginTextureRenderer);
        }

        public void Stop()
        {
            _pluginTextureRenderer?.Dispose();
            _pluginTextureRenderer = null;

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
        /// <param name="bytesPerPixel"></param>
        private void UpdateRawTextureDataFunction(IntPtr data, int width, int height, int bytesPerPixel)
        {
            _videoRender.UpdateVideoRawData(_senderId, data, ref width, ref height);
        }
    }    
}
