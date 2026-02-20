using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettingsItem : SettingsItem
{
    [SerializeField] private ToggleSettingsConfig config;

    private Toggle toggle;
    [SerializeField] private TextMeshProUGUI toggleModeHintText;

    protected override void Awake()
    {
        base.Awake();

        toggle = GetComponentInChildren<Toggle>();

        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(SyncToggleModeHintText);
    }

    private void OnEnable()
    {
        LoadData(SaveManager.instance.settingsData);
    }



    private void SyncToggleModeHintText(bool _isOn)
    {
        toggleModeHintText.text = _isOn == true ? "Enabled" : "Disabled";
    }

    public override void Confirm()
    {
        toggle.isOn = !toggle.isOn;
    }

    public override void LoadData(SettingsData _data)
    {
        if (_data.settingsDictionary.TryGetValue(config.key, out var value))
        {
            toggle.isOn = (bool)config.DeserializeString(value);
            SyncToggleModeHintText(toggle.isOn);
        }
        else
        {
            toggle.isOn = config.defaultValue;
            SyncToggleModeHintText(toggle.isOn);
        }
    }

    public override void SaveData(SettingsData _data)
    {
        if (_data.settingsDictionary.ContainsKey(config.key))
        {
            _data.settingsDictionary[config.key] = config.SerializeValue(toggle.isOn);
        }
        else
        {
            _data.settingsDictionary.Add(config.key, config.SerializeValue(toggle.isOn));
        }
    }
}
