using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public bool isInADS { get; protected set; } = false;

    public bool isReloading { get; protected set; } = false;

    public abstract bool TryFire();
    public abstract bool TryReload();

    public virtual void FinishReloading()
    {
        isReloading = false;
    }

    public virtual void PlayReloadAnimation()
    {

    }

    public virtual void FillMag()
    {

    }

    #region Reload animation IK

    public virtual void MakeLeftHandHoldMag()
    {

    }

    public virtual void MakeLeftHandReturnToNormalPosition()
    {

    }

    public virtual void MoveLeftHandToEmptyReloadingInsertMagPosition()
    {

    }

    public virtual void MoveLeftHandToEmptyReloadingMidPoint()
    {

    }

    public virtual void MakeRightHandGrabBolt()
    {

    }

    public virtual void MakeRightHandReturnToNormalPosition()
    {

    }
    #endregion

    public virtual bool CheckIfCurrentMagIsEmpty()
    {
        return false;
    }

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

    public virtual void EjectCasing()
    {

    }
}
