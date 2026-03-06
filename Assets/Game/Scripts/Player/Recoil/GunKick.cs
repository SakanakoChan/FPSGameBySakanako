using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunKick : MonoBehaviour
{
    [Header("Spring info")]
    [SerializeField] private float springStrength = 120;
    [SerializeField] private float damping = 15;

    private Vector3 positionOffset;
    private Vector3 positionVelocity;

    private Vector3 rotationOffsetEuler;
    private Vector3 rotationVelocityEuler;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        PositionGunKickLogic(deltaTime);

        RotationGunKickLogic(deltaTime);
    }

    private void PositionGunKickLogic(float deltaTime)
    {
        Vector3 targetPosition = Vector3.zero;
        Vector3 springForce = (targetPosition - positionOffset) * springStrength;
        Vector3 dampingForce = -positionVelocity * damping;

        Vector3 force = springForce + dampingForce;

        positionVelocity += force * deltaTime;
        positionOffset += positionVelocity * deltaTime;

        transform.localPosition = originalPosition + positionOffset;
    }

    private void RotationGunKickLogic(float deltaTime)
    {
        Vector3 targetRotationEuler = Vector3.zero;
        Vector3 springForce = (targetRotationEuler - rotationOffsetEuler) * springStrength;
        Vector3 dampingForce = -rotationVelocityEuler * damping;

        Vector3 force = springForce + dampingForce;

        rotationVelocityEuler += force * deltaTime;
        rotationOffsetEuler += rotationVelocityEuler * deltaTime;

        transform.localRotation = originalRotation * Quaternion.Euler(rotationOffsetEuler);

        //Quaternion worldRotation =
        //    Quaternion.AngleAxis(rotationOffsetEuler.x, mainCam.transform.right) *
        //    Quaternion.AngleAxis(rotationOffsetEuler.y, mainCam.transform.up) *
        //    Quaternion.AngleAxis(rotationOffsetEuler.z, mainCam.transform.forward);


        //transform.localRotation = Quaternion.Inverse(transform.parent.rotation) * worldRotation;
    }

    public void AddGunKick(Vector3 _positionImpulse, Vector3 _rotationImpulseEuler)
    {
        positionVelocity += _positionImpulse;
        rotationVelocityEuler += _rotationImpulseEuler;
    }
}
