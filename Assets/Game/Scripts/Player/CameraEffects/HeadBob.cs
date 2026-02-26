using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private CharacterController cc;

    [Header("Head bob info")]
    [SerializeField] private float bobFrequency_Walk = 8f;
    [SerializeField] private float bobAmplitude_Walk = 0.05f;

    [Space]
    [SerializeField] private float bobFrequency_Sprint = 12f;
    [SerializeField] private float bobAmplitude_Sprint = 0.1f;

    [Space]
    [SerializeField] private float horizontalAmplitudeMultiplier = 1.2f;

    [Space]
    [Header("Head roll info")]
    [SerializeField] private float sprintRollRate = 0.1f;
    
    [Space]
    [SerializeField] private float smoothSpeed = 10f;

    private float timer;
    private Vector3 originalPosition;
    private Quaternion originalRotation;


    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        cc = GetComponentInParent<CharacterController>();

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        float speed = playerMovement.horizontalVelocity.magnitude;
        bool isMoving = speed > 0.1f && playerMovement.movementState == PlayerMovement.MovementState.Grounded;

        if (!isMoving)
        {
            timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, smoothSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, smoothSpeed * Time.deltaTime);
            return;
        }

        //apply head bob
        bool isSprinting = playerMovement.isSprinting;

        float headBobFrequency = isSprinting ? bobFrequency_Sprint : bobFrequency_Walk;
        float headBobAmplitude = isSprinting ? bobAmplitude_Sprint : bobAmplitude_Walk;

        float speedPercent = speed / playerMovement.maxSpeed;
        speedPercent = Mathf.Clamp01(speedPercent);

        timer += Time.deltaTime * headBobFrequency * speedPercent;

        float headBobY = Mathf.Sin(timer) * headBobAmplitude * speedPercent;
        float headBobX = Mathf.Cos(timer * 0.5f/* + Mathf.PI / 2*/) * headBobAmplitude * horizontalAmplitudeMultiplier * speedPercent;

        Vector3 headBobPosition = originalPosition + new Vector3(headBobX, headBobY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, headBobPosition, smoothSpeed * Time.deltaTime);


        //apply head roll
        float roll = 0;
        if (isSprinting)
        {
            roll = Mathf.Cos(timer * 0.5f) * sprintRollRate * speedPercent;
        }
        else
        {
            roll = 0;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, roll);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
