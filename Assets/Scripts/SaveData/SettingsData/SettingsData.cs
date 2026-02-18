using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public SerializableDictionary<string, float> settingsDictionary;

    public SettingsData()
    {
        settingsDictionary = new SerializableDictionary<string, float>();
    }
}
