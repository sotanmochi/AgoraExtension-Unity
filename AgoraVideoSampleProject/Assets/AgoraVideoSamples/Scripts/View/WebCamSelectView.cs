using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class WebCamSelectView : MonoBehaviour
    {
        [SerializeField] Dropdown _selectDropdown;

        public IObservable<(int Index, string Name)> OnSelectedWebCamAsObservable() => _OnSelectedWebCamSubject;
        private Subject<(int Index, string Name)> _OnSelectedWebCamSubject = new Subject<(int Index, string Name)>();

        private string[] _dropdownItems;
        private string _dropdownMessage = "Select camera";

        void Awake()
        {
            _selectDropdown.OnValueChangedAsObservable()
            .Subscribe(selectedIndex => 
            {
                if (selectedIndex < 1)
                {
                    return;
                }

                int index = selectedIndex - 1;
                _OnSelectedWebCamSubject.OnNext((index, _dropdownItems[index]));
            })
            .AddTo(this);
        }
        
        public void UpdateSelectDropdown(List<string> itemList)
        {
            _dropdownItems = itemList.ToArray();

            _selectDropdown.ClearOptions();
            _selectDropdown.RefreshShownValue();
            _selectDropdown.options.Add(new Dropdown.OptionData { text = _dropdownMessage });
            
            foreach(var item in itemList)
            {
                _selectDropdown.options.Add(new Dropdown.OptionData { text = item });
            }
            
            _selectDropdown.RefreshShownValue();
        }
    }
}
