using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gun : Weapon
{
    [Header("Gun data")]
    [SerializeField] private GunData gunData;

    private int currentAmmoInMagzine;
    private int reserveAmmo;
    private float lastFireTime;
    private float shootInterval;


    [Header("Bullet info")]
    [SerializeField] private Transform bulletSpawnPosition;


    [Header("FX info")]
    [SerializeField] private Transform muzzleFlashPosition;
    private ParticleSystem muzzleFlash_Particle;
    private GameObject muzzleFlash_Light;

    private AudioSource audioSource;
    private CameraRecoil cameraRecoil;
    private GunKick gunKick;
    private CameraKick cameraKick;

    private int currentRecoilIndex = 0;



    private void Start()
    {
        if (gunData == null)
        {
            Debug.LogError("Didn't assign gun data for this gun: " + gameObject.name);
        }

        lastFireTime = float.NegativeInfinity;
        currentAmmoInMagzine = gunData.magSize;
        reserveAmmo = gunData.reserveAmmo;
        shootInterval = 60f / gunData.fireRate;

        muzzleFlash_Particle = Instantiate(gunData.muzzleFlash_Particle, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light = Instantiate(gunData.muzzleFlash_Light, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light?.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        cameraRecoil = GetComponentInParent<CameraRecoil>();
        gunKick = GetComponentInParent<GunKick>();
        cameraKick = GetComponentInParent<CameraKick>();
        //audioSource.clip = gunData.fireSound;
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


        ApplyRecoilRecovery();

        currentAmmoInMagzine--;
        lastFireTime = Time.time;

        ShowMuzzleFlashFx();

        SpawnBullet();

        PlayFireSound();

        AddRecoil();
    }

    public override void Reload()
    {
        if (currentAmmoInMagzine >= gunData.magSize)
        {
            Debug.Log("Mag is full, cannot reload");
            return;
        }

        if (reserveAmmo <= 0)
        {
            Debug.Log("No reserve ammo, cannot reload");
            return;
        }

        int ammoToTakeFromReserveAmmo = gunData.magSize - currentAmmoInMagzine;
        ammoToTakeFromReserveAmmo = Mathf.Min(ammoToTakeFromReserveAmmo, reserveAmmo);

        currentAmmoInMagzine += ammoToTakeFromReserveAmmo;
        reserveAmmo -= ammoToTakeFromReserveAmmo;

        currentRecoilIndex = 0;
    }

    private void AddRecoil()
    {
        Vector2 recoilImpulse = gunData.recoilPatternList[currentRecoilIndex].recoilImpulse;
        cameraRecoil?.AddRecoilImpulse(recoilImpulse);

        currentRecoilIndex++;
        currentRecoilIndex = Mathf.Clamp(currentRecoilIndex, 0, gunData.recoilPatternList.Count - 1);

        Vector3 positionGunKickImpulse = 0.1f * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-3f, -5f));
        Vector3 rotationGunKickImpulse = 15f * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-2f, 2f));
        gunKick?.AddGunKick(positionGunKickImpulse, rotationGunKickImpulse);

        Vector3 rotationCameraKickImpulse = new Vector3(0, 0, Random.Range(-10f, 10f));
        cameraKick?.AddCameraKick(rotationCameraKickImpulse);
    }

    private void ApplyRecoilRecovery()
    {
        if (Time.time - lastFireTime > gunData.recoilRecoveryDelay)
        {
            float recoveryTime = Time.time - lastFireTime - gunData.recoilRecoveryDelay;
            int recoveryShots = Mathf.FloorToInt(recoveryTime / gunData.recoilRecoveryInterval);

            currentRecoilIndex -= recoveryShots;
            currentRecoilIndex = Mathf.Clamp(currentRecoilIndex, 0, gunData.recoilPatternList.Count - 1);
        }
    }

    private void ShowMuzzleFlashFx()
    {
        if (muzzleFlash_Particle != null)
        {
            muzzleFlash_Particle.Emit(5);
        }

        if (muzzleFlash_Light != null)
        {
            StartCoroutine(ShowMuzzleFlashLightWithDuration(gunData.muzzleFlashLightDuration));
        }
    }

    private IEnumerator ShowMuzzleFlashLightWithDuration(float _duration)
    {
        muzzleFlash_Light.SetActive(true);

        yield return new WaitForSeconds(_duration);

        muzzleFlash_Light.SetActive(false);
    }

    private void SpawnBullet()
    {
        //raycast detection
        Camera mainCam = Camera.main;
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, gunData.maxRange))
        {
            Debug.Log("Bullet hit target: " + hit.collider.name);
        }
        else
        {
            hit.point = ray.origin + ray.direction * gunData.maxRange;
            Debug.LogFormat("Bullet didnt hit any target, stopped at its max range: " + gunData.maxRange);
        }

        //spawn projectile
        GameObject bullet = Instantiate(gunData.bulletPrefab);
        var projectile = bullet.GetComponent<Projectile>();

        Vector3 bulletFlyDirection = (hit.point - bulletSpawnPosition.position).normalized;
        Vector3 initialVelocity = bulletFlyDirection * gunData.bulletFlySpeed;
        projectile?.SetupProjectile(initialVelocity, gunData.bulletGravity, bulletSpawnPosition);
    }

    private void PlayFireSound()
    {
        float remainingAmmoPercentInCurrentMag = (float)currentAmmoInMagzine / (float)gunData.magSize;
        audioSource.pitch = 1 + Mathf.Pow((1 - remainingAmmoPercentInCurrentMag) * 0.25f, 1.5f);
        audioSource.PlayOneShot(gunData.fireSound);
    }
}
