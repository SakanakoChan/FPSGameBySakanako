using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipFireCrosshair : MonoBehaviour
{
    private Transform followTarget;

    private RectTransform rectTransform;
    private Camera mainCam;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            Vector3 screenPosition = mainCam.WorldToScreenPoint(followTarget.position);
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, screenPosition, null, out localPosition);
            rectTransform.localPosition = localPosition;
        }
        else
        {
            rectTransform.localPosition = Vector2.zero;
        }
    }


    /// <summary>
    /// if follow target is null, crosshair will be placed in the center of screen,
    /// otherwise crosshair will follow the target transform
    /// </summary>
    /// <param name="_followTarget"></param>
    public void SetFollowTarget(Transform _followTarget)
    {
        followTarget = _followTarget;
    }
}
