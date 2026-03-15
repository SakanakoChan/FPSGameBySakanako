using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieChaseState : ZombieState
{
    private Vector3 lastPlayerPositionForPathFinding;
    private float pathRefreshTimer;

    private Transform playerTransform;

    public ZombieChaseState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_zombie, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        playerTransform = PlayerReference.playerTransform;

        pathRefreshTimer = zombie.pathRefreshInterval;
        lastPlayerPositionForPathFinding = playerTransform.position;

        agent.SetDestination(lastPlayerPositionForPathFinding);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (stateMachine.currentState != this)
        {
            return;
        }


        float distanceToPlayer = Vector3.Distance(zombie.transform.position, playerTransform.position);
        if (distanceToPlayer > zombie.loseTargetDistance)
        {
            stateMachine.ChangeState(zombie.patrolState);
            return;
        }


        ChaseLogic();
    }

    private void ChaseLogic()
    {
        pathRefreshTimer -= Time.deltaTime;
        float playerDisplacement = Vector3.Distance(playerTransform.position, lastPlayerPositionForPathFinding);

        if (pathRefreshTimer < 0)
        {
            if (playerDisplacement > zombie.repathDistance)
                lastPlayerPositionForPathFinding = playerTransform.position;

            agent.SetDestination(lastPlayerPositionForPathFinding);
            pathRefreshTimer = zombie.pathRefreshInterval;
        }
    }
}
