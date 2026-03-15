using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerReference
{
    public static Transform playerTransform { get; private set; }

    public static void SetPlayerTrasnform(Transform _playerTrasnform)
    {
        playerTransform = _playerTrasnform;
    }
}
