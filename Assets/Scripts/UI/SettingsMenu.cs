using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour, IUIAction
{
    [Header("Top bar info")]
    [SerializeField] private Toggle gameSettingsToggle;
    [SerializeField] private Toggle mouseSettingsToggle;
    [SerializeField] private Toggle keyboardSettingsToggle;
    [SerializeField] private Toggle controllerSettingsToggle;


    private void Start()
    {
        gameSettingsToggle.onValueChanged.AddListener((isOn) => { if (isOn) Debug.Log("Game settings toggled"); });

       
    }


    public void UICancel()
    {
        UIManager.instance?.SwitchState(UIManager.MenuState.PauseMenu);
    }

    public void UIConfirm()
    {
        throw new System.NotImplementedException();
    }

    public void UISwitchPage(bool _switchToRightPage)
    {
        throw new System.NotImplementedException();
    }

    public void ClearSelectedUIItem()
    {
        throw new System.NotImplementedException();
    }

    public void SelectFirstUIItem()
    {
        throw new System.NotImplementedException();
    }
}
