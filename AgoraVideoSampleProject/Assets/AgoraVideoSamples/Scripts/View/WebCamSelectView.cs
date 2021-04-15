using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AgoraExtension.Samples
{
    public class WebCamSelectView : MonoBehaviour
    {
        [SerializeField] Dropdown _WebCamSelectDropdown;

        public IObservable<WebCamDevice> OnSelectedWebCamAsObservable() => _OnSelectedWebCamSubject;
        private Subject<WebCamDevice> _OnSelectedWebCamSubject = new Subject<WebCamDevice>();

        void Awake()
        {
            InitializeWebCamSelectDropdown();

            _WebCamSelectDropdown.OnValueChangedAsObservable()
            .Subscribe(selectedIndex => 
            {
                if (selectedIndex < 1)
                {
                    return;
                }

                _OnSelectedWebCamSubject.OnNext(WebCamTexture.devices[selectedIndex - 1]);
            })
            .AddTo(this);
        }

        private void InitializeWebCamSelectDropdown()
        {
            _WebCamSelectDropdown.ClearOptions();
            _WebCamSelectDropdown.RefreshShownValue();
            _WebCamSelectDropdown.options.Add(new Dropdown.OptionData { text = "Select camera" });

            foreach(var device in WebCamTexture.devices)
            {
                _WebCamSelectDropdown.options.Add(new Dropdown.OptionData { text = device.name });
            }

            _WebCamSelectDropdown.RefreshShownValue();
        }
    }
}
