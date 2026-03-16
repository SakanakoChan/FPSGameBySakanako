using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackState : ZombieState
{
    private bool hasHit = false;
    private bool isInAttackWindow = false;

    public ZombieAttackState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_zombie, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        anim.SetTrigger(animBoolName);

        hasHit = false;
        isInAttackWindow = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();


    }

    public override void OpenAttackWindow()
    {
        isInAttackWindow = true;
    }

    public override void CloseAttackWindow()
    {
        isInAttackWindow = false;
    }

}
