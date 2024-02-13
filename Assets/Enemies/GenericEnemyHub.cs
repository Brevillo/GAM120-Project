using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericEnemyHub : MonoBehaviour {

    [SerializeField] private EntityHealth health;
    [SerializeField] private new Rigidbody2D rigidbody;

    public EntityHealth Health      => health;
    public Rigidbody2D  Rigidbody   => rigidbody;
}
