using System.Collections.Generic;
using UnityEngine;

namespace CurlyUtility.DSA
{
    /// <summary>
    /// Statemachines regulate the transitions between different states, and are responsible for calling IState methods.
    /// Statemachines operate upon a type of context object. All states and transitions used within a statemachine must operate on the same context object.
    /// To provide the ability to use Statemachines as states themselves (checkout HFSMs), StateMachines are also IStates.
    /// However, there are additional methods for building a state machine attached to this interface.
    /// General usage of the Statemachine should begin with adding states, adding transitions between these states, setting a starting state, then calling OnLogic() everytime you want logic to be performed.
    /// Most of the time, Statemachine logic is called in the Update() method of a MonoBehaviour.
    /// </summary>
    /// <typeparam name="StateContextType"> The type of the context object that the state machine operates on</typeparam>
    public class StateMachine<StateContextType> : IState<StateContextType>
    {
        // Statemachine specics
        // Honestly there is no point of this HashSet other to enforce the notion of making the user to add a state before adding transitions
        // This is mainly for the purpose that this practice is used by the users, just incase we ever make a debugger -- this will be useful for making an editor or something

        /// <summary>
        /// All of the unique states that have been added to the state machine
        /// </summary>
        /// <returns></returns>
        public HashSet<IState<StateContextType>> ManagedStates { get; private set; } = new HashSet<IState<StateContextType>>();

        /// <summary>
        /// The current state that the state machine is in
        /// </summary>
        /// <value></value>
        public IState<StateContextType> CurrentState { get; private set; }

        /// <summary>
        /// The starting state of the state machine. This is mandatory before calling OnLogic().
        /// </summary>
        /// <value></value>
        public IState<StateContextType> StartingState {get; private set;}

        /// <summary>
        /// The current state that the state machine is in, including nested states, if the current state is a state machine
        /// </summary>
        public IState<StateContextType> NestedCurrentState()
        {
            if (CurrentState is StateMachine<StateContextType> nestedStateMachine)
            {
                return nestedStateMachine.NestedCurrentState();
            }
            return CurrentState;
        }
        
        private bool _hasStarted = false; // Have we called OnStateEnter() of the current state yet?

        /// <summary>
        /// Dictionary<State, Dictionary<Transition, State>> This is a peak type declaration
        /// This is a map, but it's optimized such that we access the map by asking it:
        /// "What is the next state B, given state A, and this transition T?"
        /// rather than,
        /// "What is the transition T, given state A, and next state B?"
        /// The reason why is is because the state machine asks, so given my current state, which transitions are viable?
        /// Then, if the transition is viable, we then progress to the state
        /// Just know that Evan really liked this declaration because he finds long declarations very funny, and long comments even funnier
        /// </summary>
        private Dictionary<IState<StateContextType>, Dictionary<IStateTransition<StateContextType>, IState<StateContextType>>> _stateMap = new();

        /// <summary>
        /// A delegate that is called everytime the state machine changes state.
        /// </summary>
        /// <param name="newState"> The new state that the statemachine has entered </param>
        /// <param name="context"> The context that triggered this state change</param>
        public delegate void StateChangeHandler(IState<StateContextType> newState, StateContextType context);

        /// <summary>
        /// The event to subscribe to if you wish to observe state machine changes.
        /// </summary>
        public StateChangeHandler OnStateChange;

        /// <summary>
        /// Adds a state to the state machine. All states must be unique.
        /// You must add a state (therefore registering it within the statemachine) before doing any other operations on it.
        /// </summary>
        /// <param name="state"> The state that you wish to add to the statemachine </param>
        /// <returns> Whether or not the state was successfully added. It returns false if the state has already been added, otherwise true </returns>
        public bool AddState(IState<StateContextType> state)
        {
            return ManagedStates.Add(state);
        }

        /// <summary>
        /// Sets the starting state of the state machine. This is mandatory before calling OnLogic().
        /// The starting state must have already been previously added to the statemachine. If not, it fails silently.
        /// </summary>
        /// <param name="state"> The state to set as the starting point of the state machine </param>
        public void SetStartingState(IState<StateContextType> state)
        {
            if (!ManagedStates.Contains(state))
            {
                Debug.LogWarning($"Cannot set starting state to {state}. Please add the state the machine to the StateMachine first!");
                return;
            }

            if (CurrentState != null) return;
            CurrentState = state;
            StartingState = state;
        }

        /// <summary>
        /// Adds a transition between two states. Both states must have already been added to the statemachine.
        /// Adds the transition from a to b, not b to a, unless bothways is true. In this case, the transijtion is added both ways.
        /// You can have multiple transitions between two states, as long as the transitions are unique.
        /// </summary>
        /// <param name="a"> The state that you want the transition to originate from </param>
        /// <param name="b"> The state that you want the transition to go to </param>
        /// <param name="transition"> The condition under which this transition should take place</param>
        /// <param name="bothways"> If true, then adds the transition between a and b, and b and a. Otherwise, it's just from a to b</param>
        /// <returns> Whether or not the transition was able to be added. Fails if one or both of the states weren't registered </returns>
        public bool AddTransition(IState<StateContextType> a, IState<StateContextType> b, IStateTransition<StateContextType> transition, bool bothways = false)
        {
            if (!(ManagedStates.Contains(a) && ManagedStates.Contains(b)))
            {
                Debug.LogWarning($"Cannot add transition between state '{a}' and state '{b}.' Please make sure both states are added into the statemachine first!");
                return false;
            }

            // Ensure that the dictionary for state 'a' exists.
            if (!_stateMap.ContainsKey(a))
            {
                _stateMap[a] = new Dictionary<IStateTransition<StateContextType>, IState<StateContextType>>();
            }
            _stateMap[a][transition] = b;

            if (bothways)
            {
                // Ensure that the dictionary for state 'b' exists.
                if (!_stateMap.ContainsKey(b))
                {
                    _stateMap[b] = new Dictionary<IStateTransition<StateContextType>, IState<StateContextType>>();
                }
                _stateMap[b][transition] = a;
            }

            return true;
        }

