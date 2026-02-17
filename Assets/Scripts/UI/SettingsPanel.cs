using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Selectable firstSettingsItem;

    private void OnEnable()
    {
        SelectFirstSettingsItem();
    }

    public void SelectFirstSettingsItem()
    {
        if (firstSettingsItem != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsItem.gameObject);
        }
    }
}
