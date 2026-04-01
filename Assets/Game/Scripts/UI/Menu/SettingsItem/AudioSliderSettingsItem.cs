using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSliderSettingsItem : SliderSettingsItem
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string paramName;
    [SerializeField] private float dbRange = 80;

    public override void LoadData()
    {
        base.LoadData();

        if (slider != null)
            AudioManager.instance?.ApplyVolume(paramName, slider.value);
        //ApplyVolume();
    }

    public override void SaveData()
    {
        base.SaveData();

        if (slider != null)
            AudioManager.instance?.ApplyVolume(paramName, slider.value);
        //ApplyVolume();
    }
}
