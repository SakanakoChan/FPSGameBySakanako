using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Weapon currentWeapon;

    private void Start()
    {
         currentWeapon = GetComponentInChildren<Weapon>();
    }

    private void Update()
    {
        if (InputManager.instance.FireHeld)
        {
            currentWeapon?.TryFire();
        }

        if (InputManager.instance.ReloadPressed)
        {
            currentWeapon?.Reload();
        }
    }
}
