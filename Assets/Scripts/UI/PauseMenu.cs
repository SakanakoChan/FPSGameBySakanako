using Rewired;
using Rewired.Integration.UnityUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour, IUIAction
{
    [Header("Button info")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        if (InputManager.instance != null)
        {
            if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
            {
                ClearSelectedUIItem();
            }
            else
            {
                SelectFirstUIItem();
            }
        }
    }

    private void Start()
    {
        SetupButtonNavigation(retryButton, exitButton, settingsButton);
        SetupButtonNavigation(settingsButton, retryButton, exitButton);
        SetupButtonNavigation(exitButton, settingsButton, retryButton);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() => Debug.Log("Fuck you"));
    }


    public void UICancel()
    {
        UIManager.instance?.SwitchState(UIManager.MenuState.None);
    }

    public void UIConfirm()
    {
        throw new System.NotImplementedException();
    }

    public void UISwitchPage(bool _siwtchToRightPage)
    {
        throw new System.NotImplementedException();
    }

    public void ClearSelectedUIItem()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SelectFirstUIItem()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }

    private void SetupButtonNavigation(Button _button, Button _upButton, Button _downButton)
    {
        var navigation = _button.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = _upButton;
        navigation.selectOnDown = _downButton;
        _button.navigation = navigation;
    }


}
