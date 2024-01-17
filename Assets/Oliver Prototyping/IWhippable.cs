using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWhippable {

    public enum Type { Light, Heavy }

    public Type type { get; }

    public Vector2 Position { get; }

    public void DisableMovement();

    public void EnableMovement();

    public void MoveTo(Vector2 position);
}
