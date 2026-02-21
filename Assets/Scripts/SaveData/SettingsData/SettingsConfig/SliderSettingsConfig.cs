using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SliderSettingsConfig", menuName = "Settings/SliderSettingsConfig")]
public class SliderSettingsConfig : SettingsConfig
{
    [Header("Slider Settings")]
    public float minValue;
    public float maxValue;
    public float defaultValue;

    [Space]
    public bool valueIsWholeNumbers;
    public float valueChangeStep;


    public override object GetDefaultValue()
    {
        return defaultValue;
    }

    public override string SerializeValue(object _valueToSerialize)
    {
        return ((float)_valueToSerialize).ToString();
    }

    public override object DeserializeString(string _stringToDeserialize)
    {
        if (float.TryParse(_stringToDeserialize, out var floatValue))
        {
            return floatValue;
        }

        Debug.Log("Failed to deserialize setting item! Now getting its default value..");
        return defaultValue;
    }

}
