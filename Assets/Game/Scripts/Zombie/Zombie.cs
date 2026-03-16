using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable
{
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }


    #region States
    private StateMachine stateMachine = new StateMachine();

    public ZombiePatrolState patrolState { get; private set; }
    public ZombieChaseState chaseState { get; private set; }
    public ZombieAttackState attackState { get; private set; }
    #endregion


    [Header("HP info")]
    [SerializeField] private float maxHP = 100;
    private float currentHP;
    public bool isDead { get; private set; } = false;

    [Header("Patrol info")]
    public List<Transform> patrolWayPoints;
    public float patrolWaitTime = 5f;

    [Header("Chase info")]
    public float pathRefreshInterval = 0.2f;
    public float repathDistance = 0.5f;
    public float startChaseDistance = 5f;
    public float loseTargetDistance = 15f;

    [Header("Attack info")]
    public float attackDistance = 1f;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private float attackPower = 10f;


    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = false;
        agent.updateRotation = true;

        currentHP = maxHP;

        patrolState = new ZombiePatrolState(this, stateMachine, null);
        chaseState = new ZombieChaseState(this, stateMachine, "Running");
        attackState = new ZombieAttackState(this, stateMachine, "Attack");

        stateMachine.Initialize(patrolState);
    }

    private void Update()
    {
        stateMachine.currentState.Update();
    }

    private void OnAnimatorMove()
    {
        transform.position = anim.rootPosition;
        agent.nextPosition = transform.position;
    }

    public void PerformAttack(out bool _hasHitTarget)
    {
        _hasHitTarget = false;

        var hits = Physics.OverlapSphere(attackPosition.position, attackRadius);
        foreach (var hit in hits)
        {
            if (hit.transform.root == transform.root)
            {
                continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                _hasHitTarget = true;
                damageable?.TakeDamage(attackPower, out bool thisDamageKilledTarget);
            }
        }
    }

    public void TakeDamage(float _damage, out bool _thisDamageKilledTarget)
    {
        _thisDamageKilledTarget = false;

        if (isDead)
        {
            return;
        }

        currentHP -= _damage;
        if (currentHP <= 0)
        {
            _thisDamageKilledTarget = true;

            Die();
        }
    }

    private void Die()
    {
        isDead = true;
    }

    public void OpenAttackWindow()
    {
        stateMachine.currentState?.OpenAttackWindow();
    }

    public void CloseAttackWindow()
    {
        stateMachine.currentState?.CloseAttackWindow();
    }
}
