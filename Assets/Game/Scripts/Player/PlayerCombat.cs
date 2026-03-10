using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCombat : MonoBehaviour
{
    private Weapon currentWeapon;
    public Animator anim;

    [Header("IK info")]
    [SerializeField] private TwoBoneIKConstraint leftHandConstraint;
    [SerializeField] private TwoBoneIKConstraint rightHandConstraint;

    public bool isInADS { get; private set; }

    private bool pause = false;

    private void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();

    }

    private void Update()
    {
        if (InputManager.instance.FireHeld)
        {
            if (currentWeapon != null)
            {
                bool fireSucceeded = currentWeapon.TryFire();
                if (fireSucceeded)
                {
                    //play arm fire animation to add animated gunkick
                    //(to combine with code driven gunkick)
                    if (isInADS == false) anim.Play("Fire", 2, 0);
                }
            }

        }

        if (InputManager.instance.ReloadPressed)
        {
            if (currentWeapon.TryReload() == true)
            {
                PlayArmReloadAnimation();
                currentWeapon?.PlayReloadAnimation();

                MakeLeftHandHoldMag();
            }
        }


        if (InputManager.instance.AimDownSightHeld)
        {
            currentWeapon?.EnterADS();
            anim.SetBool("Aim", true);
            isInADS = true;
        }
        else
        {
            currentWeapon?.ExitADS();
            anim.SetBool("Aim", false);
            isInADS = false;
        }

        anim.SetFloat("Aiming", currentWeapon.GetADSAlpha());


        //for test only
        if (Input.GetKeyDown(KeyCode.P))
        {
            pause = !pause;
            anim.speed = pause ? 0 : 1;
        }

    }

    private void PlayArmReloadAnimation()
    {
        if (currentWeapon.CheckIfCurrentMagIsEmpty() == false)
        {
            anim.Play("Reload");
        }
        else
        {
            anim.Play("Reload_Empty");
        }
    }


    public void FillMag()
    {
        currentWeapon?.FillMag();
    }


    #region Reload animation IK
    public void MakeLeftHandHoldMag()
    {
        currentWeapon?.MakeLeftHandHoldMag();
    }

    public void MakeLeftHandReturnToNormalPosition()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(leftHandConstraint, 0.1f, 0.2f, 0.1f, 1, 0));
        currentWeapon.MakeLeftHandReturnToNormalPosition();
    }


    public void MoveLeftHandToEmptyReloadingInsertMagPosition()
    {
        currentWeapon?.MoveLeftHandToEmptyReloadingInsertMagPosition();
    }

    public void MoveLeftHandToEmptyReloadingMidPoint()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(leftHandConstraint, 0.1f, 0.1f, 0.1f, 1, 0.3f));
        currentWeapon?.MoveLeftHandToEmptyReloadingMidPoint();
    }


    public void MakeRightHandGrabBolt()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(rightHandConstraint, 0.1f, 0.1f, 0.1f, 1, 0));
        currentWeapon?.MakeRightHandGrabBolt();
    }


    public void MakeRightHandReturnToNormalPosition()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(rightHandConstraint, 0.2f, 0.1f, 0.2f, 1, 0));
        currentWeapon?.MakeRightHandReturnToNormalPosition();
    }



    private IEnumerator TemporarilyReleaseHandConstraint(TwoBoneIKConstraint _handConstraint, 
        float _fadeOutTime, float _pauseTime, float _fadeInTime, float _highestWeight, float _lowestWeight)
    {
        yield return GraduallyChangeIKWeight(_handConstraint, _highestWeight, _lowestWeight, _fadeOutTime);
        yield return new WaitForSeconds(_pauseTime);
        yield return GraduallyChangeIKWeight(_handConstraint, _lowestWeight, _highestWeight, _fadeInTime);
    }


    private IEnumerator GraduallyChangeIKWeight(TwoBoneIKConstraint _handIKConstraint, float _from, float _to, float _duration)
    {
        float duration = _duration;
        float progress = 0;
        float timer = 0;

        float IKWeight = _from;
        _handIKConstraint.weight = IKWeight;

        while (timer < duration)
        {
            progress = timer / duration;
            progress = Mathf.SmoothStep(0, 1, progress);

            _handIKConstraint.weight = Mathf.Lerp(_from, _to, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        IKWeight = _to;
        _handIKConstraint.weight = IKWeight;
    }
    #endregion

    public void FinishReloading()
    {
        currentWeapon?.FinishReloading();
    }


}
