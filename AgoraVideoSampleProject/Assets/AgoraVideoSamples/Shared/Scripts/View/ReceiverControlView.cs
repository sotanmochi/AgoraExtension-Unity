using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class ReceiverControlView : MonoBehaviour
    {
        [SerializeField] Dropdown _RemoteUserList;
        [SerializeField] Button _StartReceiving;
        [SerializeField] Button _StopReceiving;
        [SerializeField] Text _resolutionText;

        public uint SenderId => _SenderId;
        private uint _SenderId;

        public IObservable<uint> OnTriggeredStartReceivingAsObservable() => _StartReceivingTrigger;
        private Subject<uint> _StartReceivingTrigger = new Subject<uint>();

        public IObservable<Unit> OnTriggeredStopReceivingAsObservable() => _StopReceivingTrigger;
        private Subject<Unit> _StopReceivingTrigger = new Subject<Unit>();

        private Dictionary<uint, (uint userId, string displayName)> _RemoteUserDictionary = new Dictionary<uint, (uint userId, string displayName)>();

        void Awake()
        {
            _RemoteUserList.ClearOptions();

            _RemoteUserList.OnValueChangedAsObservable()
            .Skip(1)
            .Subscribe(index => 
            {
                if (_RemoteUserDictionary.TryGetValue((uint)index, out var selectedUserInfo))
                {
                   _SenderId = selectedUserInfo.userId;
                }
            })
            .AddTo(this);

            _StartReceiving.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _StartReceivingTrigger.OnNext(_SenderId);
            })
            .AddTo(this);

            _StopReceiving.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _StopReceivingTrigger.OnNext(Unit.Default);
            })
            .AddTo(this);
        }

        public void UpdateRemoteUserList(IReadOnlyList<(uint userId, string displayName)> list)
        {
            _RemoteUserList.ClearOptions();
            _RemoteUserDictionary.Clear();

            foreach (var item in list.Select((value, index) => new { value, index }))
            {
                _RemoteUserList.options.Insert(item.index, new Dropdown.OptionData(item.value.displayName));
                _RemoteUserDictionary.Add((uint)item.index, item.value);
            }

            _RemoteUserList.value = list.Count;
        }

        public void UpdateResolutionText(int width, int height)
        {
            _resolutionText.text = $"{width}x{height}";
        }
    }
}
