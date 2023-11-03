## Example Usage of the State Machine

Here is an example of how to build a State Machine using Lambda functions
```cs
// Construct a state machine
_stateMachine = new StateMachine<GameplayContext>();

// Define gameplayState
LambdaState<GameplayContext> gameplayState = new LambdaState<GameplayContext>("Gameplay State");
gameplayState.Set(
    () => Debug.Log("Gameplay State : On State Enter"),
    context => Debug.Log($"Gameplay State : On Logic -- Reset Type: {context.ResetType}"),
    null,
    () => Debug.Log("Gameplay State : On State Exit")
);

// Define testState
LambdaState<GameplayContext> testState = new LambdaState<GameplayContext>("Test State");
testState.Set(
    () => Debug.Log("Test State : On State Enter"),
    context => Debug.Log($"Test State : On Logic -- Reset Type: {context.ResetType}"),
    null,
    () => Debug.Log("Test State : On State Exit")
);

// Define the conditions that must be met for the statemachine to use this transition
LambdaTransition<GameplayContext> transitionGT = new LambdaTransition<GameplayContext>("Gameplay to Test");
transitionGT.SetTransitionCondition(context => context.ResetType == ResetType.CHECKPOINT_RESET);
// Similary, do this with the other transition
LambdaTransition<GameplayContext> transitionTG = new LambdaTransition<GameplayContext>("Test to Gameplay");
transitionTG.SetTransitionCondition(context => context.ResetType == ResetType.FULL_RESET);

// Add the states to the state machine -- must do first in order to use the state machine
_stateMachine.AddState(gameplayState);
_stateMachine.AddState(testState);

// Add a transition from gameplayState to testState using the condition transitionGT
// The transition fires when the state machine is in the "gameplayState" and the transitionGT condition is true
_stateMachine.AddTransition(gameplayState, testState, transitionGT);
// Do this but in refverse
_stateMachine.AddTransition(testState, gameplayState, transitionTG);

// Set the starting state of the state machine. If you don't use this, state machine will throw an error because it doesn't know where to start
_stateMachine.SetStartingState(gameplayState);
```

Creating a state machine requires defining a "Context" type, which is passed in as a type arguement in the state machine, it's states, and transitions. This "Context" can range from type, from floats, strings, and all the way to complex clases. The context will be fed to state logic to determine how the state machine should act, and to allow for manipulation of state outside of the state machine -- allowing for the nudging of the state machine internally and externally. My general advice is to keep the "nudging" to a minimum, and to encapsulate your context into a struct or a class, that way you can add new context variables fairly easily, without having to redefine all your type information. The context gives the state machine grounding on how to operate, and acts as shared data throughout the state machine.

LambdaState and LambdaTransition types have been defined, in the off-case that your States and Transitions can easily be written as states or transitions more easily. Otherwise, to define states and transitions, you must create a new class that implements IState<StateContextType> or IStateTransition<StateContextType>.

After creating your states and transitions, add each state to the state machine, and define the transitions between each states. Then, call ```StateMachine.OnLogic(context)``` in your update loop, or whenever you need to progress the state machine.

```cs
void Update()
{
    _stateMachine.OnLogic(context)
}
```

You can even use state machines as independent states themselves! This is because they implement ```IState```. Here is an example.

```cs
void BuildComplexStateMachine()
{
    StateMachine<ContextType> subStateMachineA = new StateMachine<ContextType>();
    // Add some states and transitions to subStateMachineA, and define a starting state
    // ...

    StateMachine<ContextType> subStateMachineB = new StateMachine<ContextType>();
    // Add some states and transitions to subStateMachineB, and define a starting state
    // ...

    StateMachine<ContextType> complexStateMachine = new StateMachine<ContextType>();
    
    // Add statemachines as states
    complexStateMachine.AddState(subStateMachineA);
    complexStateMachine.AddState(subStateMachineB);

    // Define a transition
    LambdaTransition<ContextType> transiton = new LambdaTransition("A to B");
    transiton.SetTransitionCondition(context => /* some logic */);

    // Add a transition between two state machines!
    complexStateMachine.AddTransition(subStateMachineA, subStateMachineB, trasition);

    complexStateMachine.SetStartingState(subStateMachineA);
}
```

By calling ```complexStateMachine.OnLogic(context)```, it'll be performing the logic of the StateMachine that it's currently set to, and subsequently, that StateMachine's current state.

The effect of this implementation is that you can have high level state machines that govern very broad state data, like the state a player is in -- {OnGround, InAir, InWater}. Then, you can have granular substate machines that govern more finite state data for these broader states. 

I hope this short little documentation might help clear up how the StateMachine API might work -- it could be useful for all sorts of things. 