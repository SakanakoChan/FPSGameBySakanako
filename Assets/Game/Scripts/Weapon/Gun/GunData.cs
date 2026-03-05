using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum FireMode
{
    FullAuto,
    SemiAuto,
    Burst,
    Single
}

[CreateAssetMenu(fileName = "GunData", menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Basic info")]
    public int damage;
    public float fireRate;
    public int magSize;
    public int reserveAmmo;
    public float adsTime = 0.25f;
    public List<FireMode> fireModeList;

    [Header("Hip fire and ADS position")]
    public Vector3 hipFireGunPosition = new Vector3(0.1773f, -0.2071f, 0.984f);
    public Vector3 hipFireGunRotationEuler = Vector3.zero;

    [Space]
    public Vector3 ADSGunPosition;
    public Vector3 ADSGunRotationEuler;
    public AnimationCurve adsCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.2f, 0.05f),
        new Keyframe(0.5f, 0.6f),
        new Keyframe(1f, 1f)
        );

    [Header("Bullet info")]
    public float bulletFlySpeed;
    public float bulletGravity;
    public float maxRange;
    public GameObject bulletPrefab;

    [Header("Fx info")]
    public ParticleSystem muzzleFlash_Particle;
    public GameObject muzzleFlash_Light;
    public float muzzleFlashLightDuration = 0.05f;

    [Header("Audio info")]
    public AudioClip fireSound;
    public AudioClip fireSound_Empty;


    [Header("Recoil pattern")]
    public float recoilRecoveryDelay = 0.3f;
    public float recoilRecoveryInterval = 0.025f;
    public List<RecoilPattern> recoilPatternList;


    [Header("GunKick info")]
    public Vector3 basicPositionGunKick = new Vector3(0.5f, 0, -0.5f);
    public float positionGunKickMultiplier_Min = 0.8f;
    public float positionGunKickMultiplier_Max = 1.3f;

    [Space]
    public Vector3 basicRotationGunKick = new Vector3(0, 10, 25f);
    public float rotationGunKickMultiplier_Min = 0.8f;
    public float rotationGunKickMultiplier_Max = 1.3f;


    [Header("Camera kick info")]
    public Vector3 basicCameraKick = new Vector3(0, 0, 20);
    public float cameraKickMultiplierRange_Min = 0.8f;
    public float cameraKickMultiplier_Max = 1.3f;


    [Header("Tools")]
    [SerializeField] private float multiplyAllRecoilsBy = 3f;


    [ContextMenu("Multiply all recoils")]
    private void MultiplyAllRecoils()
    {
        foreach (var recoilPattern in recoilPatternList)
        {
            recoilPattern.recoilImpulse *= multiplyAllRecoilsBy;
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}