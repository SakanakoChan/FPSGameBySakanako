using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKick : MonoBehaviour
{
    [Header("Spring info")]
    [SerializeField] private float springStrength = 120;
    [SerializeField] private float damping = 15;

    [Header("Camera kick recovery info")]
    [SerializeField] private float recoveryDelay;
    [SerializeField] private float springStrengthMultiplierBeforeRecovery = 0.01f;
    private float recoveryTimer = 0;

    [Header("Low frame optimization")]
    [SerializeField] private float fixedStepTime = 1 / 120f;
    private float accumulatedTime = 0;

    private Vector3 rotationOffsetEuler;
    private Vector3 rotationVelocityEuler;

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        recoveryTimer -= Time.deltaTime;

        accumulatedTime += deltaTime;

        while (accumulatedTime >= fixedStepTime)
        {
            RotationCameraKickLogic(fixedStepTime);
            accumulatedTime -= fixedStepTime;
        }
    }

    private void RotationCameraKickLogic(float deltaTime)
    {
        Vector3 targetRotationEuler = Vector3.zero;

        Vector3 springForce = (targetRotationEuler - rotationOffsetEuler) * springStrength;
        if (recoveryTimer > 0)
        {
            springForce *= springStrengthMultiplierBeforeRecovery;
        }

        Vector3 dampingForce = -rotationVelocityEuler * damping;

        Vector3 force = springForce + dampingForce;

        rotationVelocityEuler += force * deltaTime;
        rotationOffsetEuler += rotationVelocityEuler * deltaTime;

        transform.localRotation =
            Quaternion.AngleAxis(rotationOffsetEuler.x, Vector3.right) *
            Quaternion.AngleAxis(rotationOffsetEuler.y, Vector3.up) *
            Quaternion.AngleAxis(rotationOffsetEuler.z, Vector3.forward);

        //Debug.Log($"Angle axis: {transform.localRotation.eulerAngles}, original: {rotationOffsetEuler}" );
    }

    public void AddCameraKick(Vector3 _rotationImpulseEuler)
    {
        recoveryTimer = recoveryDelay;
        rotationVelocityEuler += _rotationImpulseEuler;
    }
}
