using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] InputField _ChannelId;
        [SerializeField] InputField _UserIdInput;
        [SerializeField] Button _JoinChannel;
        [SerializeField] Button _LeaveChannel;
        [SerializeField] Text _UserId;

        public IObservable<(string ChannelId, string UserId)> OnTriggeredJoinEventAsObservable() => _OnTriggerJoinSubject;
        private Subject<(string ChannelId, string UserId)> _OnTriggerJoinSubject = new Subject<(string ChannelId, string UserId)>();

        public IObservable<Unit> OnTriggeredLeaveEventAsObservable() => _OnTriggerLeaveSubject;
        private Subject<Unit> _OnTriggerLeaveSubject = new Subject<Unit>();

        void Awake()
        {
            _JoinChannel.OnClickAsObservable()
            .Subscribe(_ => 
            {
                _OnTriggerJoinSubject.OnNext((_ChannelId.text, _UserIdInput.text));
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
