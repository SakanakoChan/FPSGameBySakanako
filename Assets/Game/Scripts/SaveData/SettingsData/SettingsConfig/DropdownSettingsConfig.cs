using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DropdownSettingsConfig", menuName = "Settings/DropdownSettingsConfig")]
public class DropdownSettingsConfig : SettingsConfig
{
    [Header("Drop down info")]
    public int defaultValue = 0;
    public List<string> options = new List<string>();

#if UNITY_EDITOR
    [Header("Unity Editor Tools")]
    [SerializeField] private string enumTypeName = "ResponsiveCurve";
#endif


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
        if (int.TryParse(_stringToDeserialize, out var value))
        {
            return value;
        }

        Debug.Log("Failed to Deserialize! Now getting its default value..");
        return defaultValue;
    }

#if UNITY_EDITOR
    [ContextMenu("GenerateOptionsFromEnum")] 
    private void GenerateOptionsFromEnum()
    {
        var type = Type.GetType(enumTypeName);

        if (type == null || type.IsEnum == false)
        {
            Debug.Log("Invalid type name!");
            return;
        }

        options = new List<string>(Enum.GetNames(type));

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
