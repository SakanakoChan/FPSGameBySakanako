using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKick : MonoBehaviour
{
    [Header("Spring info")]
    [SerializeField] private float springStrength = 120;
    [SerializeField] private float damping = 15;

    private Vector3 rotationOffsetEuler;
    private Vector3 rotationVelocityEuler;

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        RotationCameraKickLogic(deltaTime);
    }

    private void RotationCameraKickLogic(float deltaTime)
    {
        Vector3 targetRotationEuler = Vector3.zero;
        Vector3 springForce = (targetRotationEuler - rotationOffsetEuler) * springStrength;
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
        rotationVelocityEuler += _rotationImpulseEuler;
    }
}
