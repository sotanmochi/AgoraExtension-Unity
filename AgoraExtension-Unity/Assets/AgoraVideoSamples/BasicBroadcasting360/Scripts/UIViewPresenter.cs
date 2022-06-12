using UnityEngine;

namespace AgoraExtension.Samples.BasicBroadcasting360
{
    public class UIViewPresenter : MonoBehaviour
    {
        [SerializeField] Canvas _canvas;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _canvas.gameObject.SetActive(!_canvas.gameObject.activeSelf);
            }
        }
    }
}