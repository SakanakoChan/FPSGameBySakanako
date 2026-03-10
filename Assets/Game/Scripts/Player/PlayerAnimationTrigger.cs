using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTrigger : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    public void FillMag()
    {
        playerCombat?.FillMag();
    }

    public void MakeLeftHandReturnToNormalPosition()
    {
        playerCombat?.MakeLeftHandReturnToNormalPosition();
    }

    public void MoveLeftHandToEmptyReloadingInsertMagPosition()
    {
        playerCombat?.MoveLeftHandToEmptyReloadingInsertMagPosition();
    }

    public void MoveLeftHandToEmptyReloadingMidPoint()
    {
        playerCombat?.MoveLeftHandToEmptyReloadingMidPoint();
    }

    public void MakeRightHandGrabBolt()
    {
        playerCombat?.MakeRightHandGrabBolt();
    }

    public void MakeRightHandReturnToNormalPosition()
    {
        playerCombat?.MakeRightHandReturnToNormalPosition();
    }

    public void FinishReloading()
    {
        playerCombat?.FinishReloading();
    }
}
