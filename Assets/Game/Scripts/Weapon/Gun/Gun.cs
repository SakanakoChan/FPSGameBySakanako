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

    [Space]
    public float maxRange = 1000;

    private int currentAmmoInMagzine;
    private float lastFireTime;
    private float shootInterval;


    [Header("Bullet info")]
    public float bulletFlySpeed = 700;
    public float bulletGravity = -5f;
    [SerializeField] private Transform bulletSpawnPosition;
    [SerializeField] private GameObject bulletPrefab;



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
            //Debug.Log("Due to fire rate limit this gun cannot fire now");
            return;
        }

        if (currentAmmoInMagzine <= 0)
        {
            //Debug.Log("No ammo in magzine, gun cannot fire");
            return;
        }

        currentAmmoInMagzine--;
        lastFireTime = Time.time;
        ShowMuzzleFlashFx();

        //raycast detection
        Camera mainCam = Camera.main;
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxRange))
        {
            Debug.Log("Bullet hit target: " + hit.collider.name);
        }
        else
        {
            hit.point = ray.origin + ray.direction * maxRange;
            Debug.LogFormat("Bullet didnt hit any target, stopped at its max range: " + maxRange);
        }

        //spawn projectile
        GameObject bullet = Instantiate(bulletPrefab);
        var projectile = bullet.GetComponent<Projectile>();

        Vector3 bulletFlyDirection = (hit.point - bulletSpawnPosition.position).normalized;
        Vector3 initialVelocity = bulletFlyDirection * bulletFlySpeed;
        projectile?.SetupProjectile(initialVelocity, bulletGravity, bulletSpawnPosition);
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
