using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private List<Selectable> settingsItemList;

    private ScrollRect scrollRect;
    private GameObject lastSelectedSettingsItem;

    private void Awake()
    {
        scrollRect = GetComponentInChildren<ScrollRect>();

        SetupSettingsItemNavigation();
    }

    private void OnEnable()
    {
        SelectFirstSettingsItem();
        lastSelectedSettingsItem = EventSystem.current.currentSelectedGameObject;
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
            settingsItemList[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = i > 0 ? settingsItemList[i - 1] : null,
                selectOnDown = i < settingsItemList.Count - 1 ? settingsItemList[i + 1] : null,
                selectOnLeft = null,
                selectOnRight = null
            };
        }
    }

    private void ScrollToSelected()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
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
