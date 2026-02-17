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
    [SerializeField] private GameObject gameSettingsPanel;
    [SerializeField] private GameObject mouseSettingsPanel;
    [SerializeField] private GameObject keyboardSettingsPanel;
    [SerializeField] private GameObject controllerSettingsPanel;

    [Space]
    [SerializeField] private float switchPageCooldown = 0.1f;
    private float lastSwitchPageTime = float.MinValue;


    private void Start()
    {
        toggleGroup = GetComponentInChildren<ToggleGroup>();

        gameSettingsToggle.onValueChanged.RemoveAllListeners();
        mouseSettingsToggle.onValueChanged.RemoveAllListeners();
        keyboardSettingsToggle.onValueChanged.RemoveAllListeners();
        controllerSettingsToggle.onValueChanged.RemoveAllListeners();

        gameSettingsToggle.onValueChanged.AddListener(ShowGameSettingsPanel);
        mouseSettingsToggle.onValueChanged.AddListener(ShowMouseSettingsPanle);
        keyboardSettingsToggle.onValueChanged.AddListener(ShowKeyboardSettingsPanel);
        controllerSettingsToggle.onValueChanged.AddListener(ShowControllerSettingsPanel);
    }


    private void ShowGameSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        gameSettingsPanel?.SetActive(_value);
    }

    private void ShowMouseSettingsPanle(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        mouseSettingsPanel?.SetActive(_value);
    }

    private void ShowKeyboardSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        keyboardSettingsPanel?.SetActive(_value);
    }

    private void ShowControllerSettingsPanel(bool _value)
    {
        if (_value == false)
        {
            return;
        }

        HideAllPanels();

        controllerSettingsPanel?.SetActive(_value);
    }

    private void HideAllPanels()
    {
        gameSettingsPanel?.SetActive(false);
        mouseSettingsPanel?.SetActive(false);
        keyboardSettingsPanel?.SetActive(false);
        controllerSettingsPanel?.SetActive(false);
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

        nextToggle.isOn = true;
        lastSwitchPageTime = Time.unscaledTime;
    }

    private bool CheckIfCanSwitchPage()
    {
        if (Time.unscaledTime - lastSwitchPageTime < switchPageCooldown)
        {
            return false;
        }

        return true;
    }


    public void UICancel()
    {
        UIManager.instance?.SwitchState(UIManager.MenuState.PauseMenu);
    }

    public void UIConfirm()
    {
        throw new System.NotImplementedException();
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
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SelectFirstUIItem()
    {

    }


}
