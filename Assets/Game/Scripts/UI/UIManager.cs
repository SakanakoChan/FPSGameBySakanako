using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    public enum MenuState
    {
        None,
        PauseMenu,
        SettingsMenu
    }

    private MenuState currentState = MenuState.None;

    private PauseMenu pauseMenu;
    private SettingsMenu settingsMenu;

    private IUIAction currentUIAction = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InputManager.instance.OnInputDeviceChanged += OnInputDeviceChanged;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        currentState = MenuState.None;
        EnterState(MenuState.None);
    }

    private void Update()
    {
        if (InputManager.instance.OpenPauseMenuPressed)
        {
            HandleOpenPauseMenu();
        }

        if (InputManager.instance.UICancelPressed)
        {
            HandleUICancel();
        }

        if(InputManager.instance.UIConfirmPressed)
        {
            HandleUIConfirm();
        }

        if (InputManager.instance.UISwitchPageRightPressed)
        {
            HandleUISwitchPageRight();
        }

        if (InputManager.instance.UISwitchPageLeftPressed)
        {
            HandleUISwitchPageLeft();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        InputManager.instance.OnInputDeviceChanged -= OnInputDeviceChanged;
    }

    #region Statemachine Design
    public void SwitchState(MenuState _targetState)
    {
        if (currentState == _targetState)
        {
            return;
        }

        ExitState(currentState);
        currentState = _targetState;
        EnterState(_targetState);
    }

    private void EnterState(MenuState _targetState)
    {
        switch (_targetState)
        {
            case MenuState.None:
                PauseManager.instance?.UnpauseGame();
                ShowPauseMenu(false);
                ShowSettingsMenu(false);
                InputManager.instance?.EnterUIMapMode(false);
                currentUIAction = null;
                break;

            case MenuState.PauseMenu:
                PauseManager.instance?.PauseGame();
                ShowPauseMenu(true);
                ShowSettingsMenu(false);
                InputManager.instance?.EnterUIMapMode(true);
                currentUIAction = pauseMenu;
                break;

            case MenuState.SettingsMenu:
                ShowSettingsMenu(true);
                ShowPauseMenu(false);
                InputManager.instance?.EnterUIMapMode(true);
                currentUIAction = settingsMenu;
                break;
        }
    }

    private void ExitState(MenuState _state)
    {
        switch (_state)
        {
            case MenuState.None:
                break;

            case MenuState.PauseMenu:
                break;
        }
    }
    #endregion

    private void HandleOpenPauseMenu()
    {
        switch (currentState)
        {
            case MenuState.None:
                SwitchState(MenuState.PauseMenu);
                break;
        }
    }

    private void HandleUICancel()
    {
        if (currentState == MenuState.None)
        {
            return;
        }

        currentUIAction?.UICancel();
    }

    private void HandleUIConfirm()
    {
        if (currentState == MenuState.None)
        {
            return;
        }

        currentUIAction?.UIConfirm();
    }

    private void HandleUISwitchPageRight()
    {
        if (currentState == MenuState.None)
        {
            return;
        }

        currentUIAction?.UISwitchPageRight();
    }

    private void HandleUISwitchPageLeft()
    {
        if (currentState == MenuState.None)
        {
            return;
        }

        currentUIAction?.UISwitchPageLeft();
    }

    private void OnInputDeviceChanged(InputDevice _currentInputDevice)
    {
        if (currentState == MenuState.None)
        {
            return;
        }

        if (_currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            currentUIAction?.ClearSelectedUIItem();
        }
        else if (_currentInputDevice == InputDevice.Controller)
        {
            // Set selected UI item to first button in menu
            currentUIAction?.SelectFirstUIItem();
        }
    }


    private void OnSceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        Debug.Log("Scene loaded: " + _scene.name);

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>(true);

        if (pauseMenu != null)
        {
            RegisterPauseMenu(pauseMenu);
        }

        SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>(true);

        if (settingsMenu != null)
        {
            RegisterSettingsMenu(settingsMenu);
        }

        currentState = MenuState.None;
        EnterState(MenuState.None);
    }


    public void RegisterPauseMenu(PauseMenu _pauseMenu)
    {
        pauseMenu = _pauseMenu;
    }

    public void ShowPauseMenu(bool _value)
    {
        if (pauseMenu != null)
        {
            pauseMenu.gameObject.SetActive(_value);
        }
    }

    public void RegisterSettingsMenu(SettingsMenu _settingsMenu)
    {
        settingsMenu = _settingsMenu;
    }

    public void ShowSettingsMenu(bool _value)
    {
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(_value);
        }
    }


}
