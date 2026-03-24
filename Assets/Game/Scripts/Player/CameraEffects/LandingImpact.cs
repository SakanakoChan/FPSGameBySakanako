using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingImpact : MonoBehaviour
{
    private bool wasGrounded = true;
    [SerializeField] private float landingImpactForce = -0.15f;
    private float landingVelocity;
    private float cameraLandingOffset;

    [SerializeField] private float springStrength = 120f;
    [SerializeField] private float springDamping = 15f;

    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        bool isGrounded = playerMovement.groundedState == PlayerMovement.GroundedState.Grounded;
        if (!wasGrounded && isGrounded)
        {
            landingVelocity = landingImpactForce;
        }

        wasGrounded = isGrounded;

        landingVelocity += -cameraLandingOffset * springStrength * Time.deltaTime;
        landingVelocity *= Mathf.Exp(-springDamping * Time.deltaTime);

        cameraLandingOffset += landingVelocity * Time.deltaTime;
        transform.localPosition = new Vector3(0, cameraLandingOffset, 0);
    }
}
