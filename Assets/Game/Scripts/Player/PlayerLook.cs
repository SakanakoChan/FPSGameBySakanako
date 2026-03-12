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

public enum ADSSensitivityTransition
{
    Instant,
    Gradual,
    AfterADS
}

public class PlayerLook : MonoBehaviour
{
    //public CinemachineVirtualCamera vcam;
    //private CinemachinePOV pov;

    [Header("Transform to apply rotation")]
    [Tooltip("Transform to apply horizontal rotation")]
    [SerializeField] private Transform player;
    [Tooltip("Transform to apply the vertical rotation")]
    [SerializeField] private Transform cameraPivot;

    [Header("Vertical angle limit")]
    [SerializeField] private float pitchUpperLimit = 70;
    [SerializeField] private float pitchLowerLimit = -70;

    public float yaw { get; private set; }
    public float pitch { get; private set; }


    [Space]
    [Header("Mouse look control info")]
    public float lookSensitivity_Mouse = 0.2f;

    [Header("Controller look control info")]
    public float lookSensitivity_Controller = 1f;
    public float verticalSensitivityMultiplier_Controller = 0.75f;

    [Space]
    public ResponsiveCurve responsiveCurve;
    [Range(1f, 3f)]
    public float standardCurveExponent = 1.8f;
    [Range(0f, 1f)]
    public float dynamicCurveThreshold = 0.6f;

    [Space]
    public bool turnAcceleration = true;
    public AxisTurnAcceleration horizontalTurnAcceleration;
    public AxisTurnAcceleration verticalTurnAcceleration;


    [Header("Common settings")]
    public bool invertYAxis = false;


    private CameraRecoil cameraRecoil;
    private Gun currentGun;
    private PlayerCombat playerCombat;


    private void Awake()
    {
        //pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        cameraRecoil = GetComponentInChildren<CameraRecoil>();
        currentGun = GetComponentInChildren<Gun>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        lookSensitivity_Controller = GameSettings.controllerLookSensitivity;
        verticalSensitivityMultiplier_Controller = GameSettings.controllerVerticalSensitivityMultiplier;
        invertYAxis = GameSettings.invertYAxis;
        responsiveCurve = GameSettings.responsiveCurve;

        horizontalTurnAcceleration.SetupTurnAcceleration(
            GameSettings.horizontalTurnAccelerationStartDelay,
            GameSettings.horizontalTurnAccelerationRampUpTime,
            GameSettings.horizontalTurnAccelerationSensitivityMultiplier);

        verticalTurnAcceleration.SetupTurnAcceleration(
            GameSettings.verticalTurnAccelerationStartDelay,
            GameSettings.verticalTurnAccelerationRampUpTime,
            GameSettings.verticalTurnAccelerationSensitivityMultiplier);
    }

    private void Start()
    {
        PauseManager.instance.OnPauseStateChanged += HandlePause;
    }


