using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Monolift
{
    public class MonoliftManager : MonoBehaviour
    {
        [System.Serializable]
        public struct MonoliftMaterials
        {
            // Materials
            public Material OnHoverStrong;
            public Material OnHoverWeak;
            public Material OnSelect;
            public Material OnDeselect;
        }

        public static MonoliftManager Instance { get; private set; }
        [field: SerializeField] public MonoliftMaterials Materials { get; private set; }
        public int CurrentStrength = 10;

        [SerializeField] private MonoliftObject _selectedObject = null;
        private Dictionary<MonoliftObject, Material> _originalMaterials = new Dictionary<MonoliftObject, Material>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("MonoliftManager: There can only be one MonoliftManager!");
                Destroy(this);
            }
        }

        public void Hover(MonoliftObject mono, MeshRenderer renderer)
        {
            if (_selectedObject == null)
            {
                if (!_originalMaterials.ContainsKey(mono))
                {
                    _originalMaterials.Add(mono, renderer.material);
                }
                
                if (mono.RequiredStrength > CurrentStrength)
                {
                    renderer.material = Materials.OnHoverWeak;
                    return;
                }

                renderer.material = Materials.OnHoverStrong;
            }
        }

        public void Select(MonoliftObject mono, MeshRenderer renderer)
        {
            if (mono.RequiredStrength > CurrentStrength)
            {
                return;
            }
            
            if (_selectedObject == null)
            {
                if (!_originalMaterials.ContainsKey(mono))
                {
                    _originalMaterials.Add(mono, renderer.material);
                }
                renderer.material = Materials.OnSelect;
                _selectedObject = mono;
            }
        }

        public void Deselect(MonoliftObject mono, MeshRenderer renderer)
        {
            if (_selectedObject == mono)
            {
                renderer.material = _originalMaterials[mono];
                _selectedObject = null;
            }
        }

        public void Unhover(MonoliftObject mono, MeshRenderer renderer)
        {
            if (_selectedObject == null)
            {
                renderer.material = _originalMaterials[mono];
            }
        }
    }
}
