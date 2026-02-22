using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsItemSelectablePointerForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Selectable selectable;

    private void Start()
    {
        var settingsItem = GetComponentInParent<SettingsItem>();
        selectable = settingsItem.GetComponent<Selectable>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        selectable?.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selectable?.OnPointerExit(eventData);
    }
}
