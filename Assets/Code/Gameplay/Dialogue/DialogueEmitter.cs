using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyUtility;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    public class DialogueEmitter : MonoBehaviour
    {
        [field: SerializeField] public string CharacterName { get; private set; } // Character's name field
        [field: SerializeField, FilePath] public string DialoguePath { get; private set; }
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private LayerMask _obstacleLayer;

        public DialogueFrontendObject DialogueFrontend;
        public GameObject DialoguePosition;

        public Vector3 AnchorPosition => DialoguePosition != null ? DialoguePosition.transform.position : transform.position;
        
        private List<DialogueInquirer> _regsteredInquirers = new List<DialogueInquirer>();

        public delegate void DialogueEventHandler(string eventName, object[] parameters);
        public event DialogueEventHandler OnDialogueEvent;

        private void Update()
        {
            // Make sure to remove yourself from nearby DialogueInquirers
            List<DialogueInquirer> toRemove = new List<DialogueInquirer>();
            foreach (var inquirer in _regsteredInquirers)
            {
                if (inquirer == null) continue;
                if (!HasLineOfSight(inquirer.transform) || Vector2.Distance(transform.position, inquirer.transform.position) > _detectionRadius)
                {
                    inquirer.UnregisterEmitter(this);
                    toRemove.Add(inquirer);
                }
            }

            foreach (var inquirer in toRemove)
            {
                _regsteredInquirers.Remove(inquirer);
            }

            // add yourself to nearby DialogueInquirers
            var colliders = Physics.OverlapSphere(transform.position, _detectionRadius, _interactableLayer);
            foreach (var collider in colliders)
            {
                var inquirer = collider.GetComponent<DialogueInquirer>();
                if (inquirer != null && HasLineOfSight(inquirer.transform))
                {
                    inquirer.RegisterEmitter(this);
                    _regsteredInquirers.Add(inquirer);
                }
            }
        }

        private bool HasLineOfSight(Transform target)
        {
            LayerMask mask = ~_obstacleLayer;
            return Physics.Linecast(transform.position, target.position, mask);
        }

        private void OnDrawGizmos()
        {
            // Draw detection radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = Color.white;
        }
    }
}
