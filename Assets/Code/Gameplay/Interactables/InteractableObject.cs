using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Interactables
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class InteractableObject : MonoBehaviour
    {
        protected Collider InteractableCollider { get; private set; }
        protected Rigidbody InteractableRigidbody { get; private set; }

        [Header("Interactable Settings")]
        [SerializeField] private bool _automaticallySetupCollider = true; // Set to false if you want to manually setup the collider
        [SerializeField] private bool _allowOneInteractionPerInteractor = true; // Set to false if you want to allow multiple interactions per interactor
        [SerializeField] private bool _printDebugMessages = true; // Set to true if you want to print debug messages
        
        private HashSet<Interactor> _interactedInteractors = new HashSet<Interactor>();
        private List<Interactor> _interactorsInTrigger = new List<Interactor>();

        private void Awake()
        {
            InteractableCollider = GetComponent<Collider>();
            InteractableRigidbody = GetComponent<Rigidbody>();

            if (_automaticallySetupCollider)
            {
                InteractableCollider.isTrigger = true;
                InteractableRigidbody.isKinematic = true;
                InteractableRigidbody.useGravity = false;
            }
            else
            {
                if (!InteractableCollider.isTrigger)
                {
                    Debug.LogWarning("Interactable collider is not a trigger. This may cause unexpected behaviour.");
                }
            }
        }

        private void Update()
        {
            foreach (Interactor interactor in _interactorsInTrigger)
            {
                if (_printDebugMessages) Debug.Log($"InteractableObject: {interactor.name} is interacting with {name}");
                Interact(interactor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isInteractor = other.gameObject.TryGetComponent<Interactor>(out Interactor interactor);

            if (!isInteractor) return;

            if (_allowOneInteractionPerInteractor && _interactedInteractors.Contains(interactor)) return;
            if (_printDebugMessages) Debug.Log($"InteractableObject: {interactor.name} collided with {name}");
            _interactorsInTrigger.Add(interactor);
            _interactedInteractors.Add(interactor);

            OnInteractStart(interactor);
        }

        private void OnTriggerExit(Collider other)
        {
            bool isInteractor = other.gameObject.TryGetComponent<Interactor>(out Interactor interactor);

            if (!isInteractor) return;
            if (_interactorsInTrigger.Contains(interactor)) _interactorsInTrigger.Remove(interactor);
            if (_printDebugMessages) Debug.Log($"InteractableObject: {interactor.name} stopped colliding with {name}");
            OnInteractEnd(interactor);
        }
        

        /// <summary>
        /// Called when an Interactor enters the Interactable's collider.
        /// <param="interactor"> The Interactor entering the Interactable </param>
        /// </summary>
        protected virtual void OnInteractStart(Interactor interactor) {}

        /// <summary>
        /// Called when an Interactor exits the Interactable's collider.
        /// </summary>
        /// <param name="interactor"> The Interactor in the Interactable </param>
        protected virtual void Interact(Interactor interactor) {}

        /// <summary>
        /// Called when an Interactor exits the Interactable's collider.
        /// <param="interactor"> The Interactor exiting the Interactable </param>
        /// </summary>
        protected virtual void OnInteractEnd(Interactor interactor) {}
    }
}
