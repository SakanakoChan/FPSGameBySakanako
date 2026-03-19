using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDirectionIndicator : MonoBehaviour
{
    [SerializeField] private GameObject damageDirectionHintPrefab;

    public void ShowDamageDirectionHint(Vector3 _damageSourceDirection)
    {
        GameObject directionHint = SpawnUtility.SpawnObject(damageDirectionHintPrefab);
        directionHint.transform.SetParent(transform);
        directionHint.transform.localPosition = Vector3.zero;

        var directionHintScript = directionHint.GetComponent<DamageDirectionHint>();
        directionHintScript?.SetupDamageDirectionHint(_damageSourceDirection);
    }
}
