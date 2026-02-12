using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    private enum MenuState
    {
        None,
        PauseMenu,
        SettingsMenu
    }

    private MenuState currentState = MenuState.None;

    private GameObject pauseMenu;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        ShowPauseMenu(PauseManager.instance.gameIsPaused);
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
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region Statemachine Design
    private void SwitchState(MenuState _targetState)
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
                InputManager.instance?.EnterUIMapMode(false);
                break;

            case MenuState.PauseMenu:
                PauseManager.instance?.PauseGame();
                ShowPauseMenu(true);
                InputManager.instance?.EnterUIMapMode(true);
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
        switch (currentState)
        {
            case MenuState.None:
                break;

            case MenuState.PauseMenu:
                SwitchState(MenuState.None);
                break;
        }
    }


    private void OnSceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        Debug.Log("Scene loaded: " + _scene.name);

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>(true);

        if (pauseMenu != null)
        {
            RegisterPauseMenu(pauseMenu.gameObject);
        }
    }


    public void RegisterPauseMenu(GameObject _pauseMenu)
    {
        pauseMenu = _pauseMenu;
    }

    public void ShowPauseMenu(bool _value)
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(_value);
        }
    }


}
