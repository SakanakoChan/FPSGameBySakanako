using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsConfigDatabase", menuName = "Settings/SettingsConfigDatabase", order = 1)]
public class SettingsConfigDatabase : ScriptableObject
{
    public List<SettingsConfig> settingsConfigList;


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