        /// <summary>
        /// Adds a transition from any state (that has already been registered at the time of adding) to a specific state.
        /// </summary>
        /// <param name="to"> The state that you would like to transition to</param>
        /// <param name="transition"> The condition which this transition should occur </param>
        /// <param name="selfLoop"> Whether or not if you'd the state "to" to transition to itself. Default is false. </param>
        /// <returns> Whether or not all transitions could be added. Fails if the state "to" was not registered. </returns>
        public bool AddTransitionFromAny(IState<StateContextType> to, IStateTransition<StateContextType> transition, bool selfLoop = false)
        {
            if (!ManagedStates.Contains(to))
            {
                Debug.LogWarning($"Cannot add transition between any state and '{to}.' Please make sure '{to}' is added into the statemachine first!");
                return false;
            }

            foreach (var state in ManagedStates)
            {
                if (!selfLoop && state == to) continue;

                if (!_stateMap.ContainsKey(state)) _stateMap[state] = new Dictionary<IStateTransition<StateContextType>, IState<StateContextType>>();
                _stateMap[state][transition] = to;
            }

            return true;
        }

        /// <summary>
        /// Removes all transitions from state a to state b -- not bidirectional
        /// </summary>
        /// <param name="a"> The origin state </param>
        /// <param name="b"> The transition state</param>
        /// <returns> Whether or not the transitions were able to be removed. Fails if either state were not registered in the state machine. </returns>
        public bool RemoveTransition(IState<StateContextType> a, IState<StateContextType> b)
        {
            if (!(ManagedStates.Contains(a) && ManagedStates.Contains(b)))
            {
                Debug.LogWarning($"Cannot add transition between state '{a}' and state '{b}.' Please make sure both states are added into the statemachine first!");
                return false;
            }

            if (_stateMap[a] != null)
            {
                List<IStateTransition<StateContextType>> toRemove = new List<IStateTransition<StateContextType>>();
                foreach (var transitionPair in _stateMap[a])
                {
                    if (transitionPair.Value == b)
                    {
                        Debug.Log($"Removing Transition from {a} to {b}");
                        toRemove.Add(transitionPair.Key);
                    }
                }

                foreach (var transition in toRemove)
                {
                    _stateMap[a].Remove(transition);
                }

                return toRemove.Count > 0; // Return if we actually removed anything
            }

            return false;
        }
        
        public void OnStateEnter()
        {
            CurrentState?.OnStateExit();
            CurrentState = StartingState; // Set the current state to the starting state, just in case this is hierarchical
        }

        public bool IsReady()
        {
            return CurrentState.IsReady(); // we do not want to prematurely leave the state machine
        }
        /// <summary>
        /// Performs the logic of the StateMachine. Call this whenever you would like to progress the state machine.
        /// This is usually called in the Update() method of a MonoBehaviour.
        /// Responsible for changing between the states and calling the individual methods within the state.
        /// </summary>
        /// <param name="context"></param>
        public void OnLogic(StateContextType context)
        {
            // Is our current state new?
            if (_hasStarted == false)
            {
                // It is new, we haven't entered it yet, we shold call Enter
                CurrentState.OnStateEnter();
                _hasStarted = true;
            }

            // Perform the logic of the current state
            CurrentState.OnLogic(context);

            // Should we transition?
            var newState = Evaluate(CurrentState, context);
            if (newState == null) return; // The state isn't ready to transition

            // The state is ready to transition -- complete transition
            CurrentState.OnStateExit();
            newState.OnStateEnter();
            OnStateChange?.Invoke(newState, context);
            CurrentState = newState;
            _hasStarted = false;
        }
        
        /// <summary>
        /// Evaluates whether or not a state is ready to transition to another state.
        /// </summary>
        /// <param name="state"> The state we are checking the transitions of</param>
        /// <param name="context"> The context that the transitions should be fed to make their decisions. </param>
        /// <returns> The new state to transition to. If this is null, the state isn't ready to transition yet or there are no suitable states to transition to </returns>
        private IState<StateContextType> Evaluate(IState<StateContextType> state, StateContextType context)
        {
            if (state.IsReady() == false) return null; // Cannot transition yet

            if (_stateMap.ContainsKey(state) == false) 
            {
                Debug.LogWarning($"State '{state}' does not have any transitions -- you are stuck forever. Cannot evaluate transitions.");
                return null; // No transitions to evaluate
            }
            
            var transitions = _stateMap[state].Keys;

            foreach (var transition in transitions)
            {
                if (transition.CanTransition(context))
                {
                    return _stateMap[state][transition];
                }

                // Note, if our map was configured as a graph, we'd iterate through each combination of state A and state B pairs
                // This is high key useless, because we don't we don't care about the state B we are transitioning into
                // We just care if we CAN transition into, which just adds more justification for this unorthodox mapping
                // If we did go with the more common Dictionary<Node, Dictionary<Node, Edge>> representation of a map, we'd have to ask the dictionary to map backwards, which it isn't made for
            }

            // No states we can transition to
            return null;
        }
    }
}
