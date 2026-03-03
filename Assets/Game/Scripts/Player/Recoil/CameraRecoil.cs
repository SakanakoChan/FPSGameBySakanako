using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [SerializeField] private float recoilRecoveryDelay = 0.1f;
    private float recoilRecoveryTimer = 0;


    [Header("Spring Settings")]
    [SerializeField] private float springStrength = 120f;
    [SerializeField] private float damping = 18f;

    [HideInInspector] public Vector2 recoilOffset;  // 当前偏移
    private Vector2 recoilVelocity;   // 当前速度


    private void Update()
    {
        //recoilRecoveryTarget += playerLook.currentLookDelta;

        float dt = Time.deltaTime;

        Vector2 targetRecoil = Vector2.zero;

        // 计算弹簧力（拉回0）
        Vector2 springForce = (targetRecoil - recoilOffset) * springStrength;
        if (recoilRecoveryTimer > 0)
        {
            springForce *= 0.15f;
        }

        // 计算阻尼（减速）
        Vector2 dampingForce = -recoilVelocity * damping;

        // 合力
        Vector2 force = springForce + dampingForce;

        // 更新速度
        recoilVelocity += force * dt;

        // 更新位置
        recoilOffset += recoilVelocity * dt;


        recoilRecoveryTimer -= Time.deltaTime;
    }

    public void AddRecoilVelocity(Vector2 _recoilVelocity)
    {
        recoilVelocity += _recoilVelocity;
        recoilRecoveryTimer = recoilRecoveryDelay;
    }


}
