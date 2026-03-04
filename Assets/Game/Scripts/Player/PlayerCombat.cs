using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Weapon currentWeapon;
    public Animator anim;

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
                    anim.Play("Fire", 2, 0);
                }
            }

        }

        if (InputManager.instance.ReloadPressed)
        {
            currentWeapon?.Reload();
        }
    }
}
