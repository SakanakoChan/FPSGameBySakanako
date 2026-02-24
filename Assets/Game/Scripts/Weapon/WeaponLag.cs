using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLag : MonoBehaviour
{
    [SerializeField] private float lagAmount = 0.05f;
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

        float deltaYaw = Mathf.DeltaAngle(lastYaw, yaw);
        Debug.Log("Delta yaw: " + deltaYaw);

        float deltaYaw2 = Mathf.DeltaAngle(yaw, lastYaw);
        Debug.Log("Delta yaw2: " + deltaYaw2);
        float deltaPitch = Mathf.DeltaAngle(lastPitch, pitch);

        float targetYawOffset = -deltaYaw2 * lagAmount;
        float targetPitchOffset = -deltaPitch * lagAmount;

        currentYawOffset = Mathf.Lerp(currentYawOffset, targetYawOffset, smoothSpeed * Time.deltaTime);
        currentPitchOffset = Mathf.Lerp(currentPitchOffset, targetPitchOffset, smoothSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentPitchOffset, currentYawOffset, 0);

        lastYaw = yaw;
        lastPitch = pitch;
    }
}
