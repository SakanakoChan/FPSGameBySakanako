using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownSettingsItem : SettingsItem
{
    [SerializeField] private DropdownSettingsConfig config;


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
            LoadData(SaveManager.instance.settingsData);
    }

    private void Start()
    {
        InitializeDropdownOptions();

        LoadData(SaveManager.instance.settingsData);
    }

    private void Update()
    {
        //if (previousExpanded != dropdown.IsExpanded)
        //{
        //    if (dropdown.IsExpanded == true)
        //    {
        //        forceHighlight = true;
        //    }
        //    else
        //    {
        //        forceHighlight = false;

        //        if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        //        {
        //            //selectable.GetComponent<DeviceAwareSelectable>()?.ForceEnterNormalState();
        //            ForceHighlightSelectable(false);
        //        }
        //    }

        //    previousExpanded = dropdown.IsExpanded;
        //}

        //if (forceHighlight)
        //{
        //    ForceHighlightSelectable(true);
        //}
    }

    private void InitializeDropdownOptions()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(config.options);

        dropdownOptionsHaveBeenInitialized = true;
    }

    private void ForceHighlightSelectable(bool _value)
    {
        var eventData = new PointerEventData(EventSystem.current);

        if (_value == true)
        {
            selectable.OnPointerEnter(eventData);
        }
        else
        {
            selectable.OnPointerExit(eventData);
        }
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


    public override void LoadData(SettingsData _data)
    {
        if (_data.settingsDictionary.TryGetValue(config.key, out var value))
        {
            dropdown.value = (int)config.DeserializeString(value);
        }
        else
        {
            dropdown.value = config.defaultValue;
        }
    }

    public override void SaveData(SettingsData _data)
    {
        if (_data.settingsDictionary.ContainsKey(config.key))
        {
            _data.settingsDictionary[config.key] = config.SerializeValue(dropdown.value);
        }
        else
        {
            _data.settingsDictionary.Add(config.key, config.SerializeValue(dropdown.value));
        }
    }
}
