using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    #region Controller Settings
    public static bool controllerAutoSprint => SaveManager.instance.GetSettingsBool("ControllerAutoSprint");
    public static float controllerLookSensitivity => SaveManager.instance.GetSettingsFloat("ControllerLookSensitivity");
    public static float controllerVerticalSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("ControllerVerticalSensitivityMultiplier");
    public static bool invertYAxis_Controller => SaveManager.instance.GetSettingsBool("ControllerInvertYAxis");
    public static float innerDeadzone => SaveManager.instance.GetSettingsFloat("InnerDeadzone");
    public static float outerDeadzone => SaveManager.instance.GetSettingsFloat("OuterDeadzone");
    public static ResponsiveCurve responsiveCurve => SaveManager.instance.GetSettingsEnum<ResponsiveCurve>("ResponsiveCurve");
    public static bool turnAcceleration => SaveManager.instance.GetSettingsBool("TurnAcceleration");
    public static float horizontalTurnAccelerationStartDelay => SaveManager.instance.GetSettingsFloat("HorizontalTurnAccelerationStartDelay");
    public static float horizontalTurnAccelerationRampUpTime => SaveManager.instance.GetSettingsFloat("HorizontalTurnAccelerationRampUpTime");
    public static float horizontalTurnAccelerationSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("HorizontalTurnAccelerationSensitivityMultiplier");
    public static float verticalTurnAccelerationStartDelay => SaveManager.instance.GetSettingsFloat("VerticalTurnAccelerationStartDelay");
    public static float verticalTurnAccelerationRampUpTime => SaveManager.instance.GetSettingsFloat("VerticalTurnAccelerationRampUpTime");
    public static float verticalTurnAccelerationSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("VerticalTurnAccelerationSensitivityMultiplier");
    public static ADSSensitivityTransition controllerADSSensitivityTransition => SaveManager.instance.GetSettingsEnum<ADSSensitivityTransition>("ControllerADSSensitivityTransition");
    public static float controllerADSSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("ControllerADSSensitivityMultiplier");
    public static bool controllerAimAssistEnabled => SaveManager.instance.GetSettingsBool("ControllerAimAssist");
    #endregion

    #region Mouse Settings
    public static float mouseLookSensitivity => SaveManager.instance.GetSettingsFloat("MouseLookSensitivity");
    public static bool invertYAxis_Mouse => SaveManager.instance.GetSettingsBool("MouseInvertYAxis");
    public static float mouseADSSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("MouseADSSensitivityMultiplier");
    public static ADSSensitivityTransition mouseADSSensitivityTransition => SaveManager.instance.GetSettingsEnum<ADSSensitivityTransition>("MouseADSSensitivityTransition");
    #endregion

    #region Audio Settings
    public static float masterVolume => SaveManager.instance.GetSettingsFloat("MasterVolume");
    public static float sfxVolume => SaveManager.instance.GetSettingsFloat("SFXVolume");
    #endregion
}
