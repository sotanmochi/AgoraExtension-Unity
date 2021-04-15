using UnityEngine;
using agora_gaming_rtc;

namespace AgoraExtension
{
    public class AgoraJoinResult
    {
        public uint UserId;
        public string ChannelName;

        public AgoraJoinResult(uint userId, string channelName = "Unknonw")
        {
            UserId = userId;
            ChannelName = channelName;
        }
    }
}
