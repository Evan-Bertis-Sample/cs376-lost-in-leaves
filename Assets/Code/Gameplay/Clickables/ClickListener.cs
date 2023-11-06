using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using CurlyCore.Input;
using LostInLeaves.Clickables;
using UnityEngine;

namespace LostInLeaves.Clickables
{
    public class ClickListener : MonoBehaviour
    {
        [SerializeField, InputPath] private string ClickPrompt;

        private Dictionary<GameObject, IClickable> _clickables = new Dictionary<GameObject, IClickable>();
        private IClickable _currentlyHovered;
        private IClickable _currentlySelected;

        private void Start()
        {
            DependencyInjector.InjectDependencies(this);
        }

        private void Update()
        {
            // Handle hover logic
            GameObject selected = ShootRay();

            if (selected != null)
            {
                // Get the cached IClickable or retrieve it from the object and cache it.
                if (!_clickables.TryGetValue(selected, out IClickable clickable))
                {
                    clickable = selected.GetComponent<IClickable>();
                    if (clickable != null)
                    {
                        _clickables.Add(selected, clickable);
                    }
                }

                // If we have a new object under the cursor
                if (_currentlyHovered != clickable || clickable == null)
                {
                    // Call OnUnhover on the previously hovered object, if there was one
                    _currentlyHovered?.OnUnhover();

                    // Set the new hovered object
                    _currentlyHovered = clickable;
                    _currentlyHovered?.OnHover(); // Ensure we don't call OnHover on null
                }
            }
            else if (_currentlyHovered != null)
            {
                // If there's no object under the cursor, call OnUnhover on the current one
                _currentlyHovered.OnUnhover();
                _currentlyHovered = null;
            }

            // Handle select logic
            if (Input.GetMouseButtonDown(0) && _currentlyHovered != null)
            {
                // Select a new object
                _currentlySelected?.OnDeselect();
                _currentlySelected = _currentlyHovered;
                _currentlySelected.OnSelect();
            }

            // Handle deselect logic on mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                if (_currentlySelected != null)
                {
                    _currentlySelected.OnDeselect();
                    _currentlySelected = null;
                    _currentlyHovered = null; // it's possible to deselect without hovering over anything
                }
            }
        }

        private GameObject ShootRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                return hit.collider.gameObject;
            }

            return null;
        }
    }
}
