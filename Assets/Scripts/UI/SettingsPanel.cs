using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    private List<SettingsItem> settingsItemList;

    private ScrollRect scrollRect;
    private GameObject lastSelectedSettingsItem;

    private void Awake()
    {
        settingsItemList = GetComponentsInChildren<SettingsItem>(true).ToList();
        scrollRect = GetComponentInChildren<ScrollRect>();
    }

    private void OnEnable()
    {
        if (InputManager.instance != null && InputManager.instance.currentInputDevice == InputDevice.Controller)
        {
            SelectFirstSettingsItem();
        }

        lastSelectedSettingsItem = EventSystem.current.currentSelectedGameObject;
    }

    private void Start()
    {
        //put it here to avoid the problem that settingsItem.selectable in Awake
        SetupSettingsItemNavigation();
    }

    private void LateUpdate()
    {
        var currentSelectedSettingsItem = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedSettingsItem != lastSelectedSettingsItem)
        {
            lastSelectedSettingsItem = currentSelectedSettingsItem;
            ScrollToSelected();
        }
    }


    public void SelectFirstSettingsItem()
    {
        StartCoroutine(SelectFirstSettingsItem_Coroutine());
    }

    private IEnumerator SelectFirstSettingsItem_Coroutine()
    {
        //wait 1 frame to avoid some strange bugs
        //like the first settings item will enter edit mode
        yield return null;

        if (settingsItemList != null && settingsItemList.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsItemList[0].gameObject);
        }
    }

    private void SetupSettingsItemNavigation()
    {
        for (int i = 0; i < settingsItemList.Count; i++)
        {
            var selectable = settingsItemList[i].selectable;
            if (selectable == null)
                continue;

            selectable.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = i > 0 ? settingsItemList[i - 1].selectable : null,
                selectOnDown = i < settingsItemList.Count - 1 ? settingsItemList[i + 1].selectable : null,
                selectOnLeft = null,
                selectOnRight = null
            };
        }
    }

    private void ScrollToSelected()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || selected.GetComponent<SettingsItem>() == null)
            return;

        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        Canvas.ForceUpdateCanvases();

        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, selectedRect);
        Bounds viewBounds = new Bounds(viewport.rect.center, viewport.rect.size);

        float offset = 0f;

        // 如果在上面超出
        if (itemBounds.max.y > viewBounds.max.y)
        {
            offset = itemBounds.max.y - viewBounds.max.y;
        }
        // 如果在下面超出
        else if (itemBounds.min.y < viewBounds.min.y)
        {
            offset = itemBounds.min.y - viewBounds.min.y;
        }

        if (Mathf.Abs(offset) > 0.01f)
        {
            Vector2 pos = content.anchoredPosition;
            pos.y -= offset;
            content.anchoredPosition = pos;
        }
    }
}
