using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettingsDataAction
{
    public void LoadData(SettingsData _data);
    public void SaveData(SettingsData _data);
}
