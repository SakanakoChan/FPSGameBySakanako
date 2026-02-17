using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SettingsItem : MonoBehaviour
{
    public bool isInEditMode { get; protected set; } = false;

    public Selectable selectable { get; private set; }
    private Navigation cachedNavigation;

    protected virtual void Awake()
    {
        selectable = GetComponent<Selectable>();
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
        selectable.navigation = nav;
    }

    protected virtual void UnlockNavigation()
    {
        if (selectable == null)
            return;

        selectable.navigation = cachedNavigation;
    }

}
