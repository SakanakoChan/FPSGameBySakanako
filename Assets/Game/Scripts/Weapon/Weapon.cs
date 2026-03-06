using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public bool isInADS { get; protected set; } = false;

    public abstract bool TryFire();
    public abstract void Reload();

    public virtual void EnterADS() 
    { 
    }


    public virtual void ExitADS() 
    { 
    }

    public virtual float GetADSAlpha()
    {
        return 0;
    }
}
