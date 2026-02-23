using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeviceAwareSelectable : Selectable
{
    //protected override void DoStateTransition(SelectionState state, bool instant)
    //{
    //    if (InputManager.instance != null && InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
    //    {
    //        if (state == SelectionState.Pressed || state == SelectionState.Selected)
    //        {
    //            state = SelectionState.Highlighted;
    //        }
    //    }

    //    base.DoStateTransition(state, instant);
    //}

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.instance != null && InputManager.instance.currentInputDevice == InputDevice.MouseAndKeyboard)
        {
            return;
        }

        base.OnPointerDown(eventData);
    }

    public void ForceEnterHighlightState()
    {
        DoStateTransition(SelectionState.Highlighted, true);
    }

    public void ForceEnterNormalState()
    {
        DoStateTransition(SelectionState.Normal, false);
    }
}
