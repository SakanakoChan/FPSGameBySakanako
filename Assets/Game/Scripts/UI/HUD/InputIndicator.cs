using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIndicator : MonoBehaviour
{
    [Header("PS5 controller button icons")]
    public Sprite R1;
    public Sprite R2;
    public Sprite R3;
    public Sprite L1;
    public Sprite L2;
    public Sprite L3;

    [Space]
    public Sprite circle;
    public Sprite cross;
    public Sprite square;
    public Sprite triangle;

    [Space]
    public Sprite dpadLeft_PS;
    public Sprite dpadRight_PS;
    public Sprite dpadUp_PS;
    public Sprite dpadDown_PS;

    [Space]
    public Sprite touchPad;
    public Sprite options;
    public Sprite create;


    [Header("XBOX controller button icons")]
    public Sprite RB;
    public Sprite RT;
    public Sprite RS;
    public Sprite LB;
    public Sprite LT;
    public Sprite LS;

    [Space]
    public Sprite B_XBOX;
    public Sprite A_XBOX;
    public Sprite X_XBOX;
    public Sprite Y_XBOX;

    [Space]
    public Sprite dpadLeft_XBOX;
    public Sprite dpadRight_XBOX;
    public Sprite dpadUp_XBOX;
    public Sprite dpadDown_XBOX;

    [Space]
    public Sprite menu;
    public Sprite view;


    [Header("Mouse icons")]
    public Sprite mouseLeft;
    public Sprite mouseRight;
    public Sprite mouseMid;

    [Header("Keyboard icons")]
    public Sprite W;
    public Sprite A;
    public Sprite S;
    public Sprite D;
    public Sprite space;
    public Sprite C;
}
