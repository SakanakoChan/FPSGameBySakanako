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

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        imageList = GetComponentsInChildren<Image>(true).ToList();

        gameObject.SetActive(false);
    }

    public void ShowHitMark(Vector2 _localPosition, Color _color, bool _isHeadShot)
    {
        rectTransform.localPosition = _localPosition;
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
