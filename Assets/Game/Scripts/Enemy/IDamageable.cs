using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public bool isDead { get; }
    public void TakeDamage(float _damage, out bool _thisDamageKilledTarget);
}
