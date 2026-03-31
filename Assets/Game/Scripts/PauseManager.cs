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

    private void Start()
    {
        InputManager.instance.OnInputDeviceChanged += HandleCursor;

        HandleCursor(InputManager.instance.currentInputDevice);
    }

    private void OnDestroy()
    {
        InputManager.instance.OnInputDeviceChanged -= HandleCursor;
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

    public void PauseGame()
    {
        if (gameIsPaused)
        {
            return;
        }

        gameIsPaused = true;
        Time.timeScale = 0;
        OnPauseStateChanged?.Invoke(gameIsPaused);

        if (InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UnpauseGame()
    {
        if (!gameIsPaused)
        {
            return;
        }

        gameIsPaused = false;
        Time.timeScale = 1;
        OnPauseStateChanged?.Invoke(gameIsPaused);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleCursor(InputDevice _currentInputDevice)
    {
        if (gameIsPaused && _currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
