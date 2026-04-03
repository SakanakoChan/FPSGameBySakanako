using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPIndicator : MonoBehaviour
{
    [SerializeField] private Image hpIcon;
    [SerializeField] private TextMeshProUGUI hpTMP;
    [SerializeField] private float turnRedThresholdPercentage = 0.25f;

    public void UpdateHPValue(float _hp, float _maxHP)
    {
        hpTMP.text = _hp.ToString("0");

        float percentage = _hp / _maxHP;
        if(percentage <= turnRedThresholdPercentage)
        {
            hpIcon.color = Color.red;
            hpTMP.color = Color.red;
        }
        else
        {
            hpIcon.color = Color.white;
            hpTMP.color = Color.white;
        }
    }
}
