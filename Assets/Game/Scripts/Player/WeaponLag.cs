using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLag : MonoBehaviour
{
    [SerializeField] private float lagAmount = 3f;
    [SerializeField] private float adsLagAmount = 1.5f;

    [Space]
    [SerializeField] private float yawOffsetLimit = 3.5f;
    [SerializeField] private float pitchOffsetLimit = 2f;

    [Space]
    [SerializeField] private float adsYawOffsetLimit = 0.1f;
    [SerializeField] private float adsPitchOffsetLimit = 0.05f;

    [Space]
    [SerializeField] private float smoothSpeed = 10f;

    private PlayerLook playerLook;
    private float lastYaw;
    private float lastPitch;

    private float yaw;
    private float pitch;

    private float currentYawOffset;
    private float currentPitchOffset;

    private Weapon currentWeapon;


    private void Start()
    {
        playerLook = GetComponentInParent<PlayerLook>();
        currentWeapon = GetComponentInChildren<Weapon>();
    }

    private void LateUpdate()
    {
        yaw = playerLook.yaw;
        pitch = playerLook.pitch;

        float deltaYaw = Mathf.DeltaAngle(yaw, lastYaw);
        float deltaPitch = Mathf.DeltaAngle(lastPitch, pitch);

        float currentLagAmount = currentWeapon.isInADS ? adsLagAmount : lagAmount;

        float targetYawOffset = -deltaYaw * currentLagAmount;
        float targetPitchOffset = -deltaPitch * currentLagAmount;

        float currentYawOffsetLimit = currentWeapon.isInADS ? adsYawOffsetLimit : yawOffsetLimit;
        float currentPitchOffsetLimit = currentWeapon.isInADS ? adsPitchOffsetLimit : pitchOffsetLimit;

        targetYawOffset = Mathf.Clamp(targetYawOffset, -currentYawOffsetLimit, currentYawOffsetLimit);
        targetPitchOffset = Mathf.Clamp(targetPitchOffset, -currentPitchOffsetLimit, currentPitchOffsetLimit);

        currentYawOffset = Mathf.Lerp(currentYawOffset, targetYawOffset, smoothSpeed * Time.deltaTime);
        currentPitchOffset = Mathf.Lerp(currentPitchOffset, targetPitchOffset, smoothSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentPitchOffset, currentYawOffset, 0);

        lastYaw = yaw;
        lastPitch = pitch;
    }
}