    private void Update()
    {
        float lookDeltaX = InputManager.instance.mouseInput.x;
        float lookDeltaY = InputManager.instance.mouseInput.y;

        float sensitivity = lookSensitivity_Mouse;

        //Mouse
        if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            sensitivity = lookSensitivity_Mouse;

            //apply ads sensitivity transition
            if (currentGun != null)
            {
                float adsCompleteSensitivity = lookSensitivity_Mouse * (currentGun.adsFOV / currentGun.hipFireFOV);

                sensitivity = ApplyADSSensitivityTransition(lookSensitivity_Mouse, adsCompleteSensitivity);
            }

            //Debug.Log("Mouse Sensitivity: " + sensitivity);


            //in rewired, mouse related axis actions always return relative value
            //which already calculates the delta value between 2 frames
            //so it shouldn't be mutiplied by Time.deltaTime
            lookDeltaX = InputManager.instance.mouseInput.x * sensitivity;
            lookDeltaY = InputManager.instance.mouseInput.y * sensitivity;
        }
        //Controller
        else
        {
            sensitivity = lookSensitivity_Controller;

            //apply ads sensitivity transition
            if (currentGun != null)
            {
                float adsCompleteSensitivity = lookSensitivity_Controller * (currentGun.adsFOV / currentGun.hipFireFOV) * GameSettings.controllerADSSensitivityMultiplier;

                sensitivity = ApplyADSSensitivityTransition(lookSensitivity_Controller, adsCompleteSensitivity);
            }

            Vector2 rawLookInput = InputManager.instance.lookInput;
            Vector2 processedLookInput = ApplyResponsiveCurve(rawLookInput);

            float processedLookInputX = processedLookInput.x;
            float processedLookInputY = processedLookInput.y;

            if (playerCombat != null && playerCombat.armIsInADS == false)
            {
                if (turnAcceleration)
                {
                    processedLookInputX = horizontalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.x, processedLookInputX);
                    processedLookInputY = verticalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.y, processedLookInputY);
                }
            }


            //in rewired, controller stick related axis actions always return absolute value
            //meaning the result has to be multiplied by Time.deltaTime to keep consistent
            //under different frame rates
            lookDeltaX = processedLookInputX * Time.deltaTime * sensitivity;
            lookDeltaY = processedLookInputY * Time.deltaTime * verticalSensitivityMultiplier_Controller * sensitivity;
        }


        //Try to resist recoil
        if (cameraRecoil != null && cameraRecoil.recoilOffset.magnitude > 0)
        {
            lookDeltaX = ResistRecoil(lookDeltaX, ref cameraRecoil.recoilOffset.x);
            lookDeltaY = ResistRecoil(lookDeltaY, ref cameraRecoil.recoilOffset.y);
        }


        yaw += lookDeltaX;

        if (invertYAxis)
        {
            ModifyPitch(lookDeltaY);
        }
        else
        {
            ModifyPitch(-lookDeltaY);
        }

        float finalYaw = yaw + cameraRecoil.recoilOffset.x;
        float finalPitch = pitch - cameraRecoil.recoilOffset.y;


        //player.rotation = Quaternion.Euler(0, yaw, 0);
        //cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
        player.rotation = Quaternion.Euler(0, finalYaw, 0);
        cameraPivot.localRotation = Quaternion.Euler(finalPitch, 0, 0);
    }

    private void OnDestroy()
    {
        PauseManager.instance.OnPauseStateChanged -= HandlePause;
    }


    private void ModifyPitch(float _deltaValue)
    {
        pitch += _deltaValue;
        pitch = Mathf.Clamp(pitch, pitchLowerLimit, pitchUpperLimit);
    }

    private float ApplyADSSensitivityTransition(float _hipFireSensitivity, float _adsCompleteSensitivity)
    {
        float sensitivity;
        switch (GameSettings.mouseADSSensitivityTransition)
        {
            case ADSSensitivityTransition.Gradual:
                sensitivity = Mathf.Lerp(_hipFireSensitivity, _adsCompleteSensitivity, currentGun.adsAlpha);
                break;

            case ADSSensitivityTransition.Instant:
                sensitivity = currentGun.isInADS ? _adsCompleteSensitivity : _hipFireSensitivity;
                break;

            case ADSSensitivityTransition.AfterADS:
                sensitivity = currentGun.adsAlpha >= 1 ? _adsCompleteSensitivity : _hipFireSensitivity;
                break;

            default:
                sensitivity = Mathf.Lerp(_hipFireSensitivity, _adsCompleteSensitivity, currentGun.adsAlpha);
                break;
        }

        return sensitivity;
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

    private float ResistRecoil(float _lookDelta, ref float _recoilOffset)
    {
        if (Mathf.Sign(_lookDelta) == -Mathf.Sign(_recoilOffset))
        {
            float delta = Mathf.Sign(_lookDelta) * Mathf.Min(Mathf.Abs(_lookDelta), Mathf.Abs(_recoilOffset));
            _recoilOffset += delta;
            _lookDelta -= delta;
        }

        return _lookDelta;
    }

    private void HandlePause(bool _gameIsPaused)
    {
        enabled = !_gameIsPaused;
    }

}
