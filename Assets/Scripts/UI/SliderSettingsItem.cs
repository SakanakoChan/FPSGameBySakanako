using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSettingsItem : SettingsItem
{
    [SerializeField] private SliderSettingsConfig config;
    [SerializeField] protected Image editModeHintImage;

    private Slider slider;
    private TMP_InputField inputField;

    private bool canChangeSliderValue = false;
    private float sliderValueChangeCooldownForUIHorizontalInput = 0.1f;
    private float lastSliderValueChangeTime = float.MinValue;

    protected override void Awake()
    {
        base.Awake();

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

        ShowEditModeHintImage(false);
    }

    private void OnEnable()
    {
        InitializeSliderValue();
    }

    private void Update()
    {
        if (CheckIfCanChangeSliderValueForUIHorizontalInput())
        {
            var uiHorizontalInput = InputManager.instance.UIHorizontal;
            if (Mathf.Abs(uiHorizontalInput) >= 0.5f)
            {
                ModifySliderValue(Mathf.Sign(uiHorizontalInput) * config.valueChangeStep);
            }
        }
    }

    private void InitializeSliderValue()
    {
        if (slider != null)
        {
            slider.wholeNumbers = config.valueIsWholeNumbers;

            slider.minValue = config.minValue;
            slider.maxValue = config.maxValue;

            LoadData(SaveManager.instance.settingsData);

            //slider.value = config.defaultValue;
            //SyncInputFieldValue(slider.value);
        }
    }

    public override void Confirm()
    {
        if (isInEditMode)
        {
            Cancel();
            return;
        }

        isInEditMode = true;
        canChangeSliderValue = true;

        LockNavigation();
        ShowEditModeHintImage(true);
    }

    public override void Cancel()
    {
        base.Cancel();

        isInEditMode = false;
        canChangeSliderValue = false;

        UnlockNavigation();
        ShowEditModeHintImage(false);
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

        if (float.TryParse(_inputFieldText, out float inputValue))
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

    private bool CheckIfCanChangeSliderValueForUIHorizontalInput()
    {
        if (canChangeSliderValue == false || Time.unscaledTime - lastSliderValueChangeTime < sliderValueChangeCooldownForUIHorizontalInput)
        {
            return false;
        }

        return true;
    }

    private void ModifySliderValue(float _deltaValue)
    {
        slider.value += _deltaValue;

        lastSliderValueChangeTime = Time.unscaledTime;
    }

    private void ShowEditModeHintImage(bool _value)
    {
        if (editModeHintImage != null)
        {
            editModeHintImage.gameObject.SetActive(_value);
        }
    }

    public override void LoadData(SettingsData _data)
    {
        if (_data.settingsDictionary.TryGetValue(config.key, out var value))
        {
            slider.value = value;
            SyncInputFieldValue(slider.value);
        }
        else
        {
            slider.value = config.defaultValue;
            SyncInputFieldValue(slider.value);
        }
    }

    public override void SaveData(SettingsData _data)
    {
        if (_data.settingsDictionary.ContainsKey(config.key))
        {
            _data.settingsDictionary[config.key] = slider.value;
        }
        else
        {
            _data.settingsDictionary.Add(config.key, slider.value);
        }
    }
}
