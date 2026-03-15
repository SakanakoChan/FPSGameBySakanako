using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State currentState { get; private set; }

    public void Initialize(State _initialState)
    {
        currentState = _initialState;
        _initialState.Enter();
    }

    public void ChangeState(State _targetState)
    {
        if (_targetState == null || _targetState == currentState)
        {
            return;
        }

        currentState.Exit();
        currentState = _targetState;
        currentState.Enter();
    }
}
