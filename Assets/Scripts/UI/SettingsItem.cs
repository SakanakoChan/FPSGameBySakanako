using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SettingsItem : MonoBehaviour
{
    public abstract void Confirm();

    public virtual void Cancel()
    {

    }

}
