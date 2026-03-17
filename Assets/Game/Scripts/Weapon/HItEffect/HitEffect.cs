using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour, IHitEffect
{
    [Header("FX info")]
    [SerializeField] private GameObject hitFXPrefab;
    [SerializeField] private float fxPositionOffset = 0.05f;

    public void ShowHitEffect(RaycastHit _hit)
    {
        if (hitFXPrefab != null)
        {
            Quaternion impactDirection = Quaternion.LookRotation(_hit.normal);
            Vector3 impactPosition = _hit.point + fxPositionOffset * _hit.normal;

            GameObject impact = SpawnUtility.SpawnObject(hitFXPrefab);
            impact.transform.position = impactPosition;
            impact.transform.rotation = impactDirection;
            impact.transform.SetParent(_hit.collider.transform);
        }
    }
}
