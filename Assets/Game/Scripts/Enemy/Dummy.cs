using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    [Header("HP info")]
    public float maxHP = 100;
    private float currentHP;
    public bool isDead { get; private set; } = false;


    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float _damage)
    {
        currentHP += _damage;
        Debug.Log("Received damage: " + -_damage);

        if (currentHP <= 0)
        {
            isDead = true;
        }
    }
}
