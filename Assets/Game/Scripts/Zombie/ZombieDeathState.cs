using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieDeathState : ZombieState
{
    public ZombieDeathState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_zombie, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        anim.SetTrigger(animBoolName);

        agent.enabled = false;

        zombie.StartCoroutine(EnterRagdollModeWithDelay(1f));
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }

    private IEnumerator EnterRagdollModeWithDelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);

        zombie?.EnterRagdollMode();
    }
}
