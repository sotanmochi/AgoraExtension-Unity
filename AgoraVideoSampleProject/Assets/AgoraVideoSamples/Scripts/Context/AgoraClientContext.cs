using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace AgoraExtension.Samples
{
    public class AgoraClientContext : MonoBehaviour
    {
        [SerializeField] AgoraConfig _Config;
        [SerializeField] AgoraJoinParameters _JoinParameters;
        [SerializeField] AgoraClient _Client;

        public bool IsJoined => _Client.IsJoined;

        public IObservable<uint> OnJoinedAsObservable() => _Client.OnJoinedChannelAsObservable();
        public IObservable<Unit> OnLeftAsObservable() => _Client.OnLeftChannelAsObservable();

        public IReadOnlyReactiveDictionary<uint, string> RemoteUsers => _RemoteUsers;
        private ReactiveDictionary<uint, string> _RemoteUsers = new ReactiveDictionary<uint, string>();

        public int VideoWidth => _JoinParameters.VideoWidth;
        public int VideoHeight => _JoinParameters.VideoHeight;

        private List<Device> _videoDeviceList;

        void Awake()
        {
            _Client.OnUserJoinedAsObservable()
            .Subscribe(userId => 
            {
                _RemoteUsers.Add(userId, "User-" + userId);
            })
            .AddTo(this);

            _Client.OnUserLeftAsObservable()
            .Subscribe(userId => 
            {
                _RemoteUsers.Remove(userId);
            })
            .AddTo(this);
        }

        public async UniTask<bool> Join(string channelName)
        {
            if (!_Client.IsInitialized)
            {
                await _Client.Initialize(_Config);
            }

            _JoinParameters.ChannelName = channelName;
            return await _Client.Join(_JoinParameters);
        }

        public void Leave()
        {
            _Client.Leave();
        }

        public List<string> GetVideoDevices()
        {
            _videoDeviceList = _Client.GetVideoDevices();
            return _videoDeviceList.Select(device => device.Name).ToList();
        }

        public void SetVideoDevice(int index)
        {
            _Client.SetVideoDevice(_videoDeviceList[index].ID);
        }

        public void SetExternalVideoSource(bool useExternalVideoSource)
        {
            _JoinParameters.UseExternalVideoSource = useExternalVideoSource;
        }

        public void SetVideoResolution(int width, int height)
        {
            _JoinParameters.VideoWidth = width;
            _JoinParameters.VideoHeight = height;
        }

        public void EnableVideo()
        {
            _Client.EnableVideo();
        }

        public void DisableVideo()
        {
            _Client.DisableVideo();
        }

        public void SendAudioFrame(float[] frameData)
        {
            _Client.PushAudioFrame(frameData, _JoinParameters.AudioChannels, _JoinParameters.SampleRate);
        }
    }
}
