using UnityEngine;

namespace AgoraExtension.Samples.HeadlessReceiver
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Agora Extension/Samples/Create Headless Client Config", fileName = "HeadlessClientConfig")]
    public class HeadlessClientConfig : ScriptableObject
    {
        public bool EnableEmulationInEditor;
        public string Channel;
        public ulong StreamerId;
    }
}
