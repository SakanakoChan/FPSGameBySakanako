using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePatrolState : ZombieState
{
    private int currentWayPointIndex = 0;
    private Vector3 targetPosition;

    private bool isWaiting = false;
    private Coroutine waitCoroutine;

    private Transform playerTransform;

    public ZombiePatrolState(Zombie _zombie, StateMachine _stateMachine, string _animBoolName) : base(_zombie, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        playerTransform = PlayerReference.playerTransform;

        targetPosition = zombie.patrolWayPoints[currentWayPointIndex].position;
        agent.SetDestination(targetPosition);

        isWaiting = false;
        anim.SetBool("Walking", true);
        zombie?.PlaySFX(zombie.patrolSFX);
    }

    public override void Exit()
    {
        if (waitCoroutine != null)
            zombie.StopCoroutine(waitCoroutine);

        anim.SetBool("Walking", false);
    }

    public override void Update()
    {
        float distanceToPlayer = Vector3.Distance(zombie.transform.position, playerTransform.position);
        if (distanceToPlayer <= zombie.startChaseDistance)
        {
            stateMachine.ChangeState(zombie.chaseState);
            return;
        }


        if (agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            waitCoroutine = zombie.StartCoroutine(WaitAndSetNextTargetPosition());
        }
    }

    private IEnumerator WaitAndSetNextTargetPosition()
    {
        isWaiting = true;
        anim.SetBool("Walking", false);

        yield return new WaitForSeconds(zombie.patrolWaitTime);

        currentWayPointIndex++;
        currentWayPointIndex %= zombie.patrolWayPoints.Count;

        targetPosition = zombie.patrolWayPoints[currentWayPointIndex].position;
        agent.SetDestination(targetPosition);

        isWaiting = false;
        anim.SetBool("Walking", true);
    }

}
