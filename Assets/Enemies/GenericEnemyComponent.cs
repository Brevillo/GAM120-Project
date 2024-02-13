using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericEnemyHub))]
public class GenericEnemyComponent : MonoBehaviour {
    
    private GenericEnemyHub hub;
    private GenericEnemyHub Hub => hub != null ? hub : hub = GetComponent<GenericEnemyHub>();

    #region Hub Extension

    public EntityHealth Health => Hub.Health;
    public Rigidbody2D Rigidbody => Hub.Rigidbody;

    public Vector2 Velocity {
        get => Rigidbody.velocity;
        set => Rigidbody.velocity = value;
    }

    public Vector2 Position => transform.position;

    #endregion
}
