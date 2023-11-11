using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyCore;
using CurlyCore.Input;
using UnityEngine;
using static CurlyUtility.TaskUtility;

namespace LostInLeaves.Dialogue
{
    public class DialogueInquirer : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField, InputPath] private string _interactPrompt;

        [GlobalDefault] private InputManager _inputManager;
        [SerializeField] private bool _isRunningDialogue = false;

        private List<DialogueEmitter> _emittersInRange = new List<DialogueEmitter>();

        private void Awake()
        {
            DependencyInjector.InjectDependencies(this);
            _isRunningDialogue = false;
        }

        private void Update()
        {
            if (_isRunningDialogue) return;


            if (_inputManager.GetInputDown(_interactPrompt))
            {
                Debug.Log("Requesting dialogue");

                var nearestRunner = FindNearestRunnerWithLineOfSight();
                if (nearestRunner != null)
                {
                    Debug.Log("Found runner, running dialogue");
                    StartCoroutine(RunDialogueCoroutine(nearestRunner));
                }
            }
        }

        public void RegisterEmitter(DialogueEmitter emitter)
        {
            Debug.Log("Registering emitter");
            _emittersInRange.Add(emitter);
        }

        public void UnregisterEmitter(DialogueEmitter emitter)
        {
            Debug.Log("Unregistering emitter");
            _emittersInRange.Remove(emitter);
        }

        private IEnumerator RunDialogueCoroutine(DialogueEmitter emitter)
        {
            Debug.Log("Running dialogue coroutine");
            _isRunningDialogue = true;
            yield return DialogueRunner.RunDialogueCoroutine(emitter);
            _isRunningDialogue = false;
            Debug.Log("Dialogue coroutine finished");
        }

        private DialogueEmitter FindNearestRunnerWithLineOfSight()
        {
            if (_emittersInRange.Count == 0) return null;
            
            DialogueEmitter nearestRunner = null;
            float nearestDistance = float.MaxValue;
            foreach (var emitter in _emittersInRange)
            {
                if (emitter == null) continue;
                if (!HasLineOfSight(emitter.transform)) continue;

                float distance = Vector2.Distance(transform.position, emitter.transform.position);
                if (distance < nearestDistance)
                {
                    nearestRunner = emitter;
                    nearestDistance = distance;
                }
            }
            return nearestRunner;
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, obstacleLayer);
            return hit.collider == null || hit.transform == target; // No obstacle or the obstacle is the target
        }
    }
}
