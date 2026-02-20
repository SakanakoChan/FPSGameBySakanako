using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToggleSettingsConfig", menuName = "Settings/ToggleSettingsConfig")]
public class ToggleSettingsConfig : SettingsConfig
{
    [Header("Toggle Settings")]
    public bool defaultValue = false;


    public override object GetDefaultValue()
    {
        return defaultValue;
    }

    public override string SerializeValue(object _valueToSerialize)
    {
        return _valueToSerialize.ToString();
    }

    public override object DeserializeString(string _stringToDeserialize)
    {
        if (bool.TryParse(_stringToDeserialize, out var value))
            return value;

        Debug.Log("Failed to Deserialize! Now getting its default value..");
        return defaultValue;
        
    }
}
