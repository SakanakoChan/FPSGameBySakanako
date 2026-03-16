using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    protected StateMachine stateMachine;
	protected string animBoolName;

	public State(StateMachine _stateMachine, string _animBoolName)
	{
		stateMachine = _stateMachine;
		animBoolName = _animBoolName;
	}

	public virtual void Enter()
	{
		
	}

	public virtual void Exit()
	{

	}

	public virtual void Update()
	{

	}

	public virtual void OnAnimatorMove()
	{

	}


	public virtual void OpenAttackWindow()
	{

	}

	public virtual void CloseAttackWindow()
	{

	}
}
