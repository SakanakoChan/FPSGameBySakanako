using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLag : MonoBehaviour
{
    [SerializeField] private float lagAmount = 0.05f;

    [Space]
    [SerializeField] private float yawOffsetLimit = 5f;
    [SerializeField] private float pitchOffsetLimit = 5f;

    [Space]
    [SerializeField] private float smoothSpeed = 10f;

    private PlayerLook playerLook;
    private float lastYaw;
    private float lastPitch;

    private float yaw;
    private float pitch;

    private float currentYawOffset;
    private float currentPitchOffset;


    private void Start()
    {
        playerLook = GetComponentInParent<PlayerLook>();
    }

    private void LateUpdate()
    {
        yaw = playerLook.yaw;
        pitch = playerLook.pitch;

        float deltaYaw = Mathf.DeltaAngle(yaw, lastYaw);
        float deltaPitch = Mathf.DeltaAngle(lastPitch, pitch);

        float targetYawOffset = -deltaYaw * lagAmount;
        float targetPitchOffset = -deltaPitch * lagAmount;

        targetYawOffset = Mathf.Clamp(targetYawOffset, -yawOffsetLimit, yawOffsetLimit);
        targetPitchOffset = Mathf.Clamp(targetPitchOffset, -pitchOffsetLimit, pitchOffsetLimit);

        currentYawOffset = Mathf.Lerp(currentYawOffset, targetYawOffset, smoothSpeed * Time.deltaTime);
        currentPitchOffset = Mathf.Lerp(currentPitchOffset, targetPitchOffset, smoothSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentPitchOffset, currentYawOffset, 0);

        lastYaw = yaw;
        lastPitch = pitch;
    }
}
