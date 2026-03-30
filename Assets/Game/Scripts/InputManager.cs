using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputDevice
{
    MouseAndKeyboard,
    Controller
}

public enum ControllerLayout
{
    XBox,
    PlayStation,
    NintendoSwitch
}

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }

    [Header("Rewired input info")]
    [SerializeField] private int playerIDForRewired = 0;
    private Player player;

    private Vector2 moveInputRaw;
    public Vector2 moveInput { get; private set; }

    private Vector2 lookInputRaw;
    public Vector2 lookInput { get; private set; }

    public Vector2 mouseInput { get; private set;  }


    [Header("Dead zone info")]
    [Tooltip(
        "Inner dead zone. Stick input below this value is ignored.\n" +
        "Used to prevent stick drift."
    )]
    [SerializeField] private float innerDeadZone = 0.04f;

    [Tooltip(
        "Outer dead zone. Defines how far the stick must be pushed to reach maximum input.\n" +
        "A value of 1 means no outer dead zone."
    )]
    [SerializeField] private float outerDeadZone = 1f;


    [Header("Input Device Switch info")]
    public float deadZoneToTriggerControllerInput = 0.2f;
    public float mouseDeltaThresholdToTriggerMouseInput = 0.5f;

    public InputDevice currentInputDevice { get; private set; } = InputDevice.MouseAndKeyboard;
    private InputDevice previousInputDevice = InputDevice.MouseAndKeyboard;

    public ControllerLayout currentControllerLayout { get; private set; } = ControllerLayout.XBox;
    private ControllerLayout previousControllerLayout = ControllerLayout.XBox;

    public Joystick currentActiveJoystick { get; private set; } = null;

    #region Input action properties
    public bool FireHeld => player.GetButton("Fire") /*|| player.GetAxis("Fire") >= 0.2f*/;
    public bool AimDownSightHeld => player.GetButton("Aim Down Sight") /*|| player.GetAxis("Aim Down Sight") >= 0.2f*/;
    public bool JumpPressed => player.GetButtonDown("Jump");
    public bool CrouchPressed => player.GetButtonDown("Crouch");
    public bool ReloadPressed => player.GetButtonDown("Reload");
    public bool SwitchWeaponPressed => player.GetButtonDown("Switch Weapon");
    public bool SprintPressed => player.GetButtonDown("Sprint");
    public bool OpenPauseMenuPressed => player.GetButtonDown("Open Pause Menu");
    public bool UICancelPressed => player.GetButtonDown("UI Cancel");
    public bool UIConfirmPressed => player.GetButtonDown("UI Confirm");
    public bool UISwitchPageRightPressed => player.GetButtonDown("UI Switch Page Right");
    public bool UISwitchPageLeftPressed => player.GetButtonDown("UI Switch Page Left");
    public float UIHorizontal => player.GetAxisRaw("UI Horizontal");
    public float UIVertical => player.GetAxisRaw("UI Vertical");
    #endregion


    public event System.Action<InputDevice> OnInputDeviceChanged;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SyncDeadzoneFromSettings();
    }

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerIDForRewired);

        SetAllControllersRawInputDeadZonesToZero();
    }

    private void Update()
    {
        SyncDeadzoneFromSettings();

        moveInputRaw = new Vector2(player.GetAxisRaw("Move Horizontal"), player.GetAxisRaw("Move Vertical"));
        lookInputRaw = new Vector2(player.GetAxisRaw("Look Horizontal"), player.GetAxisRaw("Look Vertical"));

        moveInput = ProcessStickInput(moveInputRaw, innerDeadZone, outerDeadZone);
        lookInput = ProcessStickInput(lookInputRaw, innerDeadZone, outerDeadZone);

        mouseInput = new Vector2(player.GetAxisRaw("MouseX"), player.GetAxisRaw("MouseY"));

        InputDeviceDetection();

        DetectCurrentControllerLayout();


        //DetectControllerInputElement();

        //foreach (var controller in player.controllers.Controllers)
        //{
        //    Debug.Log($"Controller: {controller.name}, Type: {controller.type}");
        //}

        //    foreach (var map in player.controllers.maps.GetMaps(controller))
        //    {
        //        if (map.enabled)
        //        {
        //            Debug.Log($"map category id: {map.categoryId}");
        //        }
        //    }
        //}
    }

    private void SyncDeadzoneFromSettings()
    {
        innerDeadZone = GameSettings.innerDeadzone;
        outerDeadZone = GameSettings.outerDeadzone;
    }


    public void EnterUIMapMode(bool _value)
    {
        player.controllers.maps.SetMapsEnabled(_value, "UI");
        player.controllers.maps.SetMapsEnabled(!_value, "Default");
    }


    private void InputDeviceDetection()
    {
        bool hasKeyboardInput = ReInput.controllers.Keyboard.GetAnyButtonDown();
        if (mouseInput.sqrMagnitude > mouseDeltaThresholdToTriggerMouseInput || hasKeyboardInput)
        {
            currentInputDevice = InputDevice.MouseAndKeyboard;

            if (previousInputDevice != currentInputDevice)
            {
                Debug.Log("Current input device: " + currentInputDevice);
                OnInputDeviceChanged?.Invoke(currentInputDevice);
            }

            previousInputDevice = currentInputDevice;
            return;
        }

        bool hasControllerInput = false;
        foreach (var joystick in ReInput.controllers.Joysticks)
        {
            if (joystick.GetAnyButtonDown())
            {
                hasControllerInput = true;
                currentActiveJoystick = joystick;
                break;
            }

            //only detect left stick, right stick and left trigger, right trigger input
            for (int i = 0; i < joystick.axisCount && i < 6; i++)
            {
                var axisMagnitude = Mathf.Abs(joystick.GetAxis(i));
                if (axisMagnitude > deadZoneToTriggerControllerInput)
                {
                    hasControllerInput = true;
                    currentActiveJoystick = joystick;
                    break;
                }
            }
        }

        if (hasControllerInput)
        {
            currentInputDevice = InputDevice.Controller;

            if (previousInputDevice != currentInputDevice)
            {
                Debug.Log("Current input device: " + currentInputDevice);
                OnInputDeviceChanged?.Invoke(currentInputDevice);
            }

            previousInputDevice = currentInputDevice;
        }


        //***Old but convenient way***
        //var lastController = ReInput.controllers.GetLastActiveController();
        //if (lastController != null)
        //{
        //    if (lastController.type == ControllerType.Joystick)
        //    {
        //        currentInputDevice = InputDevice.Controller;
        //    }
        //    else if (lastController.type == ControllerType.Mouse || lastController.type == ControllerType.Keyboard)
        //    {
        //        currentInputDevice = InputDevice.MouseAndKeyboard;
        //    }

        //    if (previousInputDevice != currentInputDevice)
        //    {
        //        Debug.Log("Current input device: " + currentInputDevice);
        //        OnInputDeviceChanged?.Invoke(currentInputDevice);
        //    }

        //    previousInputDevice = currentInputDevice;
        //}

    }

    private void DetectCurrentControllerLayout()
    {
        //Controller lastInputDevice = player.controllers.GetLastActiveController();
        //if (lastInputDevice != null && lastInputDevice.type == ControllerType.Joystick)
        //{
        //    currentActiveJoystick = lastInputDevice as Joystick;
        //}


        if (currentActiveJoystick != null)
        {
            string controllerName = currentActiveJoystick.name;

            if (controllerName.Contains("Sony") || controllerName.Contains("Dual"))
            {
                currentControllerLayout = ControllerLayout.PlayStation;
            }
            else if (controllerName.Contains("Nintendo") || controllerName.Contains("Switch"))
            {
                currentControllerLayout = ControllerLayout.NintendoSwitch;
            }
            else
            {
                currentControllerLayout = ControllerLayout.XBox;
            }

            if (previousControllerLayout != currentControllerLayout)
            {
                Debug.Log("Current controller layout: " + currentControllerLayout);
            }
            previousControllerLayout = currentControllerLayout;
        }
    }

    private void DetectControllerInputElement()
    {
        if (currentActiveJoystick != null)
        {
            if (currentActiveJoystick.GetAnyButtonDown())
            {
                ControllerPollingInfo pollingInfo = currentActiveJoystick.PollForFirstElementDown();

                if (pollingInfo.success)
                {
                    Debug.Log("The element pressed: " + pollingInfo.elementIdentifierName + " Related id: " + pollingInfo.elementIdentifierId);

                    int pressedIdentifierID = pollingInfo.elementIdentifierId;

                    var gamepadTemplate = currentActiveJoystick.GetTemplate<IGamepadTemplate>();
                    if (gamepadTemplate != null)
                    {

                    }
                }
            }

        }
    }


    private Vector2 ProcessStickInput(Vector2 _rawInput, float _innerDeadZone, float _outerDeadZone)
    {
        float magnitude = _rawInput.magnitude;

        // 1. 硬件噪声保护
        if (magnitude < 0.0001f)
            return Vector2.zero;

        // 2. 中心死区（圆形）
        if (magnitude <= _innerDeadZone)
            return Vector2.zero;

        // 3. 外死区
        if (_outerDeadZone <= _innerDeadZone)
            return _rawInput.normalized;

        // 4. 归一化幅度
        float normalizedMagnitude =
            (magnitude - _innerDeadZone) /
            (_outerDeadZone - _innerDeadZone);

        normalizedMagnitude = Mathf.Clamp01(normalizedMagnitude);

        // 5. 保留方向
        return _rawInput.normalized * normalizedMagnitude;
    }

    private void SetAllControllersRawInputDeadZonesToZero()
    {
        foreach (var joystick in player.controllers.Joysticks)
        {
            var calibrationMap = joystick.calibrationMap;
            for (int i = 0; i < calibrationMap.axisCount; i++)
            {
                var axis = calibrationMap.GetAxis(i);

                axis.deadZone = 0;
                axis.upperDeadZone = 0;
            }
        }
    }

    private void GetAxisNumberAccordingToActionName(string _actionName)
    {
        int actionID = -1;
        actionID = ReInput.mapping.GetActionId(_actionName);

        if (actionID == -1)
        {
            Debug.Log($"Couldn't find the action {_actionName}!");
            return;
        }

        var maps = player.controllers.maps.GetAllMaps();
        foreach (var map in maps)
        {
            foreach (var elementMap in map.AllMaps)
            {
                if (elementMap.actionId == actionID)
                {
                    Debug.Log($"{_actionName} comes from axis: {elementMap.elementIndex} on controller: {map.controller.name}");
                }
            }
        }
    }

    private void AddControllerVibration(float motorLow, float motorHigh, float duration)
    {
        foreach (Joystick joystick in player.controllers.Joysticks)
        {
            //if (!joystick.supportsVibration)
            //{
            //    Debug.Log($"Current joystick: {joystick.hardwareName} doesn't support vibration!");
            //    continue;
            //}

            Debug.Log($"Current joystick: {joystick.hardwareName}, vibration support: {joystick.supportsVibration}");

            joystick.SetVibration(motorLow, motorHigh);

            Invoke(nameof(StopControllerVibration), duration);
        }
    }

    private void StopControllerVibration()
    {
        foreach (Joystick joystick in player.controllers.Joysticks)
        {
            joystick.StopVibration();
        }
    }
}


