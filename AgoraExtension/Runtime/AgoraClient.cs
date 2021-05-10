// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using agora_gaming_rtc;

namespace AgoraExtension
{
    public class AgoraClient : MonoBehaviour
    {
        public IObservable<Unit> OnInitializeAsObservable() => _OnInitialized;
        private Subject<Unit> _OnInitialized = new Subject<Unit>();

        public IObservable<uint> OnJoinedChannelAsObservable() => _OnJoinedChannel;
        private Subject<uint> _OnJoinedChannel = new Subject<uint>();

        public IObservable<Unit> OnLeftChannelAsObservable() => _OnLeftChannel;
        private Subject<Unit> _OnLeftChannel = new Subject<Unit>();

        public IObservable<uint> OnUserJoinedAsObservable() => _OnUserJoined;
        private Subject<uint> _OnUserJoined = new Subject<uint>();

        public IObservable<uint> OnUserLeftAsObservable() => _OnUserLeft;
        private Subject<uint> _OnUserLeft = new Subject<uint>();

        public bool IsInitialized => _IsInitialized;
        private bool _IsInitialized;

        public bool IsJoined => _IsJoined;
        private bool _IsJoined;

        private IRtcEngine _RtcEngine;

        void OnDestroy()
        {
            Uninitialize();
        }

        public async UniTask<bool> Initialize(AgoraConfig config)
        {
            if (_IsInitialized)
            {
                return true;
            }

            bool success = LoadEngine(config.AppId, config.AreaCode);
            if (!success)
            {
                return _IsInitialized = false;
            }

            _RtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
            _RtcEngine.OnUserJoined += OnUserJoined;
            _RtcEngine.OnUserOffline += OnUserOffline;
            _RtcEngine.OnLeaveChannel += OnLeaveChannel;

            _OnInitialized.OnNext(Unit.Default);
            return _IsInitialized = true;
        }

        public void Uninitialize()
        {
            _IsInitialized = false;

            if (_RtcEngine != null)
            {
                _RtcEngine.OnJoinChannelSuccess -= OnJoinChannelSuccess;
                _RtcEngine.OnUserJoined -= OnUserJoined;
                _RtcEngine.OnUserOffline -= OnUserOffline;
                _RtcEngine.OnLeaveChannel -= OnLeaveChannel;
            }
 
            UnloadEngine();
        }

        public async UniTask<bool> Join(AgoraJoinParameters joinParameters, int timeoutSeconds = 30)
        {
            if (_IsJoined)
            {
                return true;
            }

            _RtcEngine.SetChannelProfile(joinParameters.ChannelProfile);
            _RtcEngine.SetClientRole(joinParameters.ClientRoleType);

            // Audio
            // _RtcEngine.DisableAudio();
            // _RtcEngine.MuteLocalAudioStream(true);
            // _RtcEngine.EnableLocalAudio(false);
            _RtcEngine.SetEnableSpeakerphone(true);

            // Video
            var config = new VideoEncoderConfiguration()
            {
                dimensions = new VideoDimensions()
                { 
                    width = joinParameters.VideoWidth, 
                    height = joinParameters.VideoHeight
                },
                frameRate = joinParameters.FrameRate,
            };
            _RtcEngine.SetVideoEncoderConfiguration(config);
            _RtcEngine.SetExternalVideoSource(true);

            _RtcEngine.EnableVideoObserver();
            _RtcEngine.EnableVideo();

            // Join channel
            _RtcEngine.JoinChannel(joinParameters.ChannelName);
            await UniTask.WaitUntil(() => _IsJoined).TimeoutWithoutException(TimeSpan.FromSeconds(timeoutSeconds));

            return _IsJoined;
        }

        public void Leave()
        {
            _IsJoined = false;

            if (_RtcEngine == null)
            {
                return;
            }

            _RtcEngine.LeaveChannel();
            _RtcEngine.DisableVideo();
            _RtcEngine.DisableVideoObserver();
        }

        public void PushVideoFrame(ExternalVideoFrame externalVideoFrame)
        {
            _RtcEngine.PushVideoFrame(externalVideoFrame);            
        }

        private bool LoadEngine(string appId, AREA_CODE areaCode)
        {
            UnloadEngine();

            _RtcEngine = IRtcEngine.GetEngine(new RtcEngineConfig(appId, areaCode));
            if (_RtcEngine == null)
            {
                return false;
            }

            _RtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
            return true;
        }

        private void UnloadEngine()
        {
            if (_RtcEngine != null)
            {
                IRtcEngine.Destroy();
                _RtcEngine = null;
            }
        }

#region Engine callbacks

        private void OnJoinChannelSuccess(string channelName, uint userId, int elapsed)
        {
            _IsJoined = true;
            _RtcEngine.SetAudioSessionOperationRestriction(AUDIO_SESSION_OPERATION_RESTRICTION.AUDIO_SESSION_OPERATION_RESTRICTION_ALL);
            _OnJoinedChannel.OnNext(userId);
        }

        private void OnLeaveChannel(RtcStats stats)
        {
            _OnLeftChannel.OnNext(Unit.Default);
        }

        private void OnUserJoined(uint userId, int elapsed)
        {
            _OnUserJoined.OnNext(userId);
        }

        private void OnUserOffline(uint userId, USER_OFFLINE_REASON reason)
        {
            _OnUserLeft.OnNext(userId);
        }

#endregion

    }
}
