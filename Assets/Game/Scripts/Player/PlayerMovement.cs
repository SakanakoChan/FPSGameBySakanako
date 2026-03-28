using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum GroundedState
    {
        Grounded,
        Air
    }

    public enum Stance
    {
        Stand,
        Crouch
    }


    [SerializeField] private Animator anim;
    [SerializeField] private Transform cameraPivot;
    private CharacterController cc;

    private PlayerCombat playerCombat;

    [Header("Stance info")]
    [SerializeField] private float standHeight_CC = 2f;
    [SerializeField] private float crouchHeight_CC = 1.55f;
    [SerializeField] private float stanceHeightSmoothSpeed = 4f;
    private float bottomY;

    [Space]
    [SerializeField] private float standHeight_CamperaPivot = 0f;
    [SerializeField] private float crouchHeight_CamperaPivot = -0.8f;
    private Stance currentStance;
    private bool wantsToCrouch = false;
    private bool isCrouching = false;

    [Header("Slide info")]
    [SerializeField] private float requiredSpeedToTriggerSlide = 6f;
    [SerializeField] private float slideInitialSpeedBoost = 5f;
    [SerializeField] private float slideInitialSpeedBoostRatio = 1.5f;
    [SerializeField] private float slideFriction = 10f;
    [SerializeField] private float maxSlideTime = 1f;
    [SerializeField] private CameraKick cameraKick_Movement;
    [SerializeField] private float slideCameraKickStreangth = 2f;
    private float slideTimer;
    private bool isSliding = false;
    private bool wantsToSlide = false;
    private Vector3 slideDirection;
    private bool slideInputLock = false;


    [Header("Acceleration info")]
    [SerializeField] private float acceleration_Grounded = 8;
    [SerializeField] private float acceleration_Air = 2;

    //friction works only if player is grounded
    [SerializeField] private float friction_WithoutMoveInput = 12;
    [SerializeField] private float friction_WithMoveInput = 6;

    [Header("Move Speed info")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float crouchWalkSpeed = 2.7f;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float ADSWalkSpeed = 3;
    [SerializeField] private float crouchADSWalkSpeed = 2.2f;
    public float maxSpeed
    {
        get
        {
            if (isSprinting)
            {
                return sprintSpeed;
            }

            if (playerCombat != null && playerCombat.isInADS)
            {
                if (currentStance == Stance.Stand)
                    return ADSWalkSpeed;

                if (currentStance == Stance.Crouch)
                    return crouchADSWalkSpeed;
            }


            if (currentStance == Stance.Crouch)
                return crouchWalkSpeed;

            return walkSpeed;
        }
    }


    [Space]
    [SerializeField] private float moveInputMagnitudeThresholdToTriggerSprint = 0.7f;
    [SerializeField] private float forwardDotThresholdToTriggerSprint = 0.5f;


    [Header("Jump info")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundStickForce = -2f;


    public Vector3 horizontalVelocity { get; private set; }
    private float verticalVelocity;
    public Vector3 currentVelocity { get; private set; }
    public Vector3 ccVelocity => cc.velocity;

    public Vector3 localVelocity { get { return transform.InverseTransformVector(currentVelocity); } }


    public GroundedState groundedState { get; private set; }
    private Stance stance = Stance.Stand;

    public bool isSprinting { get; private set; } = false;
    private bool wantsToSprint = false;
    private bool wasSprintingInLastFrame = false;

    private int environmentLayerIndex = 0;

    private void Awake()
    {
        PlayerReference.SetPlayerTrasnform(transform);
    }

    private void Start()
    {
        PauseManager.instance.OnPauseStateChanged += HandlePause;

        cc = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();

        bottomY = cc.center.y - 0.5f * cc.height;
        environmentLayerIndex = LayerMask.GetMask("Environment");

        //Application.targetFrameRate = 30;
    }

    private void Update()
    {
        Vector2 moveInput = InputManager.instance.moveInput;
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        UpdateGroundedState();

        CrouchAndSlideControl();

        SlideLogic();

        JumpControl();

        ApplyGravity();

        UpdateSprintState(moveInput);

        ApplyFriction(moveInput);

        ApplyAcceleration(moveInput, moveDirection);

        UpdateMovementAnimation();

        currentVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        cc.Move(currentVelocity * Time.deltaTime);

        //Debug.Log(horizontalVelocity.magnitude);
    }



    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(hit.moveDirection * currentVelocity.magnitude * 1.5f, ForceMode.Impulse);
        }
    }


    private void UpdateGroundedState()
    {
        if (cc.isGrounded)
        {
            groundedState = GroundedState.Grounded;
        }
        else
        {
            groundedState = GroundedState.Air;
        }
    }

    private bool CheckIfCanStandUp()
    {
        float radius = cc.radius * 1f;

        Vector3 centerWorldPosition = transform.TransformPoint(cc.center);
        Vector3 feetPosition = centerWorldPosition - Vector3.up * (cc.height * 0.5f);

        Vector3 start = feetPosition + Vector3.up * radius;
        Vector3 end = feetPosition + Vector3.up * (standHeight_CC - radius);
        
        bool result = !Physics.CheckCapsule(start, end, radius, environmentLayerIndex);
        return result;
    }

    private void CrouchAndSlideControl()
    {
        if (CheckIfCanStandUp() == false)
        {
            return;
        }

        if (InputManager.instance.CrouchPressed)
        {
            wantsToCrouch = !wantsToCrouch;
            wantsToSprint = false;

            if (horizontalVelocity.magnitude >= requiredSpeedToTriggerSlide && wantsToCrouch == true)
            {
                wantsToSlide = true;
            }

            if (isSliding)
            {
                StopSlide();
                wantsToCrouch = true;
                wantsToSlide = false;
            }
        }

        bool currentMovementStateSupportsCrouch =
            groundedState == GroundedState.Grounded;

        isCrouching = wantsToCrouch && currentMovementStateSupportsCrouch;

        UpdateStance();
    }

    private void UpdateStance()
    {
        currentStance = isCrouching ? Stance.Crouch : Stance.Stand;

        float targetHeight_CC = standHeight_CC;
        float targetHeight_Pivot = standHeight_CamperaPivot;
        if (currentStance == Stance.Stand)
        {
            targetHeight_CC = standHeight_CC;
            targetHeight_Pivot = standHeight_CamperaPivot;
        }
        else if (currentStance == Stance.Crouch)
        {
            targetHeight_CC = crouchHeight_CC;
            targetHeight_Pivot = crouchHeight_CamperaPivot;
        }

        cc.height = Mathf.MoveTowards(cc.height, targetHeight_CC, stanceHeightSmoothSpeed * Time.deltaTime);

        Vector3 targetPivotPosition = new Vector3(0, targetHeight_Pivot, 0);
        cameraPivot.localPosition = Vector3.MoveTowards(cameraPivot.localPosition, targetPivotPosition, stanceHeightSmoothSpeed * Time.deltaTime);

        Vector3 newCenter = cc.center;
        newCenter.y = bottomY + (cc.height * 0.5f);
        cc.center = newCenter;
    }

    private void SlideLogic()
    {
        if (!isSliding /*&& horizontalVelocity.magnitude >= requiredSpeedToTriggerSlide*/ && wantsToSlide && isCrouching)
        {
            StartSlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            //***this friction control is written in ApplyFriction as well***
            //float speed = horizontalVelocity.magnitude;
            //float reduceAmount = slideFriction * Time.deltaTime;
            //horizontalVelocity = slideDirection * Mathf.Max(speed - reduceAmount, 0f);

            Vector3 cameraKickImpulse = new Vector3(0, 0, horizontalVelocity.magnitude * slideCameraKickStreangth * Time.deltaTime);
            cameraKick_Movement?.AddCameraKick(cameraKickImpulse);

            if (slideTimer <= 0 || horizontalVelocity.magnitude <= crouchWalkSpeed || groundedState == GroundedState.Air)
            {
                StopSlide();
            }
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        wantsToSlide = false;
        slideTimer = maxSlideTime;

        slideDirection = horizontalVelocity.normalized;

        horizontalVelocity = slideDirection * (horizontalVelocity.magnitude * slideInitialSpeedBoostRatio/*+ slideInitialSpeedBoost*/);
    }

    private void StopSlide()
    {
        isSliding = false;
    }


    private void JumpControl()
    {
        if (groundedState == GroundedState.Grounded)
        {
            //to make player stand on surface and avoid sudden floating
            if (verticalVelocity < 0)
            {
                verticalVelocity = groundStickForce;
            }

            if (InputManager.instance.JumpPressed)
            {
                if (currentStance == Stance.Stand)
                {
                    verticalVelocity = jumpForce;
                    groundedState = GroundedState.Air;
                }
                else if (currentStance == Stance.Crouch)
                {
                    wantsToCrouch = false;
                    StopSlide();
                }
            }
        }
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }


    private void UpdateSprintState(Vector2 moveInput)
    {
        float moveInputMagnitude = moveInput.magnitude;

        if (moveInputMagnitude < 0.01f)
        {
            wantsToSprint = false;
        }

        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection = moveDirection.normalized;

        //does current movement condition support sprint?
        float forwardDot = Vector3.Dot(moveDirection, transform.forward);
        bool isMovingForwardEnough = forwardDot > forwardDotThresholdToTriggerSprint;

        bool currentMovementConditionSupportsSprint =
            isMovingForwardEnough &&
            moveInputMagnitude > moveInputMagnitudeThresholdToTriggerSprint &&
            groundedState == GroundedState.Grounded;



        //if player wishes to sprint?
        //manual sprint
        if (InputManager.instance.SprintPressed)
        {
            if (currentStance == Stance.Crouch && CheckIfCanStandUp() == false)
            {
                return;
            }

            wantsToSprint = true;
            wantsToCrouch = false;
            //currentStance = Stance.Stand;

            if (isSliding)
            {
                StopSlide();
            }
        }

        //auto sprint settings
        if (playerCombat != null && playerCombat.isInADS == false && playerCombat.isTryingToFire == false)
        {
            if (InputManager.instance.currentInputDevice == InputDevice.Controller &&
                GameSettings.controllerAutoSprint == true &&
                wantsToCrouch == false)
            {
                wantsToSprint = true;
            }
        }

        bool currentCombatConditionSupportsSprint = !playerCombat.isInADS && !playerCombat.isTryingToFire && !playerCombat.isReloading;


        isSprinting = wantsToSprint && currentMovementConditionSupportsSprint && currentCombatConditionSupportsSprint;
        anim.SetBool("Running", isSprinting);


        //add sprint to fire delay
        if (wasSprintingInLastFrame == true && isSprinting == false)
        {
            playerCombat?.StartSprintToFireDelay();
        }

        wasSprintingInLastFrame = isSprinting;
    }

    private void ApplyFriction(Vector2 _moveInput)
    {
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.01f)
        {
            horizontalVelocity = Vector3.zero;
            return;
        }

        float friction = _moveInput.sqrMagnitude > 0.01f ? friction_WithMoveInput : friction_WithoutMoveInput;
        if (groundedState == GroundedState.Air)
        {
            friction = 0;
        }

        if (isSliding)
        {
            friction = slideFriction;
        }

        float speedReduceAmount = speed * friction * Time.deltaTime;
        float reducedSpeed = speed - speedReduceAmount;
        float speedReduceRate = reducedSpeed / speed;

        horizontalVelocity *= speedReduceRate;
    }

    private void ApplyAcceleration(Vector2 moveInput, Vector3 moveDirection)
    {
        if (isSliding)
            return;

        Vector3 wishDirection = moveDirection.normalized;
        float wishSpeed = maxSpeed * moveInput.magnitude;
        if (isSprinting)
        {
            wishSpeed = maxSpeed;
        }
        else
        {
            wishSpeed = maxSpeed * moveInput.magnitude;
        }

        float currentSpeedOnWishDirection = Vector3.Dot(horizontalVelocity, wishDirection);
        float maxSpeedToAdd = wishSpeed - currentSpeedOnWishDirection;

        if (maxSpeedToAdd <= 0)
        {
            return;
        }

        float actualAcceleration =
            groundedState == GroundedState.Grounded ? acceleration_Grounded : acceleration_Air;

        float actualSpeedToAdd = actualAcceleration * Time.deltaTime * wishSpeed;
        if (actualSpeedToAdd > maxSpeedToAdd)
        {
            actualSpeedToAdd = maxSpeedToAdd;
        }

        Vector3 actualVelocityToAdd = wishDirection * actualSpeedToAdd;
        horizontalVelocity += actualVelocityToAdd;

        if (groundedState == GroundedState.Grounded)
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);
    }

    private void UpdateMovementAnimation()
    {
        var moveSpeedRatio = horizontalVelocity.magnitude / walkSpeed;
        moveSpeedRatio = Mathf.Clamp01(moveSpeedRatio);
        if (groundedState == GroundedState.Air || isSliding)
        {
            moveSpeedRatio = 0;
        }
        anim.SetFloat("Movement", moveSpeedRatio, 0.1f, Time.deltaTime);
    }

    private void HandlePause(bool _gameIsPaused)
    {
        enabled = !_gameIsPaused;
    }
}
