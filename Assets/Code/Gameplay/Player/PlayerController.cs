using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyCore;
using CurlyCore.Input;
using CurlyUtility.DSA;

namespace LostInLeaves.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerContext
        {
            // References
            [field: SerializeField] public Rigidbody Rigidbody;

            // Input Values
            [field: SerializeField] public Vector2 MovementInput;
        }

        [System.Serializable]
        public struct PlayerInputs
        {
            [field: SerializeField, InputPath] public string Movement { get; private set; }
        }

        [System.Serializable]
        public struct PlayerStates
        {
            public PlayerState Idle;
            public PlayerState Move;

            public bool Validate()
            {
                // check that all states are not null
                return Idle != null && Move != null;
            }
        }

        [field: SerializeField] public string PlayerState { get; private set; }
        [SerializeField] private PlayerContext _context = new PlayerContext();
        [SerializeField] private PlayerInputs _inputs;
        [SerializeField] private PlayerStates _states;

        [GlobalDefault] private InputManager _inputManager;

        private StateMachine<PlayerContext> _stateMachine;

        public PlayerContext Context => _context;
        public PlayerInputs Inputs => _inputs;
        public IState<PlayerContext> CurrentState => _stateMachine.CurrentState;

        private void Start()
        {
            DependencyInjector.InjectDependencies(this);

            if (_states.Validate() == false)
            {
                Debug.LogError("PlayerController: States are not valid!");
                return;
            }

            // Inject dependencies into context
            _context.Rigidbody = GetComponent<Rigidbody>();

            BuildStateMachine();
        }

        private void BuildStateMachine()
        {
            _stateMachine = new StateMachine<PlayerContext>();

            // register states
            _stateMachine.AddState(_states.Idle);
            _stateMachine.AddState(_states.Move);

            // create transitions
            LambdaTransition<PlayerContext> idleToMove = new LambdaTransition<PlayerContext>(
                "Idle -> Move",
                (context) => context.MovementInput.magnitude > 0f
            );

            LambdaTransition<PlayerContext> moveToIdle = new LambdaTransition<PlayerContext>(
                "Move -> Idle",
                (context) => context.MovementInput.magnitude == 0f
            );

            // register transitions
            _stateMachine.AddTransition(_states.Idle, _states.Move, idleToMove);
            _stateMachine.AddTransition(_states.Move, _states.Idle, moveToIdle);

            _stateMachine.SetStartingState(_states.Idle);
        }

        private void Update()
        {
            ObserveContext();
            _stateMachine.OnLogic(_context);
            PlayerState = _stateMachine.CurrentState.ToString();
        }

        private void ObserveContext()
        {
            // Observe input
            Vector2 input = _inputManager.ReadInput<Vector2>(_inputs.Movement);
            _context.MovementInput = _inputManager.ReadInput<Vector2>(_inputs.Movement);
        }
    }
}
