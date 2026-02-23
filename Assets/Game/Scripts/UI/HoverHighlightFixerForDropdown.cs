using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverHighlightFixerForDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Selectable selectable;

    private void Start()
    {
        selectable = GetComponentInParent<Selectable>();
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
