using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIAction
{
    public void UIConfirm();
    public void UICancel();
    public void UISwitchPage(bool _siwtchToRightPage);
    public void ClearSelectedUIItem();
    public void SelectFirstUIItem();
}
