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

    [Header("ADS aimpoint info")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private HipFireCrosshair hipFireCrosshair;
    private Canvas hudCanvas;


    [Header("Bullet info")]
    [SerializeField] private Transform bulletSpawnPosition;
    private Vector3 logicBulletStartPosition;


    [Header("FX info")]
    [SerializeField] private Transform muzzleFlashPosition;
    private ParticleSystem muzzleFlash_Particle;
    private GameObject muzzleFlash_Light;

    private AudioSource audioSource;
    private CameraRecoil cameraRecoil;
    private GunKick gunKick;
    private CameraKick cameraKick;

    private int currentRecoilIndex = 0;


    [Header("IK Target info")]
    [SerializeField] private Transform leftHandFollowTarget;
    [SerializeField] private Transform leftHandFollowPosition_Normal;
    [SerializeField] private Transform leftHandFollowPosition_Reloading;

    private Animator anim;


    [Header("For test only")]
    [SerializeField] private Vector3 cameraKickImpulse = new Vector3(0, 0, 20);


    //public bool isInADS { get; private set; } = false;
    public float adsAlpha { get; private set; } = 0;
    private float adsAlphaTargetValue = 0;
    public float adsFOV { get; private set; }
    public float hipFireFOV { get; private set; }
    //private float adsFOVAlpha = 0;


    #region Spread fields
    //basic spread
    private float currentBasicSpreadAngle;
    private float currentBasicMinSpreadAngle;
    private float currentBasicMaxSpreadAngle;

    //shot spread punishment
    private float currentShotSpreadPunishment;
    private float currentMaxShotSpreadPunishment;

    //move speed spread punishment
    private float currentMoveSpeedSpreadPunishment;
    private float currentMaxMoveSpeedSpreadPunishment;

    //air spread punishment
    private float currentAirSpreadPunishment;

    //final spread
    private float currentFinalSpreadAngle;
    #endregion



    private PlayerMovement playerMovement;
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

        currentBasicSpreadAngle = gunData.basicHipFireSpreadAngle;
        currentBasicMaxSpreadAngle = gunData.maxHipFireSpreadAngle;
        currentBasicMinSpreadAngle = gunData.basicHipFireSpreadAngle;

        currentShotSpreadPunishment = 0;
        currentMoveSpeedSpreadPunishment = 0;
        currentAirSpreadPunishment = 0;

        currentFinalSpreadAngle = currentBasicSpreadAngle;

        logicBulletStartPosition = mainCam.transform.position;

        muzzleFlash_Particle = Instantiate(gunData.muzzleFlash_Particle, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light = Instantiate(gunData.muzzleFlash_Light, muzzleFlashPosition.position, muzzleFlashPosition.rotation, muzzleFlashPosition.parent);
        muzzleFlash_Light?.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        cameraRecoil = GetComponentInParent<CameraRecoil>();
        gunKick = GetComponentInParent<GunKick>();
        cameraKick = GetComponentInParent<CameraKick>();
        anim = GetComponent<Animator>();

        hudCanvas = hipFireCrosshair.GetComponentInParent<Canvas>();

        playerMovement = GetComponentInParent<PlayerMovement>();
        //audioSource.clip = gunData.fireSound;
    }

    private void Update()
    {
        ADSLogic();

        UpdateBasicSpread();
        UpdateShotSpreadPunishment();
        UpdateAirSpreadPunishment();
        UpdateMoveSpeedSpreadPunishment();

        UpdateFinalSpreadAngle();

        SyncSpreadWithHipFireCrosshair();
    }

    private void UpdateMoveSpeedSpreadPunishment()
    {
        //change move speed spread punishment
        currentMaxMoveSpeedSpreadPunishment = Mathf.Lerp(gunData.maxHipFireMoveSpeedPunishment, 0, adsAlpha);

        currentMoveSpeedSpreadPunishment = playerMovement.horizontalVelocity.magnitude * gunData.moveSpeedHipFireSpreadPunishmentRatio;
        currentMoveSpeedSpreadPunishment = Mathf.Clamp(currentMoveSpeedSpreadPunishment, 0, currentMaxMoveSpeedSpreadPunishment);
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

        AddShotSpread();

        UpdateFinalSpreadAngle();

        return true;
    }



    public override bool TryReload()
    {
        if (currentAmmoInMagzine >= gunData.magSize)
        {
            Debug.Log("Mag is full, cannot reload");
            return false;
        }

        if (reserveAmmo <= 0)
        {
            Debug.Log("No reserve ammo, cannot reload");
            return false;
        }

        return true;
    }

    #region Reload related functions
    public override void PlayReloadAnimation()
    {
        anim?.Play("Reload");
    }

    public override void FillMag()
    {
        int ammoToTakeFromReserveAmmo = gunData.magSize - currentAmmoInMagzine;
        ammoToTakeFromReserveAmmo = Mathf.Min(ammoToTakeFromReserveAmmo, reserveAmmo);

        currentAmmoInMagzine += ammoToTakeFromReserveAmmo;
        reserveAmmo -= ammoToTakeFromReserveAmmo;

        currentRecoilIndex = 0;
    }

    public override void MakeLeftHandHoldMag()
    {
        StartCoroutine(ChangeLeftHandFollowTarget(leftHandFollowPosition_Reloading, 0.25f));
    }

    public override void MakeLeftHandReturnToNormalPosition()
    {
        StartCoroutine(ChangeLeftHandFollowTarget(leftHandFollowPosition_Normal, 0.4f));
    }


    private IEnumerator ChangeLeftHandFollowTarget(Transform _targetPosition, float _changeDuration)
    {
        float duration = _changeDuration;
        float timer = 0;
        float progress = 0;

        leftHandFollowTarget.SetParent(_targetPosition.parent);
        Vector3 startLocalPosition = leftHandFollowTarget.localPosition;
        Quaternion startLocalRotation = leftHandFollowTarget.localRotation;

        Vector3 endLocalPosition = _targetPosition.localPosition;
        Quaternion endLocalRotation = _targetPosition.localRotation;

        while (timer < duration)
        {
            progress = timer / duration;
            progress = Mathf.SmoothStep(0, 1, progress);

            leftHandFollowTarget.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, progress);
            leftHandFollowTarget.localRotation = Quaternion.Lerp(startLocalRotation, endLocalRotation, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        leftHandFollowTarget.localPosition = endLocalPosition;
        leftHandFollowTarget.localRotation = endLocalRotation;
    }
    #endregion


    private void ADSLogic()
    {
        float currentADSTime;

        if (isInADS)
        {
            adsAlphaTargetValue = 1;
            currentADSTime = gunData.adsTime;
        }
        else
        {
            adsAlphaTargetValue = 0;
            currentADSTime = gunData.adsTime * gunData.adsExitTimeMultiplier;
        }


        adsAlpha = Mathf.MoveTowards(adsAlpha, adsAlphaTargetValue, Time.deltaTime / currentADSTime);

        AnimationCurve adsCurve = gunData.adsCurve;
        float easedAlphaValue = adsCurve.Evaluate(adsAlpha);

        FadeHipFireCrosshair();

        GunPositionADSTransition(easedAlphaValue);

        FOVADSTransition();

        CrosshairADSTransition();
    }

    private void FadeHipFireCrosshair()
    {
        float crosshairFadeThreshold = 0.5f;
        if (adsAlpha >= crosshairFadeThreshold)
        {
            float crossHairAlphaValue = 1 - ((adsAlpha - crosshairFadeThreshold) / (1 - crosshairFadeThreshold));
            hipFireCrosshair?.SetupAlphaValue(crossHairAlphaValue);
        }
        else
        {
            hipFireCrosshair?.SetupAlphaValue(1);
        }
    }

    private void GunPositionADSTransition(float easedAlphaValue)
    {
        if (weaponRoot != null)
        {
            //move gun position
            weaponRoot.localPosition = Vector3.Lerp(gunData.hipFireGunPosition, gunData.ADSGunPosition, easedAlphaValue);
            weaponRoot.localRotation =
                Quaternion.Lerp
                (
                    Quaternion.Euler(gunData.hipFireGunRotationEuler),
                    Quaternion.Euler(gunData.ADSGunRotationEuler),
                    easedAlphaValue
                );
        }
    }

    private void FOVADSTransition()
    {
        //change fov
        mainCam.fieldOfView = Mathf.Lerp(hipFireFOV, adsFOV, adsAlpha);
    }

    private void CrosshairADSTransition()
    {
        //change crosshair
        if (adsAlpha == 1)
        {
            logicBulletStartPosition = aimPoint.position;
            hipFireCrosshair?.SetFollowTarget(aimPoint);
        }
        else
        {
            logicBulletStartPosition = mainCam.transform.position;
            hipFireCrosshair?.SetFollowTarget(null);
        }
    }

    private void UpdateBasicSpread()
    {
        //change basic spread
        currentBasicMinSpreadAngle = Mathf.Lerp(gunData.basicHipFireSpreadAngle, gunData.basicADSSpreadAngle, adsAlpha);
        currentBasicMaxSpreadAngle = Mathf.Lerp(gunData.maxHipFireSpreadAngle, gunData.maxADSSpreadAngle, adsAlpha);

        currentBasicSpreadAngle = Mathf.MoveTowards(currentBasicSpreadAngle, currentBasicMinSpreadAngle, gunData.adsSpreadTransitionSpeed * Time.deltaTime);
        //currentBasicSpreadAngle = Mathf.Lerp(gunData.basicHipFireSpreadAngle, gunData.basicADSSpreadAngle, adsAlpha);
        currentBasicSpreadAngle = Mathf.Clamp(currentBasicSpreadAngle, currentBasicMinSpreadAngle, currentBasicMaxSpreadAngle);
    }

    private void AddShotSpread()
    {
        currentShotSpreadPunishment += gunData.hipFireSpreadPunishmentPerShot;
        currentShotSpreadPunishment = Mathf.Clamp(currentShotSpreadPunishment, 0, gunData.maxHipFireShotPunishment);
    }

    private void UpdateShotSpreadPunishment()
    {
        //ads should have no shot spread
        currentMaxShotSpreadPunishment = Mathf.Lerp(gunData.maxHipFireShotPunishment, 0, adsAlpha);

        //hip fire shot spread recovery
        currentShotSpreadPunishment = Mathf.MoveTowards(currentShotSpreadPunishment, 0, gunData.hipFireShotPunishmentRecoverySpeed * Time.deltaTime);
        currentShotSpreadPunishment = Mathf.Clamp(currentShotSpreadPunishment, 0, currentMaxShotSpreadPunishment);
    }

    private void UpdateAirSpreadPunishment()
    {
        if (playerMovement.movementState == PlayerMovement.MovementState.Air)
        {
            if (!isInADS)
            {
                currentAirSpreadPunishment = Mathf.MoveTowards(currentAirSpreadPunishment, gunData.airHipFireSpreadPunishment, 2f * gunData.airHipFireSpreadTransitionSpeed * Time.deltaTime);
            }
            else
            {
                currentAirSpreadPunishment = Mathf.MoveTowards(currentAirSpreadPunishment, 0, gunData.airHipFireSpreadTransitionSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentAirSpreadPunishment = Mathf.MoveTowards(currentAirSpreadPunishment, 0, gunData.airHipFireSpreadTransitionSpeed * Time.deltaTime);
        }
    }

    private void SyncSpreadWithHipFireCrosshair()
    {
        float spreadRad = currentFinalSpreadAngle * Mathf.Deg2Rad;
        float fovRad = hipFireFOV * Mathf.Deg2Rad;

        float pixelOffset =
            Mathf.Tan(spreadRad) /
            Mathf.Tan(fovRad * 0.5f) *
            (Screen.height * 0.5f);

        float uiOffset = pixelOffset / hudCanvas.scaleFactor;

        hipFireCrosshair?.SetLineTargetOffset(uiOffset);
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
        float rotationGunKickX =
            Random.Range(gunData.rotationGunKickMultiplier_Min, gunData.rotationGunKickMultiplier_Max) *
            gunData.basicRotationGunKick.x;

        float rotationGunKickY =
            sign *
            Random.Range(gunData.rotationGunKickMultiplier_Min, gunData.rotationGunKickMultiplier_Max) *
            gunData.basicRotationGunKick.y;

        float rotationGunKickZ =
            -sign *
            Random.Range(gunData.rotationGunKickMultiplier_Min, gunData.rotationGunKickMultiplier_Max) *
            gunData.basicRotationGunKick.z;


        Vector3 rotationGunKickImpulse = new Vector3(rotationGunKickX, rotationGunKickY, rotationGunKickZ);
        if (isInADS)
        {
            rotationGunKickImpulse *= 0.5f;
        }

        return rotationGunKickImpulse;
    }

    private Vector3 CalculatePositionGunKickImpulse(float sign)
    {
        float positionGunKickX =
            -sign *
            Random.Range(gunData.positionGunKickMultiplier_Min, gunData.positionGunKickMultiplier_Max) *
            Random.Range(0, gunData.basicPositionGunKick.x);

        if (isInADS)
        {
            positionGunKickX *= 0.4f;
        }

        float positionGunKickY = 0 * Random.Range(0f, 0.5f);

        float positionGunKickZ =
            Random.Range(gunData.positionGunKickMultiplier_Min, gunData.positionGunKickMultiplier_Max) *
            gunData.basicPositionGunKick.z;

        Vector3 positionGunKickImpulse = new Vector3(positionGunKickX, positionGunKickY, positionGunKickZ);

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
        Vector2 spreadOffset = Random.insideUnitCircle * currentFinalSpreadAngle;
        Quaternion spread = Quaternion.Euler(spreadOffset.y, spreadOffset.x, 0);

        Vector3 rayDirection = spread * mainCam.transform.forward;

        Ray ray = new Ray(logicBulletStartPosition, rayDirection);
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



    private void UpdateFinalSpreadAngle()
    {
        currentFinalSpreadAngle = currentBasicSpreadAngle + currentShotSpreadPunishment + currentMoveSpeedSpreadPunishment + currentAirSpreadPunishment;
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
