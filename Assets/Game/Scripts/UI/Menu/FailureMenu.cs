using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FailureMenu : MonoBehaviour, IUIAction
{
    [Header("Button info")]
    [SerializeField] private Button retryButton;
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
        SetupButtonNavigation(retryButton, exitButton, exitButton);
        SetupButtonNavigation(exitButton, retryButton, retryButton);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(Retry);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(ExitGame);
    }

    private void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    public void UICancel()
    {
        //UIManager.instance?.SwitchState(UIManager.MenuState.None);
    }

    public void UIConfirm()
    {

    }

    public void UISwitchPageRight()
    {

    }

    public void UISwitchPageLeft()
    {

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
