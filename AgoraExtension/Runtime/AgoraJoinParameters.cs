using UnityEngine;
using agora_gaming_rtc;

namespace AgoraExtension
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Agora Extension/Create Join Parameters", fileName = "AgoraJoinParameters")]
    public class AgoraJoinParameters : ScriptableObject
    {
        public CHANNEL_PROFILE ChannelProfile = CHANNEL_PROFILE.CHANNEL_PROFILE_COMMUNICATION;
        public CLIENT_ROLE_TYPE ClientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE;
        public FRAME_RATE FrameRate = FRAME_RATE.FRAME_RATE_FPS_15;
        public string ChannelName = "ChannelName";
        public uint UserId = 0;
        public bool UseExternalVideoSource = false;
        public int VideoWidth = 640;
        public int VideoHeight = 360;
        public bool UseExternalAudioSource = false;
        public bool UseExternalAudioSink = false;
        public int AudioChannels = 1; // 1: Mono, 2: Stereo.
        public int SampleRate = 48000; // [Hz]
    }
}
