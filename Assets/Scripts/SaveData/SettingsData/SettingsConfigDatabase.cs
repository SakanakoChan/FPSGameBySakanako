using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsConfigDatabase", menuName = "Settings/SettingsConfigDatabase", order = 1)]
public class SettingsConfigDatabase : ScriptableObject
{
    public List<SettingsConfig> settingsConfigList;

    public bool TryGetDefaultValue<T>(string _key, out T _defaultValue)
    {
        foreach (var config in settingsConfigList)
        {
            if (config.key == _key)
            {
                if (config.defaultValue is T value)
                {
                    _defaultValue = value;

                    return true;
                }

                Debug.LogError("Type mismatch for key: " + _key + ". Expected type: " + typeof(T) + ", but got: " + config.defaultValue.GetType());
                break;
            }
        }

        Debug.LogWarning("Settings key not found in config database: " + _key);

        _defaultValue = default(T);
        return false;

    }


#if UNITY_EDITOR
    [ContextMenu("Auto Collect All Settings Configs")]
    public void AutoCollect()
    {
        settingsConfigList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:SettingsConfig");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var config = AssetDatabase.LoadAssetAtPath<SettingsConfig>(path);

            if (config != null)
            {
                settingsConfigList.Add(config);
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Collected {settingsConfigList.Count} SettingsConfig.");
    }
#endif

}
