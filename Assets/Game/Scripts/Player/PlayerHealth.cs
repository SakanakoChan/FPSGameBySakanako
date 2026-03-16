using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("HP info")]
    public float maxHP = 100;
    private float currentHP;

    public bool isDead { get; private set; } = false;


    private void Start()
    {
        currentHP = maxHP;
    }


    public void TakeDamage(float _damage, out bool _thisDamageKilledTarget)
    {
        _thisDamageKilledTarget = false;
        
        currentHP -= _damage;

        if (currentHP < 0)
        {
            _thisDamageKilledTarget = true;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
    }
}
