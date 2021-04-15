using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class StreamerControlView : MonoBehaviour
    {
        [SerializeField] Button _StartStreaming;
        [SerializeField] Button _StopStreaming;

        public IObservable<Unit> OnTriggeredStartStreamingEventAsObservable() => _StartStreamingTrigger;
        private Subject<Unit> _StartStreamingTrigger = new Subject<Unit>();

        public IObservable<Unit> OnTriggeredStopStreamingEventAsObservable() => _StopStreamingTrigger;
        private Subject<Unit> _StopStreamingTrigger = new Subject<Unit>();

        void Awake()
        {
            _StartStreaming.OnClickAsObservable()
            .Subscribe(_ => 
            {
                _StartStreamingTrigger.OnNext(Unit.Default);
            })
            .AddTo(this);

            _StopStreaming.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _StopStreamingTrigger.OnNext(Unit.Default);
            })
            .AddTo(this);
        }
    }
}
