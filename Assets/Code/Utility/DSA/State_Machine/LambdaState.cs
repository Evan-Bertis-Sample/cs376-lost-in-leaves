using System;

namespace CurlyUtility.DSA
{
    /// <summary>
    /// A wrapper class for IState that allows for the definition of States without needing to create a new class.
    /// This is useful for states that are only used once, or for states that are very simple.
    /// </summary>
    /// <typeparam name="StateContextType"> The type of the context object that the State is operating on to perform it's logic </typeparam>
    public class LambdaState<StateContextType> : IState<StateContextType>
    {
        /// <summary>
        /// A name that makes it easier to debug the State
        /// </summary>
        /// <value></value>
        public string Name {get; private set;}

        /// <summary>
        /// The state's onEnter action
        /// </summary>
        private Action _onEnter;

        /// <summary>
        /// The state's logic action
        /// </summary>
        private Action<StateContextType> _onLogic;

        /// <summary>
        /// The state's onExit action
        /// </summary>
        private Action _onExit;

        /// <summary>
        /// The states isReady predicate
        /// </summary>
        private Func<bool> _isReady;

        /// <summary>
        /// Create a LambdaState with a name
        /// </summary>
        /// <param name="name"> The name of the state. Make it specific </param>
        public LambdaState(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        #region IState interface implementation
        public void OnStateEnter()
        {
            _onEnter?.Invoke();
        }

        public void OnLogic(StateContextType context)
        {
            _onLogic?.Invoke(context);
        }

        public bool IsReady()
        {
            if (_isReady == null) return true;
            return _isReady.Invoke();
        }

        public void OnStateExit()
        {
            _onExit?.Invoke();
        }
        #endregion

        /// <summary>
        /// Sets the state's onEnter action
        /// </summary>
        /// <param name="action"> The action to perform when the state is first entered in the StateMachine </param>
        /// <returns> The LambdaState you are modifying -- allows for chaining </returns>
        public LambdaState<StateContextType> SetEnter(Action action)
        {
            _onEnter = action;
            return this;
        }

        /// <summary>
        /// Sets the state's onLogic action
        /// </summary>
        /// <param name="action"> The action to perform when the state is performing it's logic </param>
        /// <returns> The LambdaState you are modifying -- allows for chaining </returns>
        public LambdaState<StateContextType> SetLogic(Action<StateContextType> action)
        {
            _onLogic = action;
            return this;
        }

        /// <summary>
        /// Sets the state's isReady predicate
        /// </summary>
        /// <param name="isReady"> The predicate that determines if it is possible to exit the state </param>
        /// <returns> The LambdaState you are modifying -- allows for chaining </returns>
        public LambdaState<StateContextType> SetIsReady(Func<bool> isReady)
        {
            _isReady = isReady;
            return this;
        }

        /// <summary>
        /// Sets the state's onExit action
        /// </summary>
        /// <param name="action"> The action to perform when the state is exiting in the StateMachine </param>
        /// <returns> The LambdaState you are modifying -- allows for chaining </returns>
        public LambdaState<StateContextType> SetExit(Action action)
        {
            _onExit = action;
            return this;
        }

        /// <summary>
        /// Sets the state's onEnter, onLogic, isReady, and onExit actions all at once.
        /// </summary>
        /// <param name="onEnter">The action to perform when the state is first entered in the StateMachine.</param>
        /// <param name="onLogic">The action to perform when the state is performing its logic.</param>
        /// <param name="isReady">The predicate that determines if it is possible to exit the state.</param>
        /// <param name="onExit">The action to perform when the state is exiting in the StateMachine.</param>
        public void Set(Action onEnter = null, Action<StateContextType> onLogic = null, Func<bool> isReady = null, Action onExit = null)
        {
            _onEnter = onEnter;
            _onLogic = onLogic;
            _isReady = isReady;
            _onExit = onExit;
        }
    }
}
