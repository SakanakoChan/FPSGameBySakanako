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
    [SerializeField] private bool replaceTextWithActionName = true;

    private void Awake()
    {
        inputIndicator = GetComponentInParent<InputIndicator>();
    }


    public void UpdateInputHint()
    {
        if (replaceTextWithActionName)
            tmp.text = actionName;

        if (inputIndicator != null)
        {
            Sprite sprite = inputIndicator.GetSpriteAccordingToAction(actionName);
            buttonIcon.sprite = sprite;
        }

        if (buttonIcon.sprite == null)
        {
            buttonIcon.enabled = false;
            tmp.enabled = false;
        }
        else
        {
            buttonIcon.enabled = true;
            tmp.enabled = true;
        }
    }
}
