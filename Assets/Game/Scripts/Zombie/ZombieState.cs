using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieState : State
{
    private Zombie zombie;
    private Animator anim;

    public ZombieState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_stateMachine, _animBoolName)
    {
        zombie = _zombie;
        anim = zombie.anim;
    }

}
