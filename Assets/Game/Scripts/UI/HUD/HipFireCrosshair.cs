using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipFireCrosshair : MonoBehaviour
{
    private Transform followTarget;

    private RectTransform rectTransform;
    private Camera mainCam;

    [Header("Line offset info")]
    [SerializeField] private RectTransform line_Left;
    [SerializeField] private RectTransform line_Right;
    [SerializeField] private RectTransform line_Up;
    [SerializeField] private RectTransform line_Down;

    [Space]
    [SerializeField] private float basicLineOffset = 10f;

    [Space]
    [SerializeField] private float smoothSpeed = 10f;

    private float targetLineOffset;
    private float currentLineOffset;


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


        //Sync Spread
        currentLineOffset = targetLineOffset;
        //currentLineOffset = Mathf.Lerp(currentLineOffset, targetLineOffset, smoothSpeed * Time.deltaTime);

        line_Left.localPosition = new Vector2(-currentLineOffset, 0);
        line_Right.localPosition = new Vector2(currentLineOffset, 0);
        line_Up.localPosition = new(0, currentLineOffset);
        line_Down.localPosition = new(0, -currentLineOffset);
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


    public void SetLineTargetOffset(float _targetOffset)
    {
        targetLineOffset = _targetOffset + basicLineOffset;
    }
}
