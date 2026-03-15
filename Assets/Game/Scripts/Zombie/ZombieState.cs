using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieState : State
{
    protected Zombie zombie;
    protected Animator anim;
    protected NavMeshAgent agent;

    public ZombieState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_stateMachine, _animBoolName)
    {
        zombie = _zombie;
        anim = zombie.anim;
        agent = zombie.agent;
    }

    public override void Enter()
    {
        base.Enter();

        anim.SetBool(animBoolName, true);
    }

    public override void Exit()
    {
        base.Exit();

        anim.SetBool(animBoolName, false);
    }

    public override void Update()
    {
        base.Update();
    }
}
