using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gun : Weapon
{
    [SerializeField] private Transform weaponRoot;

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


    [Header("For test only")]
    [SerializeField] private Vector3 cameraKickImpulse = new Vector3(0, 0, 20);


    //public bool isInADS { get; private set; } = false;
    private float adsAlpha = 0;
    private float adsAlphaTargetValue = 0;
    private float adsFOV;
    private float hipFireFOV;
    private float adsFOVAlpha = 0;

    private Camera mainCam;


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

        mainCam = Camera.main;

        hipFireFOV = mainCam.fieldOfView;
        adsFOV = CalculateADSVerticalFOV(hipFireFOV, gunData.adsZoomRatio);

        muzzleFlash_Particle = Instantiate(gunData.muzzleFlash_Particle, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light = Instantiate(gunData.muzzleFlash_Light, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light?.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        cameraRecoil = GetComponentInParent<CameraRecoil>();
        gunKick = GetComponentInParent<GunKick>();
        cameraKick = GetComponentInParent<CameraKick>();
        //audioSource.clip = gunData.fireSound;
    }

    private void Update()
    {
        adsAlphaTargetValue = isInADS ? 1 : 0;
        float currentADSTime = isInADS ? gunData.adsTime : gunData.adsTime * gunData.adsExitTimeMultiplier;

        adsAlpha = Mathf.MoveTowards(adsAlpha, adsAlphaTargetValue, Time.deltaTime / currentADSTime);
        //adsFOVAlpha = Mathf.SmoothStep(0, 1, adsAlpha);

        AnimationCurve adsCurve = gunData.adsCurve;
        //for (int i = 0; i < adsCurve.length; i++)
        //{
        //    adsCurve.SmoothTangents(i, 0f);
        //}

        float easedAlphaValue = adsCurve.Evaluate(adsAlpha);

        //Vector3 gunTargetPosition = isInADS ? gunData.ADSGunPosition : gunData.hipFireGunPosition;
        //Vector3 gunTargetRotationEuler = isInADS ? gunData.ADSGunRotationEuler : gunData.hipFireGunRotationEuler;

        if (weaponRoot != null)
        {
            weaponRoot.localPosition = Vector3.Lerp(gunData.hipFireGunPosition, gunData.ADSGunPosition, easedAlphaValue);
            weaponRoot.localRotation =
                Quaternion.Lerp
                (
                    Quaternion.Euler(gunData.hipFireGunRotationEuler),
                    Quaternion.Euler(gunData.ADSGunRotationEuler),
                    easedAlphaValue
                );

            mainCam.fieldOfView = Mathf.Lerp(hipFireFOV, adsFOV, adsAlpha);
        }


    }

    public override bool TryFire()
    {
        if (Time.time - lastFireTime < shootInterval)
        {
            //Debug.Log("Due to fire rate limit this gun cannot fire now");
            return false;
        }

        if (currentAmmoInMagzine <= 0)
        {
            //Debug.Log("No ammo in magzine, gun cannot fire");
            return false;
        }


        ApplyRecoilRecovery();

        currentAmmoInMagzine--;
        lastFireTime = Time.time;

        ShowMuzzleFlashFx();

        SpawnBullet();

        PlayFireSound();

        AddRecoil();

        return true;
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

        float sign = recoilImpulse.x > 0 ? 1 : -1;


        //******Position GunKick
        Vector3 positionGunKickImpulse = CalculatePositionGunKickImpulse(sign);
        //positionGunKickImpulse *= 0f;
        //Vector3 positionGunKickImpulse = 0.1f * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-3f, -5f));

        //******Rotation GunKick
        Vector3 rotationGunKickImpulse = CalculateRotationGunKickImpulse(sign);
        //rotationGunKickImpulse = Vector3.zero;
        //Vector3 rotationGunKickImpulse = 15f * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-2f, 2f));
        gunKick?.AddGunKick(positionGunKickImpulse, rotationGunKickImpulse);



        //******CameraKick
        Vector3 rotationCameraKickImpulse =
            -sign * Random.Range(gunData.cameraKickMultiplierRange_Min, gunData.cameraKickMultiplier_Max) * gunData.basicCameraKick;
        cameraKick?.AddCameraKick(rotationCameraKickImpulse);
    }

    private Vector3 CalculateRotationGunKickImpulse(float sign)
    {
        float rotationGunKickX = /*0 **/ Random.Range(-15f, -5f);
        float rotationGunKickY =
            sign *
            Random.Range(gunData.rotationGunKickMultiplier_Min, gunData.rotationGunKickMultiplier_Max) *
            gunData.basicRotationGunKick.y * 0;
        float rotationGunKickZ =
            -sign *
            Random.Range(gunData.rotationGunKickMultiplier_Min, gunData.rotationGunKickMultiplier_Max) *
            gunData.basicRotationGunKick.z * 0;

        //Quaternion gunKickRotationInCamSpace =
        //    Quaternion.AngleAxis(rotationGunKickX, mainCam.transform.right) *
        //    Quaternion.AngleAxis(rotationGunKickY, mainCam.transform.up) *
        //    Quaternion.AngleAxis(rotationGunKickZ, mainCam.transform.forward);

        //Quaternion gunKickRotationInLocalSpace = Quaternion.Inverse(gunKick.transform.parent.rotation) * gunKickRotationInCamSpace;

        Vector3 rotationGunKickImpulse = new Vector3(rotationGunKickX, rotationGunKickY, rotationGunKickZ);
        //Vector3 rotationGunKickImpulse = gunKickRotationInLocalSpace.eulerAngles;
        if (isInADS)
        {
            rotationGunKickImpulse *= 2f;
        }

        return rotationGunKickImpulse;
    }

    private Vector3 CalculatePositionGunKickImpulse(float sign)
    {
        float positionGunKickX =
            -sign *
            Random.Range(gunData.positionGunKickMultiplier_Min, gunData.positionGunKickMultiplier_Max) *
            Random.Range(0, gunData.basicPositionGunKick.x);
        Vector3 x = positionGunKickX * mainCam.transform.right;

        float positionGunKickY = 0 * Random.Range(0f, 0.5f);
        Vector3 y = positionGunKickY * mainCam.transform.up;


        float positionGunKickZ =
            Random.Range(gunData.positionGunKickMultiplier_Min, gunData.positionGunKickMultiplier_Max) *
            gunData.basicPositionGunKick.z;
        Vector3 z = positionGunKickZ * mainCam.transform.forward;

        Vector3 temp = x + y + z;

        Vector3 positionGunKickImpulse = new Vector3(positionGunKickX, positionGunKickY, positionGunKickZ);
        //Vector3 positionGunKickImpulse = gunKick.transform.parent.InverseTransformVector(temp);
        if (isInADS)
        {
            positionGunKickImpulse *= 0.4f;
        }

        return positionGunKickImpulse;
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

    public override void EnterADS()
    {
        isInADS = true;
    }

    public override void ExitADS()
    {
        isInADS = false;
    }

    public override float GetADSAlpha()
    {
        return adsAlpha;
    }

    private float CalculateADSVerticalFOV(float hipFOV, float zoomMultiplier)
    {
        float hipFOVRad = hipFOV * Mathf.Deg2Rad;

        float adsFOVRad = 2f * Mathf.Atan(Mathf.Tan(hipFOVRad * 0.5f) / zoomMultiplier);

        return adsFOVRad * Mathf.Rad2Deg;
    }
}
