using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerColorProfile : ScriptableObject {

    public Color HeadColor, AbdomenColor, ThoraxColor, WingColor, WingCasingColor, LegColor;

    private void Reset() {

        HeadColor       = Color.white;
        AbdomenColor    = Color.white;
        ThoraxColor     = Color.white;
        WingColor       = Color.white;
        WingCasingColor = Color.white;
        LegColor        = Color.white;
    }
}
