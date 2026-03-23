using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flinch : MonoBehaviour
{
    [Header("Spring info")]
    [SerializeField] private float springStrength;
    [SerializeField] private float damping;

    private Vector3 rotationOffsetEuler;
    private Vector3 rotationVelocityEuler;

    private Quaternion originalRotation;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        FlinchLogic(deltaTime);
    }

    private void FlinchLogic(float deltaTime)
    {
        Vector3 targetRotationEuler = Vector3.zero;
        Vector3 springForce = (targetRotationEuler - rotationOffsetEuler) * springStrength;
        Vector3 dampingForce = -rotationVelocityEuler * damping;

        Vector3 force = springForce + dampingForce;

        rotationVelocityEuler += force * deltaTime;
        rotationOffsetEuler += rotationVelocityEuler * deltaTime;

        transform.localRotation = originalRotation * Quaternion.Euler(rotationOffsetEuler);
    }

    public void AddFlinch(Vector3 _rotationImpulseEuler)
    {
        rotationVelocityEuler += _rotationImpulseEuler;
    }
}
