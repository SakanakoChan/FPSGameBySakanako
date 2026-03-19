using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageDirectionHint : MonoBehaviour
{
    private Vector3 damageDirection;

    [Header("Auto fade info")]
    [SerializeField] private float fadeDelay = 3f;
    [SerializeField] private float fadeDuration = 1f;
    private Image image;
    private float originalAlphaValue;

    private Transform playerTransform;
    private Camera mainCam;

    private void Awake()
    {
        image = GetComponentInChildren<Image>();
        originalAlphaValue = image.color.a;

        playerTransform = PlayerReference.playerTransform;
        mainCam = Camera.main;
    }

    private void Update()
    {
        FollowDamageDirection();
    }

    private void FollowDamageDirection()
    {
        if (damageDirection == null)
            return;

        Vector3 direction = -damageDirection;
        direction.y = 0;
        direction = direction.normalized;

        Vector3 localDirection = mainCam.transform.InverseTransformDirection(direction);
        Vector2 direction2D = new Vector2(localDirection.x, localDirection.z).normalized;

        transform.up = direction2D;
    }

    public void SetupDamageDirectionHint(Vector3 _damageDirection)
    {
        damageDirection = _damageDirection;
        image.color = image.color = new Color(image.color.r, image.color.g, image.color.b, originalAlphaValue);

        FollowDamageDirection();

        StartCoroutine(AutoFade());
    }

    private IEnumerator AutoFade()
    {
        yield return new WaitForSeconds(fadeDelay);

        float startAlphaValue = originalAlphaValue;
        float endAlphaValue = 0;

        float timer = 0;
        float progress = 0;

        while (timer < fadeDuration)
        {
            progress = timer / fadeDuration;
            progress = Mathf.SmoothStep(0, 1, progress);

            float alphaValue = Mathf.Lerp(startAlphaValue, endAlphaValue, progress);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alphaValue);

            timer += Time.deltaTime;
            yield return null;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, endAlphaValue);
    }
}
