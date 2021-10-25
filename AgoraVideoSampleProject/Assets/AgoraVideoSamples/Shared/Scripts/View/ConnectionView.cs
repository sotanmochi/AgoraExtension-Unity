using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] InputField _ChannelId;
        [SerializeField] Button _JoinChannel;
        [SerializeField] Button _LeaveChannel;
        [SerializeField] Text _UserId;

        public IObservable<string> OnTriggeredJoinEventAsObservable() => _OnTriggerJoinSubject;
        private Subject<string> _OnTriggerJoinSubject = new Subject<string>();

        public IObservable<Unit> OnTriggeredLeaveEventAsObservable() => _OnTriggerLeaveSubject;
        private Subject<Unit> _OnTriggerLeaveSubject = new Subject<Unit>();

        void Awake()
        {
            _JoinChannel.OnClickAsObservable()
            .Subscribe(_ => 
            {
                _OnTriggerJoinSubject.OnNext(_ChannelId.text);
            })
            .AddTo(this);

            _LeaveChannel.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _OnTriggerLeaveSubject.OnNext(Unit.Default);
            })
            .AddTo(this);
        }

        public void SetUserId(string userId)
        {
            _UserId.text = userId;
        }
    }
}
