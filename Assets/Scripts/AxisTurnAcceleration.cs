using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxisTurnAcceleration
{
    [Header("Turn acceleration info")]
    [Range(0f, 1f)] [SerializeField] private float turnAccelerationStickThreshold = 0.9f;
    [SerializeField] private float turnAccelerationStartDelay = 0.2f;
    [SerializeField] private float turnAccelerationRampUpTime = 0.3f;
    [Range(1f, 3f)] [SerializeField] private float maxTurnAccelerationSensitivityMultiplier = 2f;

    private float turnAccelerationTimer = 0;
    private float currentTurnAccelerationSensitvityMultiplier = 1f;

    public float ApplyTurnAcceleration(float _rawAxisInputValue, float _axisInputToAddTurnAcceleration)
    {
        float magnitude = Mathf.Abs(_rawAxisInputValue);

        if (magnitude >= turnAccelerationStickThreshold)
        {
            turnAccelerationTimer += Time.deltaTime;

            if (turnAccelerationTimer > turnAccelerationStartDelay)
            {
                float t = (turnAccelerationTimer - turnAccelerationStartDelay) / turnAccelerationRampUpTime;
                t = Mathf.Clamp01(t);

                currentTurnAccelerationSensitvityMultiplier = Mathf.Lerp(1, maxTurnAccelerationSensitivityMultiplier, t);
            }
            else
            {
                currentTurnAccelerationSensitvityMultiplier = 1f;
            }
        }
        else
        {
            currentTurnAccelerationSensitvityMultiplier = 1f;
            turnAccelerationTimer = 0;
        }

        return _axisInputToAddTurnAcceleration * currentTurnAccelerationSensitvityMultiplier;
    }
}
