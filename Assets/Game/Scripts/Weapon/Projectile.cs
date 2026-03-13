using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Bullet Impact info")]
    [SerializeField] private GameObject impactPrefab;

    private Vector3 velocity;
    private float gravity;
    private Vector3 currentPosition;
    private float fliedDistance = 0;

    private float basicDamage;

    private bool hasBeenSetup = false;

    private PlayerCombat playerCombat;
    private TrailRenderer trailRenderer;

    private GameObject prefabReference;

    private void Update()
    {
        if (hasBeenSetup == false)
        {
            return;
        }

        velocity.y += gravity * Time.deltaTime;
        Vector3 displacement = velocity * Time.deltaTime;

        if (Physics.Raycast(currentPosition, velocity.normalized, out RaycastHit hit, displacement.magnitude))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float finalDamage = basicDamage;

                bool isHeadShot = false;
                if (hit.collider.tag.Equals("Head"))
                {
                    finalDamage *= 2;
                    isHeadShot = true;
                }
                else if (hit.collider.tag.Equals("Torso"))
                {
                    finalDamage *= 1;
                }
                else if (hit.collider.tag.Equals("Arm"))
                {
                    finalDamage *= 0.9f;
                }
                else if (hit.collider.tag.Equals("Leg"))
                {
                    finalDamage *= 0.8f;
                }

                bool damageableWasDead = damageable.isDead;

                damageable.TakeDamage(finalDamage, out bool killedTarget);

                if (damageableWasDead == false)
                {
                    Color hitMarkColor = killedTarget ? Color.red : Color.white;
                    hitMarkColor.a = 0.65f;
                    playerCombat?.ShowHitFeedback(hitMarkColor, isHeadShot, killedTarget);
                }

            }

            //Debug.Log("Bullet has hit target: " + hit.collider.name);
            SpawnBulletImpact(hit);
            ObjectPoolManager.instance?.ReturnObjectToPool(gameObject, prefabReference);
            //Destroy(gameObject);
            return;
        }

        currentPosition += displacement;
        transform.position = currentPosition;
        transform.forward = velocity.normalized;

        fliedDistance += displacement.magnitude;
        if (fliedDistance > 1000)
        {
            ObjectPoolManager.instance?.ReturnObjectToPool(gameObject, prefabReference);
        }
    }

    private void OnDisable()
    {
        trailRenderer?.Clear();

        velocity = Vector3.zero;
        gravity = 0;
        currentPosition = Vector3.zero;
        fliedDistance = 0;
        basicDamage = 0;
        hasBeenSetup = false;
    }


    public void SetupProjectile(Vector3 _velocity, float _gravity, Vector3 _spawnPosition, float _damage, PlayerCombat _playerCombat)
    {
        velocity = _velocity;
        gravity = _gravity;
        basicDamage = _damage;
        playerCombat = _playerCombat;

        transform.position = _spawnPosition;
        transform.forward = velocity.normalized;
        currentPosition = transform.position;

        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer?.Clear();

        if (prefabReference == null)
            prefabReference = GetComponent<PooledObject>().prefabReference;

        hasBeenSetup = true;

        //StartCoroutine(SelfDestroyWithDelay_Coroutine(10));
    }

    private void SpawnBulletImpact(RaycastHit _hit)
    {
        if (impactPrefab != null)
        {
            Quaternion impactDirection = Quaternion.LookRotation(_hit.normal);
            Vector3 impactPosition = _hit.point + 0.05f * _hit.normal;

            GameObject impact = ObjectPoolManager.instance?.GetObjectFromPool(impactPrefab);
            impact.transform.position = impactPosition;
            impact.transform.rotation = impactDirection;
            impact.transform.SetParent(_hit.collider.transform);
            //Instantiate(impactPrefab, impactPosition, impactDirection, _hit.collider.transform.parent);
        }
    }


    private IEnumerator SelfDestroyWithDelay_Coroutine(float _delay)
    {
        yield return new WaitForSeconds(_delay);

        //Destroy(gameObject);
        ObjectPoolManager.instance?.ReturnObjectToPool(gameObject, gameObject);
    }
}
