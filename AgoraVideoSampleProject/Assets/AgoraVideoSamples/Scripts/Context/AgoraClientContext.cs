using System;
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

        public IObservable<uint> OnJoinedAsObservable() => _Client.OnJoinedChannelAsObservable();
        public IObservable<Unit> OnLeftAsObservable() => _Client.OnLeftChannelAsObservable();

        public IReadOnlyReactiveDictionary<uint, string> RemoteUsers => _RemoteUsers;
        private ReactiveDictionary<uint, string> _RemoteUsers = new ReactiveDictionary<uint, string>();

        public int VideoWidth => _JoinParameters.VideoWidth;
        public int VideoHeight => _JoinParameters.VideoHeight;

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

        public void SetVideoResolution(int width, int height)
        {
            _JoinParameters.VideoWidth = width;
            _JoinParameters.VideoHeight = height;
        }
    }
}
