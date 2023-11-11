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
        private GameObject _displayPromptInstance;

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
            var colliders = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _interactableLayer);
            foreach (var collider in colliders)
            {
                var inquirer = collider.GetComponent<DialogueInquirer>();
                if (inquirer != null && HasLineOfSight(inquirer.transform))
                {
                    inquirer.RegisterEmitter(this);
                    _regsteredInquirers.Add(inquirer);
                }
            }

            // show display prompt if there are nearby DialogueInquirers
            _displayPromptInstance.SetActive(_regsteredInquirers.Count > 0);
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, _obstacleLayer);
            return hit.collider == null || hit.transform == target; // No obstacle or the obstacle is the target
        }

    }
}
