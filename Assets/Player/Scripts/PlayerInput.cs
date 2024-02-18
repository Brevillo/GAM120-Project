using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime.Input;

public class PlayerInput : InputManager {

    [Header("Movement")]
    public Button Jump;
    public Axis Movement;

    [Header("Abilities")]
    public Button Headbutt;
    public Button Swing;
    public Button Whip;
    public Button Eat;

    [Header("Menus")]
    public Button Pause;

    [Header("Debug")]
    public Button Debug1;
    public Button Debug2;
    public Button Debug3;
    public Button Debug4;
}
