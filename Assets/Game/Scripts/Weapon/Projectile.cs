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

    private bool hasBeenSetup = false;

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
            //Debug.Log("Bullet has hit target: " + hit.collider.name);
            SpawnBulletImpact(hit);
            Destroy(gameObject);
            return;
        }

        currentPosition += displacement;
        transform.position = currentPosition;
        transform.forward = velocity.normalized;
    }


    public void SetupProjectile(Vector3 _velocity, float _gravity, Vector3 _spawnPosition)
    {
        velocity = _velocity;
        gravity = _gravity;

        transform.position = _spawnPosition;
        transform.forward = velocity.normalized;
        currentPosition = transform.position;

        hasBeenSetup = true;

        StartCoroutine(SelfDestroyWithDelay_Coroutine(10));
    }

    private void SpawnBulletImpact(RaycastHit _hit)
    {
        if (impactPrefab != null)
        {
            Quaternion impactDirection = Quaternion.LookRotation(_hit.normal);
            Vector3 impactPosition = _hit.point + 0.05f * _hit.normal;
            Instantiate(impactPrefab, impactPosition, impactDirection);
        }
    }


    private IEnumerator SelfDestroyWithDelay_Coroutine(float _delay)
    {
        yield return new WaitForSeconds(_delay);

        Destroy(gameObject);
    }
}
