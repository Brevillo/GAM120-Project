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
    [SerializeField] private EntityHealthTeam team;


    private void Start()
    {
        Destroy(gameObject, 10);
    }
    private void Update()
    {
        Vector2 velocity = rigidbody.velocity;

        velocity.y = Mathf.MoveTowards(velocity.y, -terminalVelocity, gravity * Time.deltaTime);

        rigidbody.velocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EntityHealth entity) && entity.Team != team)
        {
            entity.TakeDamage(new DamageInfo(damage, rigidbody.velocity, rigidbody.velocity.normalized * attackKnockback));
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == GameInfo.GroundLayer)
        {
            Destroy(gameObject);
        }
    }
}
