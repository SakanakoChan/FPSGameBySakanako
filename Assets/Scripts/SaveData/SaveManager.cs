using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }


    [Header("Settings data info")]
    public string settingsDataFileName = "SettingsData";
    private string settingsDataFilePath;
    public SettingsData settingsData { get; private set; }
    private List<ISettingsDataAction> settingsDataActionList = new List<ISettingsDataAction>();
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
                settingsData = new SettingsData();
                Debug.Log("No setting data found, creating new setting data..");
            }
        }


        foreach (var settingsDataAction in settingsDataActionList)
        {
            settingsDataAction?.LoadData(settingsData);
        }
    }

    public void SaveSettings()
    {
        foreach (var settingsDataAction in settingsDataActionList)
        {
            settingsDataAction?.SaveData(settingsData);
        }

        saveDataHandler?.SaveData(settingsData);
    }


    public void RegisterSettingsDataAction(ISettingsDataAction _settingsDataAction)
    {
        if (settingsDataActionList.Contains(_settingsDataAction) == false)
        {
            settingsDataActionList.Add(_settingsDataAction);
        }
    }

    public void UnregisterSettingsDataAction(ISettingsDataAction _settingsDataAction)
    {
        if (settingsDataActionList.Contains(_settingsDataAction) == true)
        {
            settingsDataActionList.Remove(_settingsDataAction);
        }
    }

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

}
