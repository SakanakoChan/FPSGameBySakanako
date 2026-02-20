using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static float controllerLookSensitivity => SaveManager.instance.GetSettingsFloat("ControllerLookSensitivity");
    public static bool invertYAxis => SaveManager.instance.GetSettingsBool("InvertYAxis");
}
