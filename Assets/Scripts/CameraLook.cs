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
    public float verticalSensitivityMultiplier = 0.75f;

    [Space]
    public ResponsiveCurve responsiveCurve;
    [Range(1f, 3f)]
    public float standardCurveExponent = 1.8f;
    [Range(0f, 1f)]
    public float dynamicCurveThreshold = 0.6f;

    [Space]
    public bool enableTurnAcceleration = true;
    public AxisTurnAcceleration horizontalTurnAcceleration;
    public AxisTurnAcceleration verticalTurnAcceleration;


    [Header("Common settings")]
    public bool invertYAxis = false;

    private void Awake()
    {
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();
    }

    private void Update()
    {
        float lookDeltaX = InputManager.instance.mouseInput.x;
        float lookDeltaY = InputManager.instance.mouseInput.y;

        float sensitivity = lookSensitivity_Mouse;

        if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            sensitivity = lookSensitivity_Mouse;

            //in rewired, mouse related axis actions always return relative value
            //which already calculates the delta value between 2 frames
            //so it shouldn't be mutiplied by Time.deltaTime
            lookDeltaX = InputManager.instance.mouseInput.x * sensitivity;
            lookDeltaY = InputManager.instance.mouseInput.y * sensitivity;
        }
        else
        {
            sensitivity = lookSensitivity_Controller;

            Vector2 rawLookInput = InputManager.instance.lookInputReal;
            Vector2 processedLookInput = ApplyResponsiveCurve(rawLookInput);

            float processedLookInputX = processedLookInput.x;
            float processedLookInputY = processedLookInput.y;

            if (enableTurnAcceleration)
            {
                processedLookInputX = horizontalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.x, processedLookInputX);
                processedLookInputY = verticalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.y, processedLookInputY);
            }

            //in rewired, controller stick related axis actions always return absolute value
            //meaning the result has to be multiplied by Time.deltaTime to keep consistent
            //under different frame rates
            lookDeltaX = processedLookInputX * Time.deltaTime * sensitivity;
            lookDeltaY = processedLookInputY * Time.deltaTime * verticalSensitivityMultiplier * sensitivity;
        }

        pov.m_HorizontalAxis.Value += lookDeltaX;

        if (invertYAxis)
        {
            pov.m_VerticalAxis.Value += lookDeltaY;
        }
        else
        {
            pov.m_VerticalAxis.Value -= lookDeltaY;
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

}
