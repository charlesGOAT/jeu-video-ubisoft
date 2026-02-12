using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private State _currentState;

    private readonly Dictionary<(Type from, string trigger), State> _states = new();

    public void AddTransition<T>(string trigger, State to) where T : State => _states[(typeof(T), trigger)] = to;

    public void AddForEachType(string trigger, State to) 
    {
        foreach (var stateType in typeof(State).Assembly.GetTypes())
        {
            if (stateType.IsSubclassOf(typeof(State)))
            {
                _states[(stateType, trigger)] = to;
            }
        }

    }

    public void SetInitialState(State state) 
    {
        _currentState = state != null ? state : throw new ArgumentNullException("The initial state cannot be null");
        _currentState.Enter();
    }

    public void Trigger(string trigger) 
    {
        _states.TryGetValue((_currentState.GetType(), trigger), out var nextState);
        if (nextState == null) 
        {
            throw new InvalidOperationException($"No transition from state {_currentState.GetType().Name} on trigger {trigger}");
        }
        
        _currentState.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }

    public void UpdateStateMachine(float dt)
    {
        _currentState.Handle(dt);
    }

}
