using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Components
{
    public class Billboard : MonoBehaviour
    {
        public enum TransformAxis
        {
            Forward, Backward, Up, Down, Left, Right
        }

        [SerializeField] private TransformAxis _axis = TransformAxis.Up;
        [SerializeField] private bool _keepUpright = true;
        [SerializeField] private bool _keepForward = false;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            var target = _camera.transform.position;
            var position = transform.position;
            var direction = (target - position).normalized;

            if (_keepUpright)
            {
                direction.y = 0;
                direction.Normalize();
            }

            if (_keepForward)
            {
                direction.x = 0;
                direction.Normalize();
            }

            switch (_axis)
            {
                case TransformAxis.Forward:
                    transform.forward = direction;
                    break;
                case TransformAxis.Backward:
                    transform.forward = -direction;
                    break;
                case TransformAxis.Up:
                    transform.up = direction;
                    break;
                case TransformAxis.Down:
                    transform.up = -direction;
                    break;
                case TransformAxis.Left:
                    transform.right = -direction;
                    break;
                case TransformAxis.Right:
                    transform.right = direction;
                    break;
            }
        }
    }
}
