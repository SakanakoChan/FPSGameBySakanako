using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    [Header("Hip fire")]
    [SerializeField] private float yawOffsetRatio = 0.5f;
    [SerializeField] private float yawOffsetLimit = 1f;

    [Header("ADS")]
    [SerializeField] private float adsYawOffsetRatio = 0.05f;
    [SerializeField] private float adsYawOffsetLimit = 0.075f;

    [Space]
    [SerializeField] private float smoothSpeed = 2f;

    private float currentYawOffset;

    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    private void Update()
    {
        float currentOffsetRatio = playerCombat.isInADS ? adsYawOffsetRatio : yawOffsetRatio;
        float targetYawOffset = playerMovement.localVelocity.x * currentOffsetRatio;
        float currentOffsetLimit = playerCombat.isInADS ? adsYawOffsetLimit : yawOffsetLimit;
        targetYawOffset = Mathf.Clamp(targetYawOffset, -currentOffsetLimit, currentOffsetLimit);

        currentYawOffset = Mathf.Lerp(currentYawOffset, targetYawOffset, smoothSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(0, currentYawOffset, 0);
    }
}
