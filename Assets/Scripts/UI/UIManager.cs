using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

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

        PauseManager.instance.OnPauseStateChanged += ShowPauseMenu;

        ShowPauseMenu(PauseManager.instance.gameIsPaused);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        PauseManager.instance.OnPauseStateChanged -= ShowPauseMenu;
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
