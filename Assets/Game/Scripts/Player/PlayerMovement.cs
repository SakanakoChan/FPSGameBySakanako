using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;

    [Header("Acceleration info")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;

    [Header("Walk info")]
    [SerializeField] private float walkSpeed;

    private Vector3 currentVelocity;

    private void Start()
    {
        PauseManager.instance.OnPauseStateChanged += HandlePause;

        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 moveInput = InputManager.instance.moveInput;
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        Vector3 targetVelocity = moveDirection * walkSpeed;
        
        if (moveInput.sqrMagnitude > 0.01f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deceleration * Time.deltaTime);
        }

        cc.Move(currentVelocity * Time.deltaTime);
    }

    private void HandlePause(bool _gameIsPaused)
    {
        enabled = !_gameIsPaused;
    }
}
