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
    public List<FireMode> fireModeList;

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