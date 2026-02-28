using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffect : MonoBehaviour
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


    //[Header("Landing Effect Spring Style")]
    //[SerializeField] private float landingImpactForce = -0.15f;

    //private float landingVelocity;
    //private float cameraLandingOffset;

    //[SerializeField] private float springStrength = 120f;
    //[SerializeField] private float springDamping = 15f;

    [Space]
    [Header("Landing Effect")]
    [SerializeField] private float landingDuration = 0.2f;
    [SerializeField] private float landingAmount = 0.15f;
    private float landingTimer;
    private bool wasGrounded = true;


    [Space]
    [SerializeField] private float smoothSpeed = 10f;


    private float timer_HeadBobAndSprintRoll;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;


    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        cc = GetComponentInParent<CharacterController>();

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        targetPosition = originalPosition;
        targetRotation = originalRotation;
    }

    private void Update()
    {
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;

        float speed = playerMovement.horizontalVelocity.magnitude;
        bool isMovingOnGround = speed > 0.1f && playerMovement.movementState == PlayerMovement.MovementState.Grounded;

        if (!isMovingOnGround)
        {
            timer_HeadBobAndSprintRoll = 0;
            //transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, smoothSpeed * Time.deltaTime);
            //transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, smoothSpeed * Time.deltaTime);
            //return;
        }

        bool isSprinting = playerMovement.isSprinting;
        float speedPercent = speed / playerMovement.maxSpeed;

        //apply head bob
        ApplyHeadBob(speed, isSprinting, speedPercent);

        ////apply head roll
        ApplySprintRoll(isSprinting, speedPercent);

        //Landing effect
        //ApplyLandingEffect();
        ApplyLandingEffect_New();

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }


    private void ApplyHeadBob(float speed, bool isSprinting, float speedPercent)
    {
        float headBobFrequency = isSprinting ? bobFrequency_Sprint : bobFrequency_Walk;
        float headBobAmplitude = isSprinting ? bobAmplitude_Sprint : bobAmplitude_Walk;

        speedPercent = speed / playerMovement.maxSpeed;
        speedPercent = Mathf.Clamp01(speedPercent);

        timer_HeadBobAndSprintRoll += Time.deltaTime * headBobFrequency * speedPercent;

        float headBobY = Mathf.Sin(timer_HeadBobAndSprintRoll) * headBobAmplitude * speedPercent;
        float headBobX = Mathf.Cos(timer_HeadBobAndSprintRoll * 0.5f/* + Mathf.PI / 2*/) * headBobAmplitude * horizontalAmplitudeMultiplier * speedPercent;

        Vector3 headBobPosition = new Vector3(headBobX, headBobY, 0);
        targetPosition += headBobPosition;
    }

    private void ApplySprintRoll(bool isSprinting, float speedPercent)
    {
        float roll = 0;
        if (isSprinting)
        {
            roll = Mathf.Cos(timer_HeadBobAndSprintRoll * 0.5f) * sprintRollRate * speedPercent;
        }
        else
        {
            roll = 0;
        }

        Quaternion sprintRollRotation = Quaternion.Euler(0, 0, roll);
        targetRotation *= sprintRollRotation;
    }
    //private void ApplyLandingEffect()
    //{
    //    bool isGrounded = playerMovement.movementState == PlayerMovement.MovementState.Grounded;
    //    if (!wasGrounded && isGrounded)
    //    {
    //        landingVelocity = landingImpactForce;
    //    }

    //    wasGrounded = isGrounded;

    //    landingVelocity += -cameraLandingOffset * springStrength * Time.deltaTime;
    //    landingVelocity *= Mathf.Exp(-springDamping * Time.deltaTime);

    //    cameraLandingOffset += landingVelocity * Time.deltaTime;
    //    targetPosition += new Vector3(0, cameraLandingOffset, 0);
    //}

    private void ApplyLandingEffect_New()
    {
        bool isGrounded = playerMovement.movementState == PlayerMovement.MovementState.Grounded;
        if (!wasGrounded && isGrounded)
        {
            landingTimer = landingDuration;
        }
        wasGrounded = isGrounded;

        if (landingTimer > 0)
        {
            landingTimer -= Time.deltaTime;

            float t = 1f - (landingTimer / landingDuration);

            float offset = -Mathf.Sin(t * Mathf.PI) * landingAmount;
            targetPosition += new Vector3(0, offset, 0);
        }

    }

}
