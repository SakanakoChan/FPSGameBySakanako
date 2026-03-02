using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private PlayerLook playerLook;

    [SerializeField] private float recoilRecoveryDelay = 0.1f;
    private float recoilRecoveryTimer = 0;

    private Vector2 currentRecoil;
    private Vector2 targetRecoil;

    [Header("Spring Settings")]
    [SerializeField] private float springStrength = 120f;
    [SerializeField] private float damping = 18f;

    public Vector2 recoilOffset { get; private set; }   // 当前偏移
    private Vector2 recoilVelocity;   // 当前速度

    private Vector2 recoilBaseView;

    private void Start()
    {
        playerLook = GetComponentInParent<PlayerLook>();
        //recoilRecoveryTarget = Vector2.zero;
    }


    private void Update()
    {
        //recoilRecoveryTarget += playerLook.currentLookDelta;

        float dt = Time.deltaTime;

        Vector2 targetRecoil = Vector2.zero;
        //if (recoilRecoveryTimer <= 0)
        //{
        //    Vector2 currentView = new Vector2(playerLook.yaw, playerLook.pitch);
        //    Vector2 deltaView = currentView - recoilBaseView;

        //    Vector2 playerInputCompensation = -deltaView;
        //    targetRecoil = new Vector2(
        //            Mathf.Clamp(playerInputCompensation.x, 0, recoilOffset.x),
        //            Mathf.Clamp(playerInputCompensation.y, 0, recoilOffset.y)
        //        );
        //}

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

        Vector2 playerInputOffset = Vector2.zero;
        if (Mathf.Sign(playerLook.currentLookDelta.x) == Mathf.Sign(-recoilOffset.x) && recoilOffset.x > 0)
        {
            playerInputOffset.x = playerLook.currentLookDelta.x;
        }

        if (Mathf.Sign(playerLook.currentLookDelta.y) == Mathf.Sign(-recoilOffset.y) && recoilOffset.y > 0)
        {
            playerInputOffset.y = playerLook.currentLookDelta.y;
        }

        recoilOffset += playerInputOffset;

        float x, y;
        x = recoilOffset.x < 0 ? 0 : recoilOffset.x;
        y = recoilOffset.y < 0 ? 0 : recoilOffset.y;

        recoilOffset = new Vector2(x, y);

        //transform.localRotation =
        //    Quaternion.Euler(-recoilOffset.y, recoilOffset.x, 0f);



        transform.localRotation = Quaternion.AngleAxis(-recoilOffset.y, Vector3.right) * Quaternion.AngleAxis(recoilOffset.x, Vector3.up);

        recoilRecoveryTimer -= Time.deltaTime;


        //currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, 10f * Time.deltaTime);
        //transform.localRotation = Quaternion.Euler(-currentRecoil.y, currentRecoil.x, 0);

        //if (recoilRecoveryTimer < 0)
        //{
        //    targetRecoil = Vector2.Lerp(targetRecoil, Vector2.zero, 5f * Time.deltaTime);
        //}
    }

    public void AddRecoilVelocity(Vector2 _recoilVelocity)
    {
        recoilVelocity += _recoilVelocity;
        recoilRecoveryTimer = recoilRecoveryDelay;

        recoilBaseView = new Vector2(playerLook.yaw, playerLook.pitch);

        //targetRecoil += _recoilVelocity;
        //recoilRecoveryTimer = recoilRecoveryDelay;
    }

}
