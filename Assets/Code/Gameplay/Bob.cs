using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Components
{
    public class Bob : MonoBehaviour
    {
        enum PositionModifier
        {
            Local, World
        }

        [SerializeField] private float _bobSpeed = 1f;
        [SerializeField] private float _bobHeight = 1f;
        [SerializeField] private float _bobOffset = 0f;
        [SerializeField] private PositionModifier _positionModifier = PositionModifier.Local;

        private float _time = 0f;
        private Vector3 _startPosition;

        private void Start()
        {
            switch (_positionModifier)
            {
                case PositionModifier.Local:
                    _startPosition = transform.localPosition;
                    break;
                case PositionModifier.World:
                    _startPosition = transform.position;
                    break;
            }
        }

        private void Update()
        {
            _time += Time.deltaTime * _bobSpeed;
            float y = Mathf.Sin(_time) * _bobHeight + _bobOffset;
            
            switch (_positionModifier)
            {
                case PositionModifier.Local:
                    transform.localPosition = new Vector3(_startPosition.x, _startPosition.y + y, _startPosition.z);
                    break;
                case PositionModifier.World:
                    transform.position = new Vector3(_startPosition.x, _startPosition.y + y, _startPosition.z);
                    break;
            }
        }
    }
}
