using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NectarProjectile : MonoBehaviour
{
    [Header("Nectar")]
    [SerializeField] private float attackKnockback;
    [SerializeField] private float damage;
    [SerializeField] private float gravity;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private new Rigidbody2D rigidbody;

    private void Update()
    {
        Vector2 velocity = rigidbody.velocity;

        velocity.y = Mathf.MoveTowards(velocity.y, -terminalVelocity, gravity * Time.deltaTime);

        rigidbody.velocity = velocity;

    }




}
