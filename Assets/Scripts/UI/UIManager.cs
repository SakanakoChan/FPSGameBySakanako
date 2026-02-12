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
        if (InputManager.instance.TogglePauseMenuPressed)
        {
            HandleTogglePauseMenu();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    private void HandleTogglePauseMenu()
    {
        switch (currentState)
        {
            case MenuState.None:
                PauseManager.instance?.PauseGame();
                ShowPauseMenu(true);
                currentState = MenuState.PauseMenu;
                break;

            case MenuState.PauseMenu:
                PauseManager.instance?.UnpauseGame();
                ShowPauseMenu(false);
                currentState = MenuState.None;
                break;
        }
    }
}
