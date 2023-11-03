using System;

namespace CurlyUtility.DSA
{
    /// <summary>
    /// A wrapper class for IStateTransition which allows for the creation of state transitions using lambda expressions.
    /// </summary>
    /// <typeparam name="StateContextType"> The context type that the transition operates on </typeparam>
    public class LambdaTransition<StateContextType> : IStateTransition<StateContextType> where StateContextType : class
    {
        /// <summary>
        /// The name of the state transistion -- makes it easier to debug
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A predicate that determines whether this transition can occur or not.
        /// </summary>
        private Func<StateContextType, bool> _transitionCondition;

        /// <summary>
        /// Construct a LambdaTransition with a name
        /// </summary>
        /// <param name="name"> The name of the transition. Make sure it's specific. </param>
        public LambdaTransition(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool CanTransition(StateContextType context)
        {
            return _transitionCondition != null && _transitionCondition(context);
        }

        /// <summary>
        /// Set the transition condition to a lambda predicate, which takes in a StateContextType and returns a bool.
        /// </summary>
        /// <param name="condition"> The condition that triggers the transition </param>
        /// <returns></returns>
        public LambdaTransition<StateContextType> SetTransitionCondition(Func<StateContextType, bool> condition)
        {
            _transitionCondition = condition;
            return this;
        }
    }
}
