using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("HP info")]
    public float maxHP = 100;
    private float currentHP;
    [SerializeField] private float hpRecoverDelay = 3f;
    [SerializeField] private float hpRecoverRate = 50f;
    private float lastDamagedTime = float.MinValue;

    [Header("HUD info")]
    [SerializeField] private DamageDirectionIndicator damageDirectionIndicator;
    [SerializeField] private HPIndicator hpIndicator;

    public bool isDead { get; private set; } = false;
    public event System.Action OnPlayerDied;

    [Header("Flinch info")]
    [SerializeField] private float damageFlinchRatio = 2f;
    private Flinch flinch;

    private void Start()
    {
        flinch = GetComponentInChildren<Flinch>();

        currentHP = maxHP;
    }

    private void Update()
    {
        if (!isDead && Time.time >= lastDamagedTime + hpRecoverDelay && currentHP < maxHP)
        {
            ModifyHP(hpRecoverRate * Time.deltaTime);
        }
    }


    public void TakeDamage(float _damage, Vector3 _damageDirection, bool _isHeadshot, out bool _thisDamageKilledTarget)
    {
        _thisDamageKilledTarget = false;

        ModifyHP(-_damage);
        //currentHP -= _damage;

        flinch?.AddFlinch(new Vector3(-damageFlinchRatio * _damage, 0, 0));
        damageDirectionIndicator.ShowDamageDirectionHint(_damageDirection);

        lastDamagedTime = Time.time;


        if (currentHP <= 0)
        {
            _thisDamageKilledTarget = true;
            Die();
        }
    }

    private void ModifyHP(float _value)
    {
        currentHP += _value;

        if(currentHP > maxHP)
            currentHP = maxHP;

        hpIndicator?.UpdateHPValue(currentHP, maxHP);
    }

    private void Die()
    {
        isDead = true;

        OnPlayerDied?.Invoke();
    }
}
