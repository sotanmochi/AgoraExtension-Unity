// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using agora_gaming_rtc;

namespace AgoraExtension
{
    public class VideoFrameReceiver : MonoBehaviour
    {
        public IObservable<Texture2D> OnReceivedVideoFrame() => _OnReceivedVideoFrame;
        private Subject<Texture2D> _OnReceivedVideoFrame = new Subject<Texture2D>();

        private bool _IsInitialized;
        private uint _SenderId;

        private Texture2D _FrameTexture;
        private int _FrameWidth;
        private int _FrameHeight;

        void OnDestroy()
        {
            StopReceiving();
        }

        public void StartReceiving(ulong senderId, int frameWidth, int frameHeight) 
        {
            _SenderId = (uint)senderId;
            _FrameWidth = frameWidth;
            _FrameHeight = frameHeight;

            _FrameTexture = new Texture2D(_FrameWidth, _FrameHeight, TextureFormat.RGBA32, false);

            if (!_IsInitialized)
            {
                Initialize();
            }
        }

        public void StopReceiving()
        {
            _SenderId = 0;
        }

        private void Initialize()
        {
            IRtcEngine engine = IRtcEngine.QueryEngine();

            if (engine != null)
            {
                var videoRawDataManager = VideoRawDataManager.GetInstance(engine);
                videoRawDataManager.SetOnRenderVideoFrameCallback(OnRenderVideoFrameHandler);
                _IsInitialized = true;
            }
            else
            {
                _IsInitialized = false;
            }
        }

        private async void OnRenderVideoFrameHandler(uint uid, VideoFrame videoFrame)
        {
            if (uid == _SenderId)
            {
                int width = videoFrame.width;
                int height = videoFrame.height;
                byte[] data = videoFrame.buffer;

                await UniTask.Yield(PlayerLoopTiming.Update);

                try
                {
                    if (width == _FrameWidth  && height == _FrameHeight)
                    {
                        _FrameTexture.LoadRawTextureData(data);
                        _FrameTexture.Apply();
                    }
                    else
                    {
                        _FrameWidth = width;
                        _FrameHeight = height;
                        _FrameTexture.Resize(_FrameWidth, _FrameHeight);
                        _FrameTexture.LoadRawTextureData(data);
                        _FrameTexture.Apply();
                    }

                    _OnReceivedVideoFrame.OnNext(_FrameTexture);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }    
}
