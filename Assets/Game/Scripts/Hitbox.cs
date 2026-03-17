using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitboxType
{
    Head,
    Torso,
    Arm,
    Leg
}

public class Hitbox : MonoBehaviour
{
    public HitboxType hitboxType;
}
