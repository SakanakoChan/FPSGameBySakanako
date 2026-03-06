using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Weapon currentWeapon;
    public Animator anim;

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
                    //anim.Play("Fire", 2, 0);
                }
            }

        }

        if (InputManager.instance.ReloadPressed)
        {
            currentWeapon?.Reload();
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
}
