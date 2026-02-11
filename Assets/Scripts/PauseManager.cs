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
        gameIsPaused = !gameIsPaused;

        Time.timeScale = gameIsPaused ? 0f : 1f;

        UIManager.instance.ShowPauseMenu(gameIsPaused);

        OnPauseStateChanged?.Invoke(gameIsPaused);
    }
}
