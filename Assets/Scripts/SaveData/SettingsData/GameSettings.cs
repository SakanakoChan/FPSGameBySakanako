using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static float controllerLookSensitivity => SaveManager.instance.GetSettingsFloat("ControllerLookSensitivity");
    public static float controllerVerticalSensitivityMultiplier => SaveManager.instance.GetSettingsFloat("ControllerVerticalSensitivityMultiplier");
    public static bool invertYAxis => SaveManager.instance.GetSettingsBool("InvertYAxis");
    public static float innerDeadzone => SaveManager.instance.GetSettingsFloat("InnerDeadzone");
    public static float outerDeadzone => SaveManager.instance.GetSettingsFloat("OuterDeadzone");
    public static ResponsiveCurve responsiveCurve => SaveManager.instance.GetSettingsEnum<ResponsiveCurve>("ResponsiveCurve");
}
