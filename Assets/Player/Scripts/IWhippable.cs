using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWhippable {

    public enum Type { Light, Heavy }

    public Type WhippableType { get; }

    public Vector2 WhippablePosition { get; set; }

    public void DisableMovement();

    public void EnableMovement();
}
