using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereForShot : MonoBehaviour, IDamageable, IScoreSource
{
    [Header("HP info")]
    public float maxHP = 100;
    private float currentHP;
    public bool isDead { get; protected set; }

    private SphereEmitter emitter;
    private PlayerCombat playerCombat;

    public void TakeDamage(float _damage, Vector3 _damageDirection, bool _isHeadshot, out bool _thisDamageKilledTarget)
    {
        _thisDamageKilledTarget = false;

        if (isDead)
        {
            return;
        }

        currentHP -= _damage;
        Debug.Log("Received damage: " + _damage);

        if (currentHP <= 0)
        {
            _thisDamageKilledTarget = true;
            Die();

            AddScore(ScoreType.Kill, 100, "Kill");
            if (_isHeadshot)
            {
                AddScore(ScoreType.Headshot, 25, "Headshot");
            }
        }
    }

    private void Die()
    {
        isDead = true;

        emitter.RegisterSphereKill();
        playerCombat?.ReplenishMag();
        SpawnUtility.DestroyObject(gameObject);
        //StartCoroutine(RespawnWithDelay(1));
    }

    //private IEnumerator RespawnWithDelay(float _delay)
    //{
    //    yield return new WaitForSeconds(_delay);

    //    yield return new WaitForSeconds(0.5f);

    //    currentHP = maxHP;
    //    isDead = false;
    //}

    public void AddScore(ScoreType _scoreType, int _scoreValue, string _scoreDescription)
    {
        GameEvents.OnScore?.Invoke(_scoreType, _scoreValue, _scoreDescription);
    }

    public void SetupSphere(SphereEmitter _emitter)
    {
        emitter = _emitter;
        isDead = false;
        currentHP = maxHP;

        playerCombat = PlayerReference.playerTransform.GetComponent<PlayerCombat>();
    }
}
