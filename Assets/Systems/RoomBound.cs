using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBound : CameraBound {

    public static List<RoomBound> Contains(Vector2 position)
        => volumes.FindAll(v => v.rect.Contains(position));

    private void OnEnable() => volumes.Add(this);
    private void OnDisable() => volumes.Remove(this);

    private static readonly List<RoomBound> volumes = new();
}
