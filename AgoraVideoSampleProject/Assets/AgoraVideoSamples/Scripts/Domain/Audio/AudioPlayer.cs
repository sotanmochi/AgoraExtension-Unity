using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgoraExtension.Samples
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _AudioSource;
        [SerializeField] AudioPlaylist _Playlist;

        public void Play(int index = 0)
        {
            _AudioSource.Stop();
            _AudioSource.clip = _Playlist.AudioClips[index];
            _AudioSource.Play();
        }

        public void Stop()
        {
            _AudioSource.Stop();
        }
    }
}
