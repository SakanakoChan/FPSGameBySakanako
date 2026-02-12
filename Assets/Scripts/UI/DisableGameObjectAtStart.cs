using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableGameObjectAtStart : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
