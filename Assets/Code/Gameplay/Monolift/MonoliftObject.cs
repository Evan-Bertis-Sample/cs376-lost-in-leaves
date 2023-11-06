using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Clickables;
using UnityEngine;

namespace LostInLeaves.Monolift
{
    [RequireComponent(typeof(MeshRenderer), typeof(Collider), typeof(MonoliftTrack))]
    public class MonoliftObject : MonoBehaviour, IClickable
    {
        public int RequiredStrength = 10;
        public bool IsSelected { get; private set; } = false;
        
        private MeshRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // listen for mouse events
        public void OnHover()
        {
            MonoliftManager.Instance.Hover(this, _renderer);
        }

        public void OnUnhover()
        {
            Debug.Log("Unhovering");
            MonoliftManager.Instance.Unhover(this, _renderer);
        }

        public void OnSelect()
        {
            MonoliftManager.Instance.Select(this, _renderer);
        }

        public void OnDeselect()
        {
            MonoliftManager.Instance.Deselect(this, _renderer);
        }
    }
}
