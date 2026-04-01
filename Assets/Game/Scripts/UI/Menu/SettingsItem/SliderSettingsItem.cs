using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSettingsItem : SettingsItem
{
    [SerializeField] private SliderSettingsConfig config;
    [SerializeField] protected Image editModeHintImage;

    protected Slider slider;
    private TMP_InputField inputField;

    private float sliderValueChangeCooldownForUIHorizontalInput = 0.1f;
    private float lastSliderValueChangeTime = float.MinValue;

    protected override void Awake()
    {
        base.Awake();

        if (config == null)
        {
            Debug.LogError("Didn't assign config for settings item: " + gameObject.name + "!");
        }

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

            LoadData();

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

        SetEditMode(true);
    }

    public override void Cancel()
    {
        if (isInEditMode == false)
        {
            return;
        }

        SetEditMode(false);
    }

    protected override void OnEditModeChanged(bool _editMode)
    {
        if (_editMode == true)
        {
            LockNavigation();
            ShowEditModeHintImage(true);
        }
        else
        {
            UnlockNavigation();
            ShowEditModeHintImage(false);
        }
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
        if (isInEditMode == false || Time.unscaledTime - lastSliderValueChangeTime < sliderValueChangeCooldownForUIHorizontalInput)
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

    public override void LoadData()
    {
        slider.value = SaveManager.instance.GetSettingsFloat(config.key);
        SyncInputFieldValue(slider.value);

        //Deprecated
        //if (_data.settingsDictionary.TryGetValue(config.key, out var value))
        //{
        //    slider.value = (float)config.DeserializeString(value)/*float.Parse(value)*/;
        //    SyncInputFieldValue(slider.value);
        //}
        //else
        //{
        //    slider.value = config.defaultValue;
        //    SyncInputFieldValue(slider.value);
        //}
    }

    public override void SaveData()
    {
        if (slider == null)
        {
            return;
        }

        SaveManager.instance.SetSettings(config.key, config.SerializeValue(slider.value));

        //Deprecated
        //if (_data.settingsDictionary.ContainsKey(config.key))
        //{
        //    _data.settingsDictionary[config.key] = config.SerializeValue(slider.value)/*slider.value.ToString()*/;
        //}
        //else
        //{
        //    _data.settingsDictionary.Add(config.key, config.SerializeValue(slider.value)/*slider.value.ToString()*/);
        //}
    }
}
