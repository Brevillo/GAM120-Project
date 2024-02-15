using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public readonly struct EntityHealthCollision {

    public EntityHealthCollision(EntityHealth entity, Collider2D collider) {
        this.entity     = entity;
        this.collider   = collider;
        this.gameObject = collider.gameObject;
    }

    public readonly EntityHealth entity;
    public readonly Collider2D collider;
    public readonly GameObject gameObject;
}

public class EntityHealthCollisionTrigger : MonoBehaviour {

    public UnityEvent<EntityHealthCollision> OnEntityCollision;
    public UnityEvent<Collider2D> OnNonEntityCollision;

    private void OnTriggerEnter2D(Collider2D collision)    => CheckCollisionForEntity(collision);
    private void OnCollisionEnter2D(Collision2D collision) => CheckCollisionForEntity(collision.collider);

    private void CheckCollisionForEntity(Collider2D collider) {

        if (collider.TryGetComponent(out EntityHealth entity))
            OnEntityCollision.Invoke(new(entity, collider));

        else OnNonEntityCollision.Invoke(collider);
    }
}
