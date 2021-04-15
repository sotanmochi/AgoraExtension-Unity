// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using UniRx;
using agora_gaming_rtc;

namespace AgoraExtension
{
    public enum VideoPixelFormat
    {
        RGBA32,
        ARGB32
    }

    public class VideoFrameStreamer : MonoBehaviour
    {
        private Texture _SourceTexture;
        private ExternalVideoFrame.VIDEO_PIXEL_FORMAT _VideoPixelFormat = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA; // RGBA32
        private long _Frame;
        private IDisposable _Disposable;

        void OnDestroy()
        {
            StopStreaming();
        }

        public void SetStreamingSource(Texture texture)
        {
            _SourceTexture = texture;
        }

        public void SetPixelFormat(VideoPixelFormat format)
        {
            switch (format)
            {
                case VideoPixelFormat.ARGB32:
                    _VideoPixelFormat = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_ARGB; // ARGB32
                    break;
                case VideoPixelFormat.RGBA32:
                    _VideoPixelFormat = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA; // RGBA32
                    break;
                default:
                    _VideoPixelFormat = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA; // RGBA32
                    break;
            }
        }

        public bool StartStreaming(int fps = 30)
        {
            if (_SourceTexture == null)
            {
                return false;
            }

            _Frame = 0;
            _Disposable = Observable.Interval(TimeSpan.FromMilliseconds(1000.0f/fps))
            .Subscribe(async(_) => 
            {
                SendVideoFrame();
            });

            return true;
        }

        public void StopStreaming()
        {
            _Disposable?.Dispose();
        }

        private async void SendVideoFrame()
        {
            await UniTask.WaitForEndOfFrame();

            var request = AsyncGPUReadback.Request(_SourceTexture, 0);
            await UniTask.WaitUntil(() => request.done);

            var rawByteArray = request.GetData<byte>().ToArray();

            // Checks whether the IRtcEngine instance is existed.
            IRtcEngine engine = IRtcEngine.QueryEngine();
            if (engine != null)
            {
                // Creates a new external video frame.
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                // Sets the buffer type of the video frame.
                externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                // Sets the format of the video pixel.
                externalVideoFrame.format = _VideoPixelFormat;
                // Applies raw data.
                externalVideoFrame.buffer = rawByteArray;
                // Sets the width (pixel) of the video frame.
                externalVideoFrame.stride = _SourceTexture.width;
                // Sets the height (pixel) of the video frame.
                externalVideoFrame.height = _SourceTexture.height;
                // Removes pixels from the sides of the frame
                externalVideoFrame.cropLeft = 0;
                externalVideoFrame.cropTop = 0;
                externalVideoFrame.cropRight = 0;
                externalVideoFrame.cropBottom = 0;
                // Rotates the video frame (0, 90, 180, or 270)
                externalVideoFrame.rotation = 0;
                // Increments with the video timestamp.
                externalVideoFrame.timestamp = _Frame++;
                // Pushes the external video frame with the frame you create.
                engine.PushVideoFrame(externalVideoFrame);
            }
        }
    }
}
