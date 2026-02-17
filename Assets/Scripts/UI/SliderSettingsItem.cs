using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SliderSettingsItem : SettingsItem
{
    [SerializeField] private SliderSettingsConfig config;

    private Slider slider;
    private TMP_InputField inputField;

    private bool canChangeSliderValue = false;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        inputField = GetComponentInChildren<TMP_InputField>();

        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((value) =>
            {
                SyncInputFieldValue(value);
            });
        }

        if (inputField != null)
        {
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener((text) =>
            {
                SyncSliderValue(text);
            });
        }
    }

    private void OnEnable()
    {
        InitializeSliderValue();
    }

    private void InitializeSliderValue()
    {
        if (slider != null)
        {
            slider.wholeNumbers = config.valueIsWholeNumbers;

            slider.minValue = config.minValue;
            slider.maxValue = config.maxValue;

            slider.value = config.defaultValue;
            SyncInputFieldValue(slider.value);
        }
    }

    public override void Confirm()
    {
        canChangeSliderValue = true;
    }

    public override void Cancel()
    {
        base.Cancel();

        canChangeSliderValue = false;
    }

    private void SyncInputFieldValue(float _sliderValue)
    {
        if (inputField == null)
            return;

        if (slider != null && slider.wholeNumbers)
        {
            inputField.text = Mathf.RoundToInt(_sliderValue).ToString();
        }
        else
        {
            inputField.text = _sliderValue.ToString("F2");
        }
    }

    private void SyncSliderValue(string _inputFieldText)
    {
        if (slider == null)
            return;

        if(float.TryParse(_inputFieldText, out float inputValue))
        {
            inputValue = Mathf.Clamp(inputValue, slider.minValue, slider.maxValue);

            if (slider.wholeNumbers)
            {
                inputValue = Mathf.Round(inputValue);
            }

            slider.value = inputValue;
        }

        SyncInputFieldValue(slider.value);
    }
}
