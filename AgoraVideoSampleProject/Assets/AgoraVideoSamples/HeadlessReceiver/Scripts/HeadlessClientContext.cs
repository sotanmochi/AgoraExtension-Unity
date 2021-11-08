using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace AgoraExtension.Samples.HeadlessReceiver
{
    public class HeadlessClientContext : MonoBehaviour
    {
        [SerializeField] AgoraClientContext _agoraClientContext;
        [SerializeField] VideoFrameReceiver _videoFrameReceiver;
        [SerializeField] HeadlessClientConfig _config;

        private string _channelName;
        private ulong _streamerId;

        private ulong _receivedFrameCount;
        private int _videoFrameWidth;
        private int _videoFrameHeight;

        private async void Start()
        {
            _channelName = _config.Channel;
            _streamerId = _config.StreamerId;

#if UNITY_EDITOR
            if (!_config.EnableEmulationInEditor)
            {
                return;
            }
#endif

#if UNITY_SERVER
            ReadCommandLineArgs();
#endif

#if UNITY_EDITOR || UNITY_SERVER
            StartUp();
            await _agoraClientContext.Join(_channelName, 0);
            _videoFrameReceiver.StartReceiving(_streamerId, _agoraClientContext.VideoWidth, _agoraClientContext.VideoHeight);
#endif
        }

        private void OnDestroy()
        {
            _videoFrameReceiver.StopReceiving();
            _agoraClientContext.Leave();
        }

        private void StartUp()
        {
            _videoFrameReceiver.OnReceivedVideoFrame()
            .TakeUntilDestroy(this)
            .Subscribe(texture => 
            {
                _receivedFrameCount++;
                if (texture.width != _videoFrameWidth || texture.height != _videoFrameHeight)
                {
                    _videoFrameWidth = texture.width;
                    _videoFrameHeight = texture.height;
                    ConsoleLog($"***** Received video frame size: {texture.width}x{texture.height} *****");
                }
            });

            Observable.Interval(TimeSpan.FromSeconds(5))
            .TakeUntilDestroy(this)
            .Subscribe(_ => 
            {
                ConsoleLog($"Received video frame count: {_receivedFrameCount}");
            });
        }

        private void ReadCommandLineArgs()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--channel")
                {
                    _channelName = args[i + 1];
                }
                if (args[i] == "--streamerid")
                {
                    if (ulong.TryParse(args[i + 1], out var value))
                    {
                        _streamerId = value;
                    }
                }
            }
        }

        private void ConsoleLog(string message)
        {
#if UNITY_SERVER
            {
                Console.WriteLine();
                Console.WriteLine(message);
            }
#else
            {
                Debug.Log(message);
            }
#endif
        }

    }
}
