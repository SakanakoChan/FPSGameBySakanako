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


    #endregion

    [Header("HP info")]
    [SerializeField] private float maxHP = 100;
    private float currentHP;
    public bool isDead { get; private set; } = false;

    [Header("Move info")]
    [SerializeField] private float rootMotionMultiplier = 2f;

    [Space]
    [SerializeField] private Transform playerTransform;


    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        currentHP = maxHP;

        agent.updatePosition = false;
        agent.updateRotation = true;
    }

    private void Update()
    {
        agent.SetDestination(playerTransform.position);
    }

    private void OnAnimatorMove()
    {
        Vector3 deltaPosition = anim.deltaPosition * rootMotionMultiplier;
        transform.position += deltaPosition;

        //transform.position = anim.rootPosition;
        agent.nextPosition = transform.position;
    }

    public void TakeDamage(float _damage, out bool _isKilled)
    {
        throw new System.NotImplementedException();
    }
}
