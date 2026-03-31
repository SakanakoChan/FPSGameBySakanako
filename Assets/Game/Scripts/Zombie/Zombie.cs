using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable, IScoreSource
{
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }


    #region States
    private StateMachine stateMachine = new StateMachine();

    public ZombiePatrolState patrolState { get; private set; }
    public ZombieChaseState chaseState { get; private set; }
    public ZombieAttackState attackState { get; private set; }
    public ZombieDeathState deathState { get; private set; }
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


    [Header("Audio info")]
    public AudioSource patrolSFX;
    public AudioSource chaseSFX;
    public AudioSource attackSFX;
    public AudioSource deathSFX;


    #region Ragdoll Control
    private List<Rigidbody> ragDollRBList;
    private Collider cd;
    #endregion


    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cd = GetComponent<Collider>();
        ragDollRBList = GetComponentsInChildren<Rigidbody>().ToList();

        foreach (var rb in ragDollRBList)
        {
            rb.isKinematic = true;
        }

        agent.updatePosition = false;
        agent.updateRotation = true;

        currentHP = maxHP;

        patrolState = new ZombiePatrolState(this, stateMachine, null);
        chaseState = new ZombieChaseState(this, stateMachine, "Running");
        attackState = new ZombieAttackState(this, stateMachine, "Attack");
        deathState = new ZombieDeathState(this, stateMachine, "Death");

        stateMachine.Initialize(patrolState);
    }

    private void Update()
    {
        stateMachine.currentState.Update();
        //Debug.Log(stateMachine.currentState);
    }

    private void OnAnimatorMove()
    {
        transform.position = anim.rootPosition;
        agent.nextPosition = transform.position;
    }

    public void PerformAttack(out bool _hasHitTarget)
    {
        _hasHitTarget = false;

        if (isDead)
            return;

        var hits = Physics.OverlapSphere(attackPosition.position, attackRadius);
        foreach (var hit in hits)
        {
            if (hit.transform.root == transform.root)
            {
                continue;
            }

            Debug.Log("Zombie hit: " + hit.name);
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (damageable != null && playerHealth != null)
            {
                _hasHitTarget = true;

                Vector3 damageDirection = (hit.transform.position - transform.position).normalized;
                damageable?.TakeDamage(attackPower, damageDirection, false, out bool thisDamageKilledTarget);
            }
        }
    }

    public void TakeDamage(float _damage, Vector3 _damageDirection, bool _isHeadshot, out bool _thisDamageKilledTarget)
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

            Die(_damageDirection);

            AddScore(ScoreType.Kill, 100, "Kill");
            if (_isHeadshot)
            {
                AddScore(ScoreType.Headshot, 25, "Headshot");
            }
        }
    }

    private void Die(Vector3 _damageDirection)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Vector3 localPosition = transform.InverseTransformDirection(_damageDirection);
        int deathDirection = CalculateDeathDirection(localPosition);
        anim.SetInteger("DeathDirection", deathDirection);

        stateMachine.ChangeState(deathState);
    }

    private int CalculateDeathDirection(Vector3 localPosition)
    {
        int deathDirection = 0;
        if (localPosition.z > 0.5f)
        {
            deathDirection = 0;
            return deathDirection;
        }

        if (localPosition.z < -0.5f)
        {
            deathDirection = 2;
            return deathDirection;
        }

        if (localPosition.x > 0)
        {
            deathDirection = 1;
            return deathDirection;
        }

        if (localPosition.x < 0)
        {
            deathDirection = 3;
            return deathDirection;
        }

        return deathDirection;
    }

    public void EnterRagdollMode()
    {
        cd.isTrigger = true;
        anim.enabled = false;

        foreach (var rb in ragDollRBList)
        {
            rb.isKinematic = false;
        }
    }

    public void OpenAttackWindow()
    {
        stateMachine.currentState?.OpenAttackWindow();
    }

    public void CloseAttackWindow()
    {
        stateMachine.currentState?.CloseAttackWindow();
    }

    public void AddScore(ScoreType _scoreType, int _scoreValue, string _scoreDescription)
    {
        GameEvents.OnScore?.Invoke(_scoreType, _scoreValue, _scoreDescription);
    }

    public void PlaySFX(AudioSource _audioSource)
    {
        patrolSFX?.Stop();
        chaseSFX?.Stop();
        attackSFX?.Stop();
        deathSFX?.Stop();

        _audioSource?.Play();
    }
}
