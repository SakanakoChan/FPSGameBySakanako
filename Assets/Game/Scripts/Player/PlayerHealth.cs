using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("HP info")]
    public float maxHP = 100;
    private float currentHP;

    [Header("HUD info")]
    [SerializeField] private DamageDirectionIndicator damageDirectionIndicator;

    public bool isDead { get; private set; } = false;

    [Header("Flinch info")]
    [SerializeField] private float damageFlinchRatio = 2f;
    private Flinch flinch;

    private void Start()
    {
        flinch = GetComponentInChildren<Flinch>();

        currentHP = maxHP;
    }


    public void TakeDamage(float _damage, Vector3 _damageDirection, bool _isHeadshot, out bool _thisDamageKilledTarget)
    {
        _thisDamageKilledTarget = false;

        currentHP -= _damage;

        flinch?.AddFlinch(new Vector3(-damageFlinchRatio * _damage, 0, 0));
        damageDirectionIndicator.ShowDamageDirectionHint(_damageDirection);


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
