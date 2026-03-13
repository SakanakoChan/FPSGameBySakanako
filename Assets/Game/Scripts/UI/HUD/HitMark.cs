using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HitMark : MonoBehaviour
{
    private RectTransform rectTransform;

    private List<Image> imageList;

    [SerializeField] private GameObject pattern_Normal;
    [SerializeField] private GameObject pattern_HeadShot;

    private Gun currentGun;
    private Camera mainCam;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;

        imageList = GetComponentsInChildren<Image>(true).ToList();

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentGun != null && gameObject.activeSelf == true)
        {
            Vector3 screenPosition = mainCam.WorldToScreenPoint(currentGun.logicBulletStartPosition + mainCam.transform.forward);
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, screenPosition, null, out localPosition);
            rectTransform.localPosition = localPosition;
        }
    }

    public void ShowHitMark(Gun _gun, Color _color, bool _isHeadShot)
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        currentGun = _gun;

        foreach (var image in imageList)
        {
            image.color = _color;
        }

        if (_isHeadShot)
        {
            pattern_Normal.SetActive(false);
            pattern_HeadShot.SetActive(true);
        }
        else
        {
            pattern_Normal.SetActive(true);
            pattern_HeadShot.SetActive(false);
        }
    }
}
