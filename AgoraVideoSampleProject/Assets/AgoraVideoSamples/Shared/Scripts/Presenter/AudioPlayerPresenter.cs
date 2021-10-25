using UnityEngine;
using UniRx;

namespace AgoraExtension.Samples
{
    public class AudioPlayerPresenter : MonoBehaviour
    {
        [SerializeField] AudioPlayer _AudioPlayer;
        [SerializeField] AudioPlayerView _View;

        void Awake()
        {
            _View.OnTriggeredPlayAudioAsObservable()
            .Subscribe(_ => 
            {
                _AudioPlayer.Play();
            })
            .AddTo(this);

            _View.OnTriggeredStopAudioAsObservable()
            .Subscribe(_ => 
            {
                _AudioPlayer.Stop();
            })
            .AddTo(this);
        }
    }
}
