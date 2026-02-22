using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour, IUIAction
{
    [Header("Top bar info")]
    [SerializeField] private Toggle gameSettingsToggle;
    [SerializeField] private Toggle mouseSettingsToggle;
    [SerializeField] private Toggle keyboardSettingsToggle;
    [SerializeField] private Toggle controllerSettingsToggle;
    private ToggleGroup toggleGroup;


    [Header("Settings panels")]
    [SerializeField] private SettingsPanel gameSettingsPanel;
    [SerializeField] private SettingsPanel mouseSettingsPanel;
    [SerializeField] private SettingsPanel keyboardSettingsPanel;
    [SerializeField] private SettingsPanel controllerSettingsPanel;
    private SettingsPanel currentSettingsPanel;

    [Space]
    [SerializeField] private float switchPageCooldown = 0.1f;
    private float lastSwitchPageTime = float.MinValue;

    private SettingsItem[] settingsItemList;


    private void Awake()
    {
        settingsItemList = GetComponentsInChildren<SettingsItem>();
    }

    private void Start()
    {
        toggleGroup = GetComponentInChildren<ToggleGroup>();

        gameSettingsToggle.onValueChanged.RemoveAllListeners();
        mouseSettingsToggle.onValueChanged.RemoveAllListeners();
        keyboardSettingsToggle.onValueChanged.RemoveAllListeners();
        controllerSettingsToggle.onValueChanged.RemoveAllListeners();

        gameSettingsToggle.onValueChanged.AddListener(ShowGameSettingsPanel);
        mouseSettingsToggle.onValueChanged.AddListener(ShowMouseSettingsPanel);
        keyboardSettingsToggle.onValueChanged.AddListener(ShowKeyboardSettingsPanel);
        controllerSettingsToggle.onValueChanged.AddListener(ShowControllerSettingsPanel);

        ShowGameSettingsPanel(true);
    }

    private void OnDisable()
    {
        CommitSettingsItemValueChangesToSettingsData();
        SaveManager.instance?.SaveSettings();
    }



    private void ShowGameSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        gameSettingsPanel?.gameObject.SetActive(_value);
        currentSettingsPanel = gameSettingsPanel;
    }

    private void ShowMouseSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        mouseSettingsPanel?.gameObject.SetActive(_value);
        currentSettingsPanel = mouseSettingsPanel;
    }

    private void ShowKeyboardSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        keyboardSettingsPanel?.gameObject.SetActive(_value);
        currentSettingsPanel = keyboardSettingsPanel;
    }

    private void ShowControllerSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        controllerSettingsPanel?.gameObject.SetActive(_value);
        currentSettingsPanel = controllerSettingsPanel;
    }

    private void HideAllPanels()
    {
        gameSettingsPanel?.gameObject.SetActive(false);
        mouseSettingsPanel?.gameObject.SetActive(false);
        keyboardSettingsPanel?.gameObject.SetActive(false);
        controllerSettingsPanel?.gameObject.SetActive(false);
    }

    private void SwitchPage(bool _switchToRightPage)
    {
        if (CheckIfCanSwitchPage() == false)
        {
            return;
        }

        var activeToggle = toggleGroup.GetFirstActiveToggle();
        if (activeToggle == null)
            return;

        var nextSelectable = _switchToRightPage == true ? activeToggle.navigation.selectOnRight : activeToggle.navigation.selectOnLeft;

        if (nextSelectable == null)
            return;

        var nextToggle = nextSelectable.GetComponent<Toggle>();
        if (nextToggle == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        nextToggle.isOn = true;
        lastSwitchPageTime = Time.unscaledTime;
    }

    private bool CheckIfCanSwitchPage()
    {
        if (Time.unscaledTime - lastSwitchPageTime < switchPageCooldown)
        {
            return false;
        }

        var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        var settingsItem = currentSelectedGameObject?.GetComponent<SettingsItem>();

        if (settingsItem != null && settingsItem.isInEditMode == true)
        {
            return false;
        }

        return true;
    }


    public void UICancel()
    {
        var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject == null)
        {
            UIManager.instance?.SwitchState(UIManager.MenuState.PauseMenu);
            return;
        }


        var settingsItem = currentSelectedGameObject.GetComponentInParent<SettingsItem>();

        if (settingsItem != null && settingsItem.isInEditMode == true)
        {
            settingsItem.Cancel();
        }
        else
        {
            UIManager.instance?.SwitchState(UIManager.MenuState.PauseMenu);
        }
    }

    public void UIConfirm()
    {
        var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject == null)
            return;

        var settingsItem = currentSelectedGameObject.GetComponentInParent<SettingsItem>();
        settingsItem?.Confirm();
    }

    public void UISwitchPageRight()
    {
        SwitchPage(true);
    }

    public void UISwitchPageLeft()
    {
        SwitchPage(false);
    }

    public void ClearSelectedUIItem()
    {
        var selectedGameObject = EventSystem.current.currentSelectedGameObject;
        selectedGameObject?.GetComponentInParent<SettingsItem>()?.Cancel();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SelectFirstUIItem()
    {
        currentSettingsPanel?.SelectFirstSettingsItem();
    }

    private void CommitSettingsItemValueChangesToSettingsData()
    {
        foreach (var settingsItem in settingsItemList)
        {
            settingsItem?.SaveData();
        }
    }
}
