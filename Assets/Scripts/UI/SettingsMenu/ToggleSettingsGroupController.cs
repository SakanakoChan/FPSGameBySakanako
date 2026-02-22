using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettingsGroupController : MonoBehaviour
{
    [SerializeField] private List<GameObject> childSettings;

    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponentInChildren<Toggle>();
        toggle.onValueChanged.AddListener(ShowChildSettings);

        ShowChildSettings(toggle.isOn);
    }

    public void ShowChildSettings(bool _value)
    {
        foreach (var child in childSettings)
        {
            if (child != null)
                child.SetActive(_value);
        }
    }
}
