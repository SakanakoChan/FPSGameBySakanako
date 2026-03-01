using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    public enum FireMode
    {
        FullAuto,
        SemiAuto,
        Burst,
        Single
    }

    [Header("Gun info")]
    public int damage;
    public float fireRate;
    public int magSize;
    public int reserveAmmo;
    public FireMode fireMode;

    private int currentAmmoInMagzine;
    private float lastFireTime;
    private float shootInterval;

    [Header("FX info")]
    [SerializeField] private ParticleSystem muzzleFlash_Particle;
    [SerializeField] private GameObject muzzleFlash_Light;
    [SerializeField] private float muzzleFlashLightDuration = 0.05f;


    private void Start()
    {
        lastFireTime = float.NegativeInfinity;
        currentAmmoInMagzine = magSize;
        shootInterval = 60f / fireRate;
    }

    public override void TryFire()
    {
        if (Time.time - lastFireTime < shootInterval)
        {
            Debug.Log("Due to fire rate limit this gun cannot fire now");
            return;
        }

        if (currentAmmoInMagzine <= 0)
        {
            Debug.Log("No ammo in magzine, gun cannot fire");
            return;
        }

        currentAmmoInMagzine--;
        lastFireTime = Time.time;
        ShowMuzzleFlashFx();
    }

    public override void Reload()
    {
        if (currentAmmoInMagzine >= magSize)
        {
            Debug.Log("Mag is full, cannot reload");
            return;
        }

        if (reserveAmmo <= 0)
        {
            Debug.Log("No reserve ammo, cannot reload");
            return;
        }

        int ammoToTakeFromReserveAmmo = magSize - currentAmmoInMagzine;
        ammoToTakeFromReserveAmmo = Mathf.Min(ammoToTakeFromReserveAmmo, reserveAmmo);

        currentAmmoInMagzine += ammoToTakeFromReserveAmmo;
        reserveAmmo -= ammoToTakeFromReserveAmmo;
    }

    private void ShowMuzzleFlashFx()
    {
        if (muzzleFlash_Particle != null)
        {
            muzzleFlash_Particle.Emit(5);
        }

        if (muzzleFlash_Light != null)
        {
            StartCoroutine(ShowMuzzleFlashLightWithDuration(muzzleFlashLightDuration));
        }
    }

    private IEnumerator ShowMuzzleFlashLightWithDuration(float _duration)
    {
        muzzleFlash_Light.SetActive(true);

        yield return new WaitForSeconds(_duration);

        muzzleFlash_Light.SetActive(false);
    }
}
