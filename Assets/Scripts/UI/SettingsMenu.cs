using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour, IUIAction
{
    [SerializeField] private List<Button> topBarButtonList;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(topBarButtonList[0].gameObject);
    }

    private void Update()
    {
        Debug.Log("Current selected item: " + EventSystem.current.firstSelectedGameObject);
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
