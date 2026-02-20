using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingsConfig : ScriptableObject
{
    [Header("Common")]
    public string key;
    public string displayName;
    //public float defaultValue;

    public abstract object GetDefaultValue();
    public abstract string SerializeValue(object _valueToSerialize);
    public abstract object DeserializeString(string _stringToDeserialize);
}
