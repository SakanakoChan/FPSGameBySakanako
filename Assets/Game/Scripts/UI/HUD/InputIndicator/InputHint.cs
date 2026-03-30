using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputHint : MonoBehaviour
{
    private InputIndicator inputIndicator;

    [Header("Basic info")]
    [SerializeField] private Image buttonIcon;
    [SerializeField] private TextMeshProUGUI tmp;

    [Space]
    [SerializeField] private string actionName;

    private void Start()
    {
        inputIndicator = GetComponentInParent<InputIndicator>();
    }

    private void Update()
    {
        tmp.text = actionName;

        if (inputIndicator != null)
        {
            Sprite sprite = inputIndicator.GetSpriteAccordingToAction(actionName);
            buttonIcon.sprite = sprite;
        }
    }
}
