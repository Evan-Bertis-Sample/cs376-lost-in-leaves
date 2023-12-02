using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Player;
using UnityEngine;

namespace LostInLeaves.Components
{
    /// <summary>
    /// Projects the position of a RectTransform to be on the plane formed by the player's position and the camera's forward vector
    /// This is used to make UI elements appear to be in the world
    /// The UI element will still be in the same pixel position on the screen, but it will appear to be in the world
    /// 
    /// </summary>
    public class CanvasToPlayerPlane : MonoBehaviour
    {
        public static Vector3 PlayerPosition => PlayerController.Instance.gameObject.transform.position;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Camera _camera;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _camera = Camera.main;

            if (_rectTransform == null)
            {
                Debug.LogError("CanvasToPlayerPlane: No RectTransform was provided!");
                return;
            }

            if (_camera == null)
            {
                Debug.LogError("CanvasToPlayerPlane: No camera was provided!");
                return;
            }

            SetPosition();
        }

        private void SetPosition()
        {
            if (_rectTransform == null || _camera == null)
            {
                return;
            }

            Vector3 playerToCamera = _camera.transform.position - PlayerPosition; // defines the normal of the plane

            // project the position of the rect transform onto the plane
            
        }
    }
}
