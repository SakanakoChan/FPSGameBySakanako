using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance { get; private set; }

    public event System.Action<bool> OnPauseStateChanged;

    public bool gameIsPaused { get; private set; } = false;

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

    private void Update()
    {
        if (InputManager.instance.TogglePauseMenuPressed)
        {
            TogglePause();
        }
    }


    private void TogglePause()
    {
        if (gameIsPaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }

        //gameIsPaused = !gameIsPaused;

        //Time.timeScale = gameIsPaused ? 0f : 1f;

        //OnPauseStateChanged?.Invoke(gameIsPaused);
    }

    private void PauseGame()
    {
        if (gameIsPaused)
        {
            return;
        }

        gameIsPaused = true;
        Time.timeScale = 0;
        OnPauseStateChanged?.Invoke(gameIsPaused);
    }

    private void UnpauseGame()
    {
        if (!gameIsPaused)
        {
            return;
        }

        gameIsPaused = false;
        Time.timeScale = 1;
        OnPauseStateChanged?.Invoke(gameIsPaused);
    }
}
