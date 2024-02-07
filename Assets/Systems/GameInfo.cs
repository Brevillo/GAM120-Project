using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameInfo {

    public const string GroundLayerName = "Ground";

    public static LayerMask GroundMask = LayerMask.GetMask(GroundLayerName);

    public static int GroundLayer = LayerMask.NameToLayer(GroundLayerName);
}
