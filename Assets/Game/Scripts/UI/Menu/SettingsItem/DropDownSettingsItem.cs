using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownSettingsItem : SettingsItem
{
    [SerializeField] private DropdownSettingsConfig config;

    [Header("For UGUI Dropdown bug fix")]
    [SerializeField] private Image highlightImageWhenDropdownExpanded;


    private TMP_Dropdown dropdown;
    private SettingsPanel settingsPanel;

    private bool dropdownOptionsHaveBeenInitialized = false;
    private bool forceHighlight = false;
    private bool previousExpanded = false;

    protected override void Awake()
    {
        base.Awake();

        if (config == null)
        {
            Debug.LogError("Didn't assign config for settings item: " + gameObject.name + "!");
        }

        dropdown = GetComponentInChildren<TMP_Dropdown>();
        settingsPanel = GetComponentInParent<SettingsPanel>();
    }

    private void OnEnable()
    {
        if (dropdownOptionsHaveBeenInitialized)
            LoadData();
    }

    private void Start()
    {
        InitializeDropdownOptions();

        LoadData();
    }

    private void Update()
    {
        if (previousExpanded != dropdown.IsExpanded)
        {
            if (dropdown.IsExpanded == true)
            {
                //forceHighlight = true;
                highlightImageWhenDropdownExpanded.gameObject.SetActive(true);
            }
            else
            {
                //forceHighlight = false;
                highlightImageWhenDropdownExpanded.gameObject.SetActive(false);
            }

            previousExpanded = dropdown.IsExpanded;
        }
    }

    private void InitializeDropdownOptions()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(config.options);

        dropdownOptionsHaveBeenInitialized = true;
    }


    public override void Confirm()
    {
        if (dropdown == null)
        {
            return;
        }

        if (isInEditMode)
        {
            Cancel();
            return;
        }

        SetEditMode(true);

        ExecuteEvents.Execute(dropdown.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        //prevent default (should be the previously selected one) selected dropdown option
        //from always being the first one in some strange case
        StartCoroutine(WaitOneFrameAndSelectPreviousOption_Coroutine());
    }

    public override void Cancel()
    {
        if (dropdown == null)
        {
            return;
        }

        settingsPanel?.BlockScrollForOneFrame();
        dropdown.Hide();
        EventSystem.current.SetSelectedGameObject(gameObject);
        SetEditMode(false);
    }

    private IEnumerator WaitOneFrameAndSelectPreviousOption_Coroutine()
    {
        yield return null;

        var options = dropdown.GetComponentsInChildren<Toggle>();
        if (dropdown.value < options.Length)
        {
            EventSystem.current.SetSelectedGameObject(options[dropdown.value].gameObject);
        }
    }


    public override void LoadData()
    {
        dropdown.value = SaveManager.instance.GetSettingsInt(config.key);

        //Deprecated
        //if (_data.settingsDictionary.TryGetValue(config.key, out var value))
        //{
        //    dropdown.value = (int)config.DeserializeString(value);
        //}
        //else
        //{
        //    dropdown.value = config.defaultValue;
        //}
    }

    public override void SaveData()
    {
        if (dropdown == null || !dropdownOptionsHaveBeenInitialized)
        {
            return;
        }

        SaveManager.instance.SetSettings(config.key, config.SerializeValue(dropdown.value));

        //Deprecated
        //if (_data.settingsDictionary.ContainsKey(config.key))
        //{
        //    _data.settingsDictionary[config.key] = config.SerializeValue(dropdown.value);
        //}
        //else
        //{
        //    _data.settingsDictionary.Add(config.key, config.SerializeValue(dropdown.value));
        //}
    }
}
