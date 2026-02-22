using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SettingsItem : MonoBehaviour, ISettingsDataAction
{
    public bool isInEditMode { get; protected set; } = false;

    public Selectable selectable { get; private set; }
    private Navigation cachedNavigation;

    protected virtual void Awake()
    {
        selectable = GetComponent<Selectable>();

        if (selectable == null)
        {
            Debug.LogWarning($"{gameObject.name} has no Selectable component.");
        }

        //SaveManager.instance?.RegisterSettingsDataAction(this);
    }

    protected virtual void OnDestroy()
    {
        //SaveManager.instance?.UnregisterSettingsDataAction(this);
    }


    public abstract void Confirm();

    public virtual void Cancel()
    {

    }

    protected virtual void LockNavigation()
    {
        if (selectable == null)
            return;

        cachedNavigation = selectable.navigation;

        var nav = selectable.navigation;

        nav.mode = Navigation.Mode.None;
        nav.selectOnRight = null;
        nav.selectOnLeft = null;
        nav.selectOnUp = null;
        nav.selectOnDown = null;

        selectable.navigation = nav;
    }

    protected virtual void UnlockNavigation()
    {
        if (selectable == null)
            return;

        selectable.navigation = cachedNavigation;
    }

    protected void SetEditMode(bool _editMode)
    {
        if (isInEditMode == _editMode)
        {
            return;
        }

        isInEditMode = _editMode;
        OnEditModeChanged(_editMode);
    }

    protected virtual void OnEditModeChanged(bool _editMode)
    {
        if (_editMode == true)
        {
            LockNavigation();
        }
        else
        {
            UnlockNavigation();
        }
    }

    public abstract void LoadData();

    public abstract void SaveData();

}
