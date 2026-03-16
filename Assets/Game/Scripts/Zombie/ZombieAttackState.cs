using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackState : ZombieState
{
    private bool hasHit = false;
    //controlled by animation event
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

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(animBoolName) && info.normalizedTime >= 1f)
        {
            stateMachine.ChangeState(zombie.chaseState);
            return;
        }


        if (isInAttackWindow && !hasHit)
        {
            bool hasHitTarget = false;
            zombie?.PerformAttack(out hasHitTarget);

            if (hasHitTarget)
                hasHit = true;
        }
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
