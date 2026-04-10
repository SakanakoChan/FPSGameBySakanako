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
    public bool invertYAxis_Controller = false;
    public bool invertYAxis_Mouse = false;


    [Header("Controller Aim Assist")]
    [SerializeField] private float aaRange;
    [SerializeField] private float dotRequirementToTriggerAA = 0.98f;
    [SerializeField] private float aaSlowDownFactor = 0.5f;
    [SerializeField] private float aaRotationAssistStrength = 0.6f;
    [SerializeField] private float aaRotationAssistMaxSpeedPerFrame = 60;
    private Transform aaTarget;
    private Vector3 lastAATargetPosition_RA;
    private Transform lastAATarget_RA;
    private int enemyLayerIndex;
    private int environmentLyaerIndex;


    private CameraRecoil cameraRecoil;
    private Gun currentGun;
    private PlayerCombat playerCombat;
    private PlayerMovement playerMovement;
    private Transform camTransform;


    private void Awake()
    {
        //pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        cameraRecoil = GetComponentInChildren<CameraRecoil>();
        currentGun = GetComponentInChildren<Gun>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMovement = GetComponent<PlayerMovement>();

        camTransform = Camera.main.transform;
        enemyLayerIndex = LayerMask.GetMask("Enemy");
        environmentLyaerIndex = LayerMask.GetMask("Environment");
    }

    private void OnEnable()
    {
        lookSensitivity_Controller = GameSettings.controllerLookSensitivity;
        verticalSensitivityMultiplier_Controller = GameSettings.controllerVerticalSensitivityMultiplier;
        invertYAxis_Controller = GameSettings.invertYAxis_Controller;
        responsiveCurve = GameSettings.responsiveCurve;

        horizontalTurnAcceleration.SetupTurnAcceleration(
            GameSettings.horizontalTurnAccelerationStartDelay,
            GameSettings.horizontalTurnAccelerationRampUpTime,
            GameSettings.horizontalTurnAccelerationSensitivityMultiplier);

        verticalTurnAcceleration.SetupTurnAcceleration(
            GameSettings.verticalTurnAccelerationStartDelay,
            GameSettings.verticalTurnAccelerationRampUpTime,
            GameSettings.verticalTurnAccelerationSensitivityMultiplier);

        lookSensitivity_Mouse = GameSettings.mouseLookSensitivity;
        invertYAxis_Mouse = GameSettings.invertYAxis_Mouse;

        turnAcceleration = GameSettings.turnAcceleration;
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
                float adsCompleteSensitivity = lookSensitivity_Mouse * (currentGun.adsFOV / currentGun.hipFireFOV) * GameSettings.mouseADSSensitivityMultiplier;

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

            if (playerCombat != null && playerCombat.isInADS == false)
            {
                if (turnAcceleration)
                {
                    processedLookInputX = horizontalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.x, processedLookInputX);
                    processedLookInputY = verticalTurnAcceleration.ApplyTurnAcceleration(rawLookInput.y, processedLookInputY);
                }
            }


            if (rawLookInput.magnitude >= 0.5f)
            {
                processedLookInputY = YInputSupress(processedLookInputX, processedLookInputY);
            }

            Vector2 rotationAssist = Vector2.zero;
            if (GameSettings.controllerAimAssistEnabled)
            {
                FindAimAssistTarget();

                if (aaTarget != null)
                {
                    ApplyAimAssistSlowDown(ref processedLookInputX, ref processedLookInputY);
                    rotationAssist = ApplyAimAssistRotationAssist();
                }
            }


            //in rewired, controller stick related axis actions always return absolute value
            //meaning the result has to be multiplied by Time.deltaTime to keep consistent
            //under different frame rates
            lookDeltaX = (processedLookInputX * sensitivity + rotationAssist.x) * Time.deltaTime;
            lookDeltaY = (processedLookInputY * verticalSensitivityMultiplier_Controller * sensitivity + rotationAssist.y) * Time.deltaTime;
        }


        //Try to resist recoil
        if (cameraRecoil != null && cameraRecoil.recoilOffset.magnitude > 0)
        {
            lookDeltaX = ResistRecoil(lookDeltaX, ref cameraRecoil.recoilOffset.x);
            lookDeltaY = ResistRecoil(lookDeltaY, ref cameraRecoil.recoilOffset.y);
        }


        yaw += lookDeltaX;
        
        if (InputManager.instance.currentInputDevice == InputDevice.Controller)
        {
            if (invertYAxis_Controller)
            {
                ModifyPitch(lookDeltaY);
            }
            else
            {
                ModifyPitch(-lookDeltaY);
            }
        }
        else
        {
            if (invertYAxis_Mouse)
            {
                ModifyPitch(lookDeltaY);
            }
            else
            {
                ModifyPitch(-lookDeltaY);
            }
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



    #region Controller Input Process
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

    private float YInputSupress(float lookInputX, float lookInputY)
    {
        float absX = Mathf.Abs(lookInputX);
        float absY = Mathf.Abs(lookInputY);

        float dominance = absX / (absX + absY + 0.0001f);
        float suppression = Mathf.Lerp(1f, 0.75f, dominance);

        lookInputY *= suppression;
        return lookInputY;
    }

    private void FindAimAssistTarget()
    {
        Transform bestTarget = null;
        float bestScore = -1;

        var potentailTargetList = Physics.OverlapSphere(camTransform.position, aaRange, enemyLayerIndex);

        foreach (var target in potentailTargetList)
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.isDead == true)
                continue;

            var hitboxList = target.GetComponentsInChildren<Hitbox>();
            foreach (var hitbox in hitboxList)
            {
                Vector3 directionToHitbox = (hitbox.transform.position - camTransform.position).normalized;

                float dot = Vector3.Dot(camTransform.forward, directionToHitbox);

                if (dot >= dotRequirementToTriggerAA)
                {
                    Vector3 obstacleDetectionStartPoint = camTransform.position + camTransform.forward * 0.1f;
                    Vector3 obstacleDetectionDirection = hitbox.transform.position - obstacleDetectionStartPoint;
                    float obstacleDetectionDistance = Vector3.Distance(hitbox.transform.position, obstacleDetectionStartPoint);

                    //float castRadius = 0.1f;
                    if (!Physics.Raycast(obstacleDetectionStartPoint, obstacleDetectionDirection, out var hitInfo, obstacleDetectionDistance, environmentLyaerIndex))
                    {
                        if (dot >= bestScore)
                        {
                            bestTarget = target.transform;
                            bestScore = dot;
                        }
                    }
                }
            }
        }

        aaTarget = bestTarget;
    }


    private Vector2 CalculateRotationAssist(Transform _aaTarget)
    {
        if (_aaTarget == null || _aaTarget != lastAATarget_RA)
        {
            lastAATargetPosition_RA = _aaTarget == null ? Vector3.zero : _aaTarget.position;
            lastAATarget_RA = _aaTarget;
            return Vector2.zero;
        }

        //if (lastAATargetPosition_RA == Vector3.zero)
        //{
        //    lastAATargetPosition_RA = _aaTarget.position;
        //    return Vector2.zero;
        //}

        Vector3 aaTargetWorldVelocity = (_aaTarget.position - lastAATargetPosition_RA) / Time.deltaTime;
        lastAATargetPosition_RA = _aaTarget.position;

        Vector3 playerWorldVelocity = playerMovement.ccVelocity;
        Vector3 relativeVelocity = aaTargetWorldVelocity - playerWorldVelocity;

        float horizontalRelativeSpeed = Vector3.Dot(relativeVelocity, camTransform.right);
        float verticalRelativeSpeed = Vector3.Dot(relativeVelocity, camTransform.up);

        float distanceToTarget = Vector3.Distance(_aaTarget.position, camTransform.position);
        distanceToTarget = Mathf.Max(distanceToTarget, 1f);

        float aimBotAngularVelocityX = horizontalRelativeSpeed / distanceToTarget;
        float aimBotAngularVelocityY = verticalRelativeSpeed / distanceToTarget;

        float rotationAssisX = aimBotAngularVelocityX * Mathf.Rad2Deg * aaRotationAssistStrength;
        float rotationAssistY = aimBotAngularVelocityY * Mathf.Rad2Deg * aaRotationAssistStrength;

        rotationAssisX = Mathf.Clamp(rotationAssisX, -aaRotationAssistMaxSpeedPerFrame, aaRotationAssistMaxSpeedPerFrame);
        rotationAssistY = Mathf.Clamp(rotationAssistY, -aaRotationAssistMaxSpeedPerFrame, aaRotationAssistMaxSpeedPerFrame);

        Vector2 rotationAssist = new Vector2(rotationAssisX, rotationAssistY);
        return rotationAssist;
    }

    private Vector2 ApplyAimAssistRotationAssist()
    {
        return CalculateRotationAssist(aaTarget);
    }

    private void ApplyAimAssistSlowDown(ref float processedLookInputX, ref float processedLookInputY)
    {
        processedLookInputX *= aaSlowDownFactor;
        processedLookInputY *= aaSlowDownFactor;
    }
    #endregion


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
