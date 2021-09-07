using UnityEngine;
using agora_gaming_rtc;

namespace AgoraExtension
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Agora Extension/Create Join Parameters", fileName = "AgoraJoinParameters")]
    public class AgoraJoinParameters : ScriptableObject
    {
        public string ChannelName = "ChannelName";
        public bool UseExternalVideoSource = false;
        public int VideoWidth = 640;
        public int VideoHeight = 360;
        public FRAME_RATE FrameRate = FRAME_RATE.FRAME_RATE_FPS_15;
        public CLIENT_ROLE_TYPE ClientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE;
        public CHANNEL_PROFILE ChannelProfile = CHANNEL_PROFILE.CHANNEL_PROFILE_COMMUNICATION;
        public int AudioChannels = 1; // 1: Mono, 2: Stereo.
        public int SampleRate = 48000; // [Hz]
    }
}
