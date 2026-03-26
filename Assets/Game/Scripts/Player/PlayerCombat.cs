using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCombat : MonoBehaviour
{
    private Weapon currentWeapon;

    [Header("Basic info")]
    public CameraKick cameraKick;
    private GunKick gunKick;
    private PlayerMovement playerMovement;

    [Space]
    public Animator armsAnim;
    public Animator cameraTiltAnim;

    [Header("HUD info")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private HipFireCrosshair hipFireCrosshair;
    [SerializeField] private HitMark hitMark;
    [SerializeField] private WeaponInfoIndicator weaponInfoIndicator;

    [Header("IK info")]
    [SerializeField] private TwoBoneIKConstraint leftHandConstraint;
    [SerializeField] private TwoBoneIKConstraint rightHandConstraint;

    [Header("Audio info")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip killSound_Normal;
    [SerializeField] private AudioClip killSound_HeadShot;

    public bool isInADS
    {
        get
        {
            if (currentWeapon != null)
            {
                return currentWeapon.isInADS;
            }

            return false;
        }
    }
    public bool isTryingToFire { get; private set; } = false;
    public bool isReloading
    {
        get
        {
            if (currentWeapon != null)
            {
                return currentWeapon.isReloading;
            }

            return false;
        }
    }

    private bool pause = false;

    private void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();
        playerMovement = GetComponent<PlayerMovement>();

        //cameraKick = GetComponentInChildren<CameraKick>();
        gunKick = GetComponentInChildren<GunKick>();

        if (currentWeapon != null)
            weaponInfoIndicator?.UpdateAmmoInfo(currentWeapon.GetCurrentAmmoInMagzine(), currentWeapon.GetReserveAmmo());
    }

    private void Update()
    {
        SyncSpreadWithHipFireCrosshair();

        isTryingToFire = false;
        if (InputManager.instance.FireHeld)
        {
            if (currentWeapon != null)
            {
                isTryingToFire = true;
                if (playerMovement.isSprinting)
                {
                    return;
                }

                bool fireSucceeded = currentWeapon.TryFire();
                if (fireSucceeded)
                {
                    //play arm fire animation to add animated gunkick
                    //(to combine with code driven gunkick)
                    if (isInADS == false)
                        armsAnim.Play("Fire", 2, 0);

                    weaponInfoIndicator?.UpdateAmmoInfo(currentWeapon.GetCurrentAmmoInMagzine(), currentWeapon.GetReserveAmmo());
                }
            }

        }

        if (InputManager.instance.ReloadPressed)
        {
            if (currentWeapon.TryReload() == true)
            {
                if (isInADS)
                {
                    CancelADS();
                }

                PlayArmReloadAnimation();
                //currentWeapon?.PlayReloadAnimation();

                if (currentWeapon.CheckIfCurrentMagIsEmpty())
                {
                    cameraTiltAnim?.Play("Reload_Empty");
                }
                else
                {
                    cameraTiltAnim?.Play("Reload");
                }
                MakeLeftHandHoldMag();
            }
        }


        if (!currentWeapon.isReloading)
        {
            if (InputManager.instance.AimDownSightHeld)
            {
                currentWeapon?.EnterADS();
                armsAnim.SetBool("Aim", true);
                //isInADS = true;
            }
            else
            {
                CancelADS();
            }
        }


        armsAnim.SetFloat("Aiming", currentWeapon.GetADSAlpha());


        //for test only
        if (Input.GetKeyDown(KeyCode.P))
        {
            pause = !pause;
            armsAnim.speed = pause ? 0 : 1;
        }

    }


    public void CancelADS()
    {
        currentWeapon?.ExitADS();
        armsAnim.SetBool("Aim", false);
        //isInADS = false;
    }

    private void PlayArmReloadAnimation()
    {
        if (currentWeapon.CheckIfCurrentMagIsEmpty() == false)
        {
            armsAnim.Play("Reload", 3, 0);
        }
        else
        {
            armsAnim.Play("Reload_Empty", 3, 0);
        }
    }

    //public void OnEjectCasing()
    //{
    //    currentWeapon?.EjectCasing();
    //}

    public void OnMagReleased()
    {
        cameraKick?.AddCameraKick(new Vector3(0, 0, 30f));
        gunKick?.AddGunKick(Vector3.zero, new Vector3(Random.Range(-20f, -10f), Random.Range(5f, 10f), Random.Range(-240f, -180f)));
    }

    public void OnMagInserted_EmptyReloading()
    {
        cameraKick?.AddCameraKick(new Vector3(0, 0, 40f));
        gunKick?.AddGunKick(Vector3.zero, new Vector3(Random.Range(-20f, -10f), Random.Range(5f, 10f), Random.Range(-240f, -180f)));
    }

    public void FillMag()
    {
        currentWeapon?.FillMag();
        weaponInfoIndicator?.UpdateAmmoInfo(currentWeapon.GetCurrentAmmoInMagzine(), currentWeapon.GetReserveAmmo());

        cameraKick?.AddCameraKick(new Vector3(0, 0, 50f));
        gunKick?.AddGunKick(Vector3.zero, new Vector3(Random.Range(-20f, -10f), Random.Range(5f, 10f), Random.Range(-240f, -180f)));
    }

    public void OnStockAgainstArm()
    {
        cameraKick?.AddCameraKick(new Vector3(0, 0, 20f));
    }


    #region Reload animation IK
    public void MakeLeftHandHoldMag()
    {
        currentWeapon?.MakeLeftHandHoldMag();
    }

    public void MakeLeftHandReturnToNormalPosition()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(leftHandConstraint, 0.1f, 0.2f, 0.1f, 1, 0));
        currentWeapon.MakeLeftHandReturnToNormalPosition();
    }


    public void MoveLeftHandToEmptyReloadingInsertMagPosition()
    {
        currentWeapon?.MoveLeftHandToEmptyReloadingInsertMagPosition();
    }

    public void MoveLeftHandToEmptyReloadingMidPoint()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(leftHandConstraint, 0.1f, 0.1f, 0.1f, 1, 0.3f));
        currentWeapon?.MoveLeftHandToEmptyReloadingMidPoint();
    }


    public void MakeRightHandGrabBolt()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(rightHandConstraint, 0.1f, 0.1f, 0.1f, 1, 0));
        currentWeapon?.MakeRightHandGrabBolt();
    }


    public void MakeRightHandReturnToNormalPosition()
    {
        StartCoroutine(TemporarilyReleaseHandConstraint(rightHandConstraint, 0.2f, 0.1f, 0.2f, 1, 0));
        currentWeapon?.MakeRightHandReturnToNormalPosition();
    }



    private IEnumerator TemporarilyReleaseHandConstraint(TwoBoneIKConstraint _handConstraint,
        float _fadeOutTime, float _pauseTime, float _fadeInTime, float _highestWeight, float _lowestWeight)
    {
        yield return GraduallyChangeIKWeight(_handConstraint, _highestWeight, _lowestWeight, _fadeOutTime);
        yield return new WaitForSeconds(_pauseTime);
        yield return GraduallyChangeIKWeight(_handConstraint, _lowestWeight, _highestWeight, _fadeInTime);
    }


    private IEnumerator GraduallyChangeIKWeight(TwoBoneIKConstraint _handIKConstraint, float _from, float _to, float _duration)
    {
        float duration = _duration;
        float progress = 0;
        float timer = 0;

        float IKWeight = _from;
        _handIKConstraint.weight = IKWeight;

        while (timer < duration)
        {
            progress = timer / duration;
            progress = Mathf.SmoothStep(0, 1, progress);

            _handIKConstraint.weight = Mathf.Lerp(_from, _to, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        IKWeight = _to;
        _handIKConstraint.weight = IKWeight;
    }
    #endregion

    public void FinishReloading()
    {
        currentWeapon?.FinishReloading();
    }

    private void SyncSpreadWithHipFireCrosshair()
    {
        Gun gun = currentWeapon as Gun;

        float spreadRad = gun.currentFinalSpreadAngle * Mathf.Deg2Rad;
        float fovRad = gun.hipFireFOV * Mathf.Deg2Rad;

        float pixelOffset =
            Mathf.Tan(spreadRad) /
            Mathf.Tan(fovRad * 0.5f) *
            (Screen.height * 0.5f);

        float uiOffset = pixelOffset / hudCanvas.scaleFactor;

        hipFireCrosshair?.SetLineTargetOffset(uiOffset);
    }

    public void ShowHitFeedback(Color _hitMarkColor, bool _isHeadShot, bool _thisHitMakesKill)
    {
        hitMark?.ShowHitMark(currentWeapon as Gun, _hitMarkColor, _isHeadShot);

        if (_thisHitMakesKill)
        {
            if (_isHeadShot)
            {
                AudioManager.instance?.PlaySound(killSound_HeadShot, transform.position);
            }
            else
            {
                AudioManager.instance?.PlaySound(killSound_Normal, transform.position);
            }
        }
        else
        {
            AudioManager.instance?.PlaySound(hitSound, transform.position);
        }
    }

    public void StartSprintToFireDelay()
    {
        currentWeapon?.StartSprintToFireDelay();
    }

}
