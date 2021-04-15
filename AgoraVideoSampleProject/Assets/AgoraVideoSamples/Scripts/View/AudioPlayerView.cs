using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class AudioPlayerView : MonoBehaviour
    {
        [SerializeField] Button _Play;
        [SerializeField] Button _Stop;

        public IObservable<Unit> OnTriggeredPlayAudioAsObservable() => _OnTriggerPlaySubject;
        private Subject<Unit> _OnTriggerPlaySubject = new Subject<Unit>();

        public IObservable<Unit> OnTriggeredStopAudioAsObservable() => _OnTriggerStopSubject;
        private Subject<Unit> _OnTriggerStopSubject = new Subject<Unit>();

        void Awake()
        {
            _Play.OnClickAsObservable()
            .Subscribe(_ => 
            {
                _OnTriggerPlaySubject.OnNext(Unit.Default);
            })
            .AddTo(this);

            _Stop.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _OnTriggerStopSubject.OnNext(Unit.Default);
            })
            .AddTo(this);
        }
    }
}
