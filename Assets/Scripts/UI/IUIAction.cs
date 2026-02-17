using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIAction
{
    public void UIConfirm();
    public void UICancel();

    public void UISwitchPageRight();
    public void UISwitchPageLeft();

    public void ClearSelectedUIItem();
    public void SelectFirstUIItem();
}
