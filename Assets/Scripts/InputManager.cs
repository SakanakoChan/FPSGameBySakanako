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

    [Space]
    [SerializeField] private Vector2 moveInputRaw;

    [Space]
    [SerializeField] private Vector2 lookInputRaw;

    [Space]
    public Vector2 lookInputReal;

    [Space]
    public Vector2 mouseInput;


    [Header("Dead zone info")]
    [Tooltip(
        "Inner dead zone. Stick input below this value is ignored.\n" +
        "Used to prevent stick drift."
    )]
    [SerializeField] private float innerDeadZone = 0f;

    [Tooltip(
        "Outer dead zone. Defines how far the stick must be pushed to reach maximum input.\n" +
        "A value of 1 means no outer dead zone."
    )]
    [SerializeField] private float outerDeadZone = 1f;


    [Header("Input Device Switch info")]
    public float deadZoneToTriggerControllerInput = 0.2f;
    public float mouseDeltaThresholdToTriggerMouseInput = 0.5f;
    public InputDevice currentInputDevice { get; private set; } = InputDevice.MouseAndKeyboard;
    public ControllerLayout currentControllerLayout { get; private set; } = ControllerLayout.XBox;
    private Joystick currentActiveJoystick = null;



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


    private void Start()
    {
        player = ReInput.players.GetPlayer(playerIDForRewired);

        SetAllControllersRawInputDeadZonesToZero();
    }

    private void Update()
    {
        moveInputRaw = new Vector2(player.GetAxisRaw("Move Horizontal"), player.GetAxisRaw("Move Vertical"));
        lookInputRaw = new Vector2(player.GetAxisRaw("Look Horizontal"), player.GetAxisRaw("Look Vertical"));

        lookInputReal = ProcessStickInput(lookInputRaw, innerDeadZone, outerDeadZone);

        mouseInput = new Vector2(player.GetAxisRaw("MouseX"), player.GetAxisRaw("MouseY"));


        if (player.GetButtonDown("Jump"))
        {
            Debug.Log("Jump button pressed");

            AddControllerVibration(0.5f, 0.5f, 0.2f);
        }

        if (player.GetButtonDown("Crouch"))
        {
            Debug.Log("Crouch button pressed");
        }

        if (player.GetButtonDown("Reload"))
        {
            Debug.Log("Reload button pressed");
        }

        if (player.GetButtonDown("Switch Weapon"))
        {
            Debug.Log("Switch Weapon button pressed");
        }

        InputDeviceDetection();

        DetectCurrentControllerLayout();

        Debug.Log("Current input device: " + currentInputDevice);
        Debug.Log("Current controller layout: " + currentControllerLayout);

        //foreach (var controller in player.controllers.Controllers)
        //{
        //    Debug.Log($"Controller: {controller.name}, Type: {controller.type}");

        //    foreach (var map in player.controllers.maps.GetMaps(controller))
        //    {
        //        if (map.enabled)
        //        {
        //            Debug.Log($"map category id: {map.categoryId}");
        //        }
        //    }
        //}
    }

    private void InputDeviceDetection()
    {
        bool hasKeyboardInput = ReInput.controllers.Keyboard.GetAnyButtonDown();
        if (mouseInput.sqrMagnitude > mouseDeltaThresholdToTriggerMouseInput || hasKeyboardInput)
        {
            currentInputDevice = InputDevice.MouseAndKeyboard;
            return;
        }

        bool hasControllerButtonInput = false;
        foreach (var joystick in ReInput.controllers.Joysticks)
        {
            if (joystick.GetAnyButtonDown())
            {
                hasControllerButtonInput = true;
                break;
            }
        }

        if (lookInputRaw.sqrMagnitude > deadZoneToTriggerControllerInput || hasControllerButtonInput)
        {
            currentInputDevice = InputDevice.Controller;
        }
    }

    private void DetectCurrentControllerLayout()
    {
        Controller lastInputDevice = player.controllers.GetLastActiveController();
        if (lastInputDevice != null && lastInputDevice.type == ControllerType.Joystick)
        {
            currentActiveJoystick = lastInputDevice as Joystick;
        }


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


