using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
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
    private CharacterController cc;


    [Header("Acceleration info")]
    [SerializeField] private float acceleration = 8;
    //[SerializeField] private float deceleration;
    [SerializeField] private float friction_WithoutMoveInput = 12;
    [SerializeField] private float friction_WithMoveInput = 6;

    [Header("Move Speed info")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    private float maxSpeed
    {
        get
        {
            if (isSprinting)
            {
                return sprintSpeed;
            }

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


    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private Vector3 currentVelocity;

    private MovementState movementState;
    private Stance stance;

    private bool isSprinting = false;
    private bool wantsToSprint = false;


    private void Start()
    {
        PauseManager.instance.OnPauseStateChanged += HandlePause;

        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 moveInput = InputManager.instance.moveInput;
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        //UpdateMovementState();

        //if (movementState == MovementState.Grounded)
        //{
        //    //to make player stand on surface and avoid sudden floating
        //    if (verticalVelocity < 0)
        //    {
        //        verticalVelocity = groundStickForce;
        //    }

        //    if (InputManager.instance.JumpPressed)
        //    {
        //        verticalVelocity = jumpForce;
        //        movementState = MovementState.Air;
        //    }
        //}

        //ApplyGravity();

        UpdateSprintState(moveInput);

        ApplyFriction(moveInput);

        ApplyAcceleration(moveInput, moveDirection);

        var moveSpeedRatio = horizontalVelocity.magnitude / maxSpeed;
        moveSpeedRatio = Mathf.Clamp01(moveSpeedRatio);
        anim.SetFloat("Movement", moveSpeedRatio, 0.1f, Time.deltaTime);

        currentVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        cc.Move(currentVelocity * Time.deltaTime);

        //Debug.Log(currentVelocity.magnitude);
    }


    private void UpdateMovementState()
    {
        if (cc.isGrounded)
        {
            movementState = MovementState.Grounded;
        }
        else
        {
            movementState = MovementState.Air;
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
            isSprinting = false;
            wantsToSprint = false;
            anim.SetBool("Running", isSprinting);
            return;
        }

        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection = moveDirection.normalized;

        //does current movement condition support sprint?
        float forwardDot = Vector3.Dot(moveDirection, transform.forward);
        bool isMovingForwardEnough = forwardDot > forwardDotThresholdToTriggerSprint;

        bool curremtMovementConditionSupportsSprint = isMovingForwardEnough && moveInputMagnitude > moveInputMagnitudeThresholdToTriggerSprint;



        //if player wishes to sprint?
        //manual sprint
        if (InputManager.instance.SprintPressed)
        {
            wantsToSprint = true;
        }

        //auto sprint settings
        if (InputManager.instance.currentInputDevice == InputDevice.Controller && GameSettings.controllerAutoSprint == true)
        {
            wantsToSprint = true;
        }


        isSprinting = wantsToSprint && curremtMovementConditionSupportsSprint;
        anim.SetBool("Running", isSprinting);
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

        float speedReduceAmount = speed * friction * Time.deltaTime;
        float reducedSpeed = speed - speedReduceAmount;
        float speedReduceRate = reducedSpeed / speed;

        horizontalVelocity *= speedReduceRate;
    }

    private void ApplyAcceleration(Vector2 moveInput, Vector3 moveDirection)
    {
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

        float actualSpeedToAdd = acceleration * Time.deltaTime * wishSpeed;
        if (actualSpeedToAdd > maxSpeedToAdd)
        {
            actualSpeedToAdd = maxSpeedToAdd;
        }

        Vector3 actualVelocityToAdd = wishDirection * actualSpeedToAdd;
        horizontalVelocity += actualVelocityToAdd;
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);
    }

    private void HandlePause(bool _gameIsPaused)
    {
        enabled = !_gameIsPaused;
    }
}
