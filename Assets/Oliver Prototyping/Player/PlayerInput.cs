using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputManager {

    [Header("Movement")]
    public Button Jump;
    public Axis Movement;
    public Button Whip;
    public Button Attack;

    [Header("Debug")]
    public Button Debug1;
    public Button Debug2;
    public Button Debug3;
    public Button Debug4;
}
