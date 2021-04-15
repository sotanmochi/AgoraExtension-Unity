using UnityEngine;
using agora_gaming_rtc;

namespace AgoraExtension
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Agora Extension/Create Config", fileName = "AgoraConfig")]
    public class AgoraConfig : ScriptableObject
    {
        public string AppId;
        public AREA_CODE AreaCode = AREA_CODE.AREA_CODE_JP;
    }
}
