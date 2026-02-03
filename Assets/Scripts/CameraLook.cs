using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResponsiveCurve
{
    Standard,
    Linear,
    Dynamic
}

public class CameraLook : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;

    [Space]
    [Header("Mouse look control info")]
    public float lookSensitivity_Mouse = 0.2f;

    [Header("Controller look control info")]
    public float lookSensitivity_Controller = 1f;

    [Space]
    public ResponsiveCurve responsiveCurve;
    [Range(1f, 3f)]
    public float standardCurveExponent = 1.8f;
    [Range(0f, 1f)]
    public float dynamicCurveThreshold = 0.6f;

    [Space]
    public bool enableTurnAcceleration = true;
    [Range(0f, 1f)] public float turnAccelerationStickThreshold = 0.9f;
    public float turnAccelerationStartDelay = 0.2f;
    public float turnAccelerationRampUpTime = 0.3f;
    [Range(1f, 3f)] public float maxTurnAccelerationSensitivityMultiplier = 2f;

    private float turnAccelerationTimer = 0;
    private float currentTurnAccelerationSensitvityMultiplier = 1f;


    [Header("Common settings")]
    public bool invertYAxis = false;

    private void Awake()
    {
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();
    }

    private void Update()
    {
        float lookX = InputManager.instance.mouseInput.x;
        float lookY = InputManager.instance.mouseInput.y;

        float sensitivity = lookSensitivity_Mouse;

        if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            //in rewired, mouse related axis actions always return relative value
            //which already calculates the delta value between 2 frames
            //so it shouldn't be mutiplied by Time.deltaTime
            lookX = InputManager.instance.mouseInput.x;
            lookY = InputManager.instance.mouseInput.y;

            sensitivity = lookSensitivity_Mouse;
        }
        else
        {
            Vector2 processedLookInput = ApplyResponsiveCurve(InputManager.instance.lookInputReal);
            processedLookInput = ApplyTurnAcceleration(processedLookInput);

            //in rewired, controller stick related axis actions always return absolute value
            //meaning the result has to be multiplied by Time.deltaTime to keep consistent
            //under different frame rates
            lookX = processedLookInput.x * Time.deltaTime;
            lookY = processedLookInput.y * Time.deltaTime;

            sensitivity = lookSensitivity_Controller;
        }

        pov.m_HorizontalAxis.Value += lookX * sensitivity;

        if (invertYAxis)
        {
            pov.m_VerticalAxis.Value += lookY * sensitivity;
        }
        else
        {
            pov.m_VerticalAxis.Value -= lookY * sensitivity;
        }
    }


    private Vector2 ApplyResponsiveCurve(Vector2 _lookInput)
    {
        float magnitude = _lookInput.magnitude;
        if (magnitude <= 0f) return Vector2.zero;

        Vector2 direction = _lookInput / magnitude;
        //same as
        //Vector2 direction = input.normalized;
        float curvedMagnitude = magnitude;

        switch (responsiveCurve)
        {
            case ResponsiveCurve.Linear:
                curvedMagnitude = magnitude;
                break;

            case ResponsiveCurve.Standard:
                curvedMagnitude = Mathf.Pow(magnitude, standardCurveExponent);
                break;

            case ResponsiveCurve.Dynamic:
                if (magnitude < dynamicCurveThreshold)
                {
                    // 小输入：标准曲线
                    curvedMagnitude = Mathf.Pow(magnitude, standardCurveExponent);
                }
                else
                {
                    // 大输入：逐渐转线性
                    float t = Mathf.InverseLerp(dynamicCurveThreshold, 1f, magnitude);
                    float standard = Mathf.Pow(magnitude, standardCurveExponent);
                    curvedMagnitude = Mathf.Lerp(standard, magnitude, t);
                }
                break;
        }

        return direction * curvedMagnitude;
    }

    private Vector2 ApplyTurnAcceleration(Vector2 _lookInput)
    {
        if (enableTurnAcceleration == false)
        {
            return _lookInput;
        }

        float magnitude = _lookInput.magnitude;

        if (magnitude >= turnAccelerationStickThreshold)
        {
            turnAccelerationTimer += Time.deltaTime;

            if (turnAccelerationTimer > turnAccelerationStartDelay)
            {
                float t = (turnAccelerationTimer - turnAccelerationStartDelay) / turnAccelerationRampUpTime;
                t = Mathf.Clamp01(t);

                currentTurnAccelerationSensitvityMultiplier = Mathf.Lerp(1f, maxTurnAccelerationSensitivityMultiplier, t);
            }
            else
            {
                currentTurnAccelerationSensitvityMultiplier = 1f;
            }
        }
        else
        {
            turnAccelerationTimer = 0f;
            currentTurnAccelerationSensitvityMultiplier = 1f;
        }

        return _lookInput * currentTurnAccelerationSensitvityMultiplier;
    }
}
