using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    [Header("Rewired input info")]
    [SerializeField] private int playerIDForRewired = 0;
    private Player player;

    [Space]
    [SerializeField] private Vector2 moveInputRaw;

    [Space]
    [SerializeField] private Vector2 lookInputRaw;

    [Space]
    [SerializeField] private Vector2 lookInputReal;


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


