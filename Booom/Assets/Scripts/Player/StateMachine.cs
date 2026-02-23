using System;
using System.Collections.Generic;

public class StateMachine
{
    public State CurrentState { get; private set; }

    private readonly Dictionary<(Type from, string trigger), State> _states = new();

    public void AddTransition<T>(string trigger, State to) where T : State => _states[(typeof(T), trigger)] = to;

    public void AddForEachType(string trigger, State to) 
    {
        foreach (var stateType in typeof(State).Assembly.GetTypes())
        {
            if (stateType.IsSubclassOf(typeof(State)) && stateType != to.GetType())
            {
                _states[(stateType, trigger)] = to;
            }
        }

    }

    public void SetInitialState(State state) 
    {
        CurrentState = state != null ? state : throw new ArgumentNullException("The initial state cannot be null");
        CurrentState.Enter();
    }

    public void Trigger(string trigger) 
    {
        _states.TryGetValue((CurrentState.GetType(), trigger), out var nextState);
        if (nextState == null) 
        {
            throw new InvalidOperationException($"No transition from state {CurrentState.GetType().Name} on trigger {trigger}");
        }
        
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void UpdateStateMachine(float dt)
    {
        CurrentState.Handle(dt);
    }

}
