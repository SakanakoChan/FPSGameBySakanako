using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;

    [Space]
    [Header("Camera look control info")]
    public float lookSensitivity_Mouse = 0.2f;
    public float lookSensitivity_Controller = 1f;
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
            lookX = InputManager.instance.mouseInput.x;
            lookY = InputManager.instance.mouseInput.y;

            sensitivity = lookSensitivity_Mouse;
        }
        else
        {
            lookX = InputManager.instance.lookInputReal.x;
            lookY = InputManager.instance.lookInputReal.y;

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

}
