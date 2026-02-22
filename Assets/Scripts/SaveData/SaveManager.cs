using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }


    [Header("Settings data info")]
    public string settingsDataFileName = "SettingsData";
    [SerializeField] private SettingsConfigDatabase settingsConfigDatabase;
    private string settingsDataFilePath;
    public SettingsData settingsData { get; private set; }
    //private List<ISettingsDataAction> settingsDataActionList = new List<ISettingsDataAction>();
    private DataFileHandler saveDataHandler;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (settingsConfigDatabase == null)
        {
            Debug.LogError("Warning: settings config database is actually missing!");
        }

        settingsDataFilePath = Application.persistentDataPath;
        saveDataHandler = new DataFileHandler(settingsDataFilePath, settingsDataFileName);
        LoadSettings();
    }


    public void LoadSettings()
    {
        if (settingsData == null)
        {
            settingsData = saveDataHandler.LoadData<SettingsData>();
            Debug.Log("Trying to load setting data from file..");

            if (settingsData == null)
            {
#if UNITY_EDITOR
                AutoCollectAllSettingsConfigsForDatabase();
#endif
                settingsData = new SettingsData();
                WriteDefaultSettings(settingsData);
                SaveSettings();

                Debug.Log("No settings data found, creating new settings data..");
            }
        }

        //Debug.Log("SettingsDataAction Count: " + settingsDataActionList.Count);
        //foreach (var settingsDataAction in settingsDataActionList)
        //{
        //    settingsDataAction?.LoadData(settingsData);
        //}
    }

    public void SaveSettings()
    {
        //foreach (var settingsDataAction in settingsDataActionList)
        //{
        //    settingsDataAction?.SaveData(settingsData);
        //}

        saveDataHandler?.SaveData(settingsData);
    }



    private void WriteDefaultSettings(SettingsData _settingsData)
    {
        foreach (var settingsConfig in settingsConfigDatabase.settingsConfigList)
        {
            var defaultValue = settingsConfig.GetDefaultValue();
            var serializedString = settingsConfig.SerializeValue(defaultValue);

            _settingsData.settingsDictionary[settingsConfig.key] = serializedString;
        }
    }

    //public void RegisterSettingsDataAction(ISettingsDataAction _settingsDataAction)
    //{
    //    if (settingsDataActionList.Contains(_settingsDataAction) == false)
    //    {
    //        settingsDataActionList.Add(_settingsDataAction);
    //    }
    //}

    //public void UnregisterSettingsDataAction(ISettingsDataAction _settingsDataAction)
    //{
    //    if (settingsDataActionList.Contains(_settingsDataAction) == true)
    //    {
    //        settingsDataActionList.Remove(_settingsDataAction);
    //    }
    //}

    public int GetSettingsInt(string _key)
    {
        if (settingsData.settingsDictionary.TryGetValue(_key, out var value))
        {
            return int.Parse(value);
        }

        Debug.LogWarning($"Settings key not found in save data: {_key}, now trying to get default value");

        if (settingsConfigDatabase.TryGetDefaultValue(_key, out int defaultValue))
        {
            return defaultValue;
        }

        Debug.Log("Default value for key: " + _key + " also not found in config database, returning 0");
        return 0;
    }

    public float GetSettingsFloat(string _key)
    {
        if (settingsData.settingsDictionary.TryGetValue(_key, out var value))
        {
            return float.Parse(value);
        }

        Debug.LogWarning($"Settings key not found in save data: {_key}, now trying to get default value");
        
        if(settingsConfigDatabase.TryGetDefaultValue(_key, out float defaultValue))
        {
            return defaultValue;
        }

        Debug.Log("Default value for key: " + _key + " also not found in config database, returning 0");
        return 0f;
    }


    public bool GetSettingsBool(string _key)
    {
        if (settingsData.settingsDictionary.TryGetValue(_key, out var value))
        {
            return bool.Parse(value);
        }

        Debug.LogWarning($"Settings key not found in save data: {_key}, now trying to get default value");

        if (settingsConfigDatabase.TryGetDefaultValue(_key, out bool defaultValue))
        {
            return defaultValue;
        }

        Debug.Log("Default value for key: " + _key + " also not found in config database, returning false");
        return false;
    }

    public T GetSettingsEnum<T>(string _key) where T : struct, Enum
    {
        if (settingsData.settingsDictionary.TryGetValue(_key, out var value))
        {
            if (Enum.TryParse<T>(value, out var result))
            {
                return result;
            }
        }

        Debug.LogWarning($"Settings key not found in save data: {_key}, now trying to get default value");

        if (settingsConfigDatabase.TryGetDefaultValue(_key, out T defaultValue))
        {
            return defaultValue;
        }

        Debug.Log("Default value for key: " + _key + " also not found in config database, returning false");
        return default;
    }

    public void SetSettings(string _key, string _serializedValue)
    {
        if (settingsData.settingsDictionary.ContainsKey(_key))
        {
            settingsData.settingsDictionary[_key] = _serializedValue;
        }
        else
        {
            Debug.Log($"Didn't find the key in settingdata: {_key}. Now adding new setting item to settings data..");
            settingsData.settingsDictionary[_key] = _serializedValue;
        }
    }


#if UNITY_EDITOR
    [ContextMenu("Show save file in file explorer")]
    private void ShowSaveFileInFileExplorer()
    {
        string path = Path.Combine(Application.persistentDataPath);

        //if (!File.Exists(path))
        //{
        //    Debug.LogWarning($"Save file does not exist at path:\n{path}");
        //    return;
        //}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // Windows
        System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace("/", "\\"));
#endif
    }

    [ContextMenu("Auto collect all settings configs for database")]
    private void AutoCollectAllSettingsConfigsForDatabase()
    {
        if (settingsConfigDatabase != null)
        {
            settingsConfigDatabase.AutoCollect();
        }
    }
#endif
}
