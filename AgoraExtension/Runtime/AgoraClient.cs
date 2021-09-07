// Copyright (c) 2021 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
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
        private IVideoDeviceManager _VideoDeviceManager;

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

            _VideoDeviceManager = _RtcEngine.GetVideoDeviceManager();

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
            _RtcEngine.SetExternalAudioSource(true, joinParameters.SampleRate, joinParameters.AudioChannels);
            _RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY_STEREO, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_SHOWROOM);
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

            _RtcEngine.SetExternalVideoSource(joinParameters.UseExternalVideoSource);

            _RtcEngine.EnableVideoObserver();
            _RtcEngine.EnableVideo();
            _VideoDeviceManager.CreateAVideoDeviceManager();

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
            _VideoDeviceManager.ReleaseAVideoDeviceManager();
        }

        public string GetCurrentVideoDevice()
        {
            string deviceID = "Unknown";
            _VideoDeviceManager.GetCurrentVideoDevice(ref deviceID);
            return deviceID;
        }

        public List<Device> GetVideoDevices()
        {
            List<Device> devices = new List<Device>();

            int count = _VideoDeviceManager.GetVideoDeviceCount();
            for (int i = 0; i < count; i++)
            {
                var device = new Device(){ Index = i };
                _VideoDeviceManager.GetVideoDevice(i, ref device.Name, ref device.ID);
                devices.Add(device);
            }

            return devices;
        }

        public void SetVideoDevice(string deviceId)
        {
            _VideoDeviceManager.SetVideoDevice(deviceId);
        }

        public void EnableVideo()
        {
            _RtcEngine.EnableVideo();
        }

        public void DisableVideo()
        {
            _RtcEngine.DisableVideo();
        }

        public void PushVideoFrame(ExternalVideoFrame externalVideoFrame)
        {
            _RtcEngine.PushVideoFrame(externalVideoFrame);            
        }

        public void PushAudioFrame(AudioFrame audioFrame)
        {
            _RtcEngine.PushAudioFrame(audioFrame);
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void PushAudioFrame(float[] pcm, int channels = 1, int samplingRate = 48000)
        {
            long currentTimeMillis = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

            // Convert 32bit float PCM to 16bit PCM data bytes.
            var dataStream = new MemoryStream();
            for (int i = 0; i < pcm.Length; i++)
            {
                dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(pcm[i] * Int16.MaxValue)), 0, sizeof(Int16));
            }
            var buffer = dataStream.ToArray();

            var audioFrame = new AudioFrame()
            {
                type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16,
                bytesPerSample = 2, // PCM16
                buffer = buffer,
                channels = channels,
                samples = pcm.Length / channels,
                samplesPerSec = samplingRate,
                renderTimeMs = currentTimeMillis,
            };

            _RtcEngine.PushAudioFrame(audioFrame);
        }

        private bool LoadEngine(string appId, AREA_CODE areaCode)
        {
            UnloadEngine();

            _RtcEngine = IRtcEngine.GetEngine(new RtcEngineConfig(appId, new LogConfig(), areaCode));
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
