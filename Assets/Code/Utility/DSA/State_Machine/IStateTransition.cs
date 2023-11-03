namespace CurlyUtility.DSA
{
    /// <summary>
    /// A state transition is a transition between two states in a state machine.
    /// It is a predicate that determines whether a transition can occur or not.
    /// It operates upon a context, which is the object that the state machine is operating on.
    /// </summary>
    /// <typeparam name="StateContextType"> The type of the context object that the transition is using to determine whether or not the state can transition. </typeparam>
    public interface IStateTransition<StateContextType>
    {
        /// <summary>
        /// A method that determines whether a transition can occur or not.
        /// </summary>
        /// <param name="context"> The context that the transition operates on</param>
        /// <returns> Whether or not the transition can occur </returns>
        public bool CanTransition(StateContextType context);
    }
}
