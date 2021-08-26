using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class MicrophoneControlView : MonoBehaviour
    {
        [SerializeField] private Dropdown _selectDropdown;
        [SerializeField] private Toggle _loopbackToggle;
        
        public IReadOnlyReactiveProperty<string> CurrentDevice => _currentItem;
        private ReactiveProperty<string> _currentItem = new ReactiveProperty<string>();
        
        public IReadOnlyReactiveProperty<bool> LoopbackIsActive => _loopbackIsActive;
        private ReactiveProperty<bool> _loopbackIsActive = new ReactiveProperty<bool>(true);
        
        private string[] _dropdownItems;
        private string _dropdownMessage = "Select Microphone";
        
        private void Awake()
        {
            _dropdownItems = Microphone.devices;
            InitializeSelectDropdown();
            
            _selectDropdown.OnValueChangedAsObservable()
            .Subscribe(selectedIndex => 
            {
                if (selectedIndex < 1)
                {
                    return;
                }
                _currentItem.Value = _dropdownItems[selectedIndex - 1];
            })
            .AddTo(this);
            
            _loopbackToggle.OnValueChangedAsObservable()
            .Subscribe(value => _loopbackIsActive.Value = value)
            .AddTo(this);
        }
        
        private void InitializeSelectDropdown()
        {
            _selectDropdown.ClearOptions();
            _selectDropdown.RefreshShownValue();
            _selectDropdown.options.Add(new Dropdown.OptionData { text = _dropdownMessage });
            
            foreach(var item in _dropdownItems)
            {
                _selectDropdown.options.Add(new Dropdown.OptionData { text = item });
            }
            
            _selectDropdown.RefreshShownValue();
        }
    }
}