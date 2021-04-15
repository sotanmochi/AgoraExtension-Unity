using System.Collections.Generic;
using UnityEngine;

namespace AgoraExtension.Samples
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Agora Extension/Samples/Create Audio Playlist", fileName = "AudioPlaylist")]
    public class AudioPlaylist : ScriptableObject
    {
        public List<AudioClip> AudioClips = new List<AudioClip>();
    }
}
