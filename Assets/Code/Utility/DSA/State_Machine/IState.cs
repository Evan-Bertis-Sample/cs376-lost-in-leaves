namespace CurlyUtility.DSA
{
    /// <summary>
    /// An interface for defining States that can be used in a StateMachine.
    /// </summary>
    /// <typeparam name="StateContextType"></typeparam>
    public interface IState<StateContextType>
    {
        /// <summary>
        /// When the state is entered in the StateMachine, this method is called.
        /// </summary>
        public void OnStateEnter() {}

        /// <summary>
        /// Everytime that a StateMachine calls it's logic, this method is called, if this is the State that the StateMachine is currently in.
        /// </summary>
        /// <param name="context"> The context that the State can operate on </param>
        public void OnLogic(StateContextType context) {}

        /// <summary>
        /// A predicate that determines whether this state to be left or not in the StateMachine. Use this to prolong the life of a state! Defaults to always be true
        /// </summary>
        /// <returns> Whether the statemachine can progress to it's next state, given there is a valid transition </returns>
        public bool IsReady() {return true; }

        /// <summary>
        /// When the state is exited in the Statemachine, this method is called.
        /// </summary>
        public void OnStateExit() {}
    }
}
