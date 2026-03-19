using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponInfoIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI ammoInMag;
    [SerializeField] private TextMeshProUGUI reserveAmmo;

    public void UpdateWeaponName(string _weaponName)
    {
        weaponName.text = _weaponName;
    }

    public void UpdateAmmoInfo(int _ammoInMag, int _reserveAmmo)
    {
        ammoInMag.text = _ammoInMag.ToString();
        reserveAmmo.text = _reserveAmmo.ToString();
    }
}
