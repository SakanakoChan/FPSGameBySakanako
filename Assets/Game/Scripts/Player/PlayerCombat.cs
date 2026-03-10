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
                anim.Play("Reload");
                currentWeapon?.MakeLeftHandHoldMag();
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

    }


    public void FillMag()
    {
        currentWeapon?.FillMag();
    }

    public void MakeLeftHandHoldMag()
    {
        currentWeapon?.MakeLeftHandHoldMag();
    }

    public void MakeLeftHandReturnToNormalPosition()
    {
        StartCoroutine(MakeLeftHandReturnToNormalPosition_IKTransition());
        //leftHandConstraint.weight = 0.2f;
        currentWeapon.MakeLeftHandReturnToNormalPosition();
    }


    private IEnumerator MakeLeftHandReturnToNormalPosition_IKTransition()
    {
        yield return GraduallyChangeIKWeight(leftHandConstraint, 1, 0f, 0.1f);

        yield return new WaitForSeconds(0.2f);

        yield return GraduallyChangeIKWeight(leftHandConstraint, 0f, 1, 0.1f);
    }

    private IEnumerator GraduallyChangeIKWeight(TwoBoneIKConstraint _handIKConstraint, float _from, float _to, float _duration)
    {
        float duration = _duration;
        float progress = 0;
        float timer = 0;

        float IKWeight = _from;

        while (timer < duration)
        {
            progress = timer / duration;
            progress = Mathf.SmoothStep(0, 1, progress);

            _handIKConstraint.weight = Mathf.Lerp(_from, _to, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        IKWeight = _to;
    }


}
