using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingsConfig : ScriptableObject
{
    [Header("Common")]
    public string key;
    public string displayName;
}
