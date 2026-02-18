using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SliderSettingsConfig", menuName = "Settings/SliderSettingsConfig")]
public class SliderSettingsConfig : SettingsConfig
{
    [Header("Slider Settings")]
    public float minValue;
    public float maxValue;

    [Space]
    public bool valueIsWholeNumbers;
    public float valueChangeStep;
}
