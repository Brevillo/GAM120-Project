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

    public new bool enabled {
        get => _enabled;
        set => ((_enabled = collider.enabled = value) ? OnEnable : OnDisable).Invoke();
    }

    [SerializeField] private bool _enabled = true;

    private new Collider2D collider;

    private void Awake() {
        collider = GetComponent<Collider2D>();
    }

    public UnityEvent<EntityHealthCollision> OnEntityEnter, OnEntityStay;
    public UnityEvent<Collider2D> OnNonEntityEnter, OnNonEntityStay;
    public UnityEvent OnEnable, OnDisable;

    private void OnTriggerEnter2D(Collider2D collider)      => CheckCollisionForEntity(collider,            OnEntityEnter,  OnNonEntityEnter);
    private void OnCollisionEnter2D(Collision2D collision)  => CheckCollisionForEntity(collision.collider,  OnEntityEnter,  OnNonEntityEnter);
    private void OnTriggerStay2D(Collider2D collider)       => CheckCollisionForEntity(collider,            OnEntityStay,   OnNonEntityStay);
    private void OnCollisionStay2D(Collision2D collision)   => CheckCollisionForEntity(collision.collider,  OnEntityStay,   OnNonEntityStay);

    private void CheckCollisionForEntity(Collider2D collider, UnityEvent<EntityHealthCollision> onEntity, UnityEvent<Collider2D> onNonEntity) {

        if (!enabled) return;

        if (collider.TryGetComponent(out EntityHealth entity))
            onEntity.Invoke(new(entity, collider));

        else onNonEntity.Invoke(collider);
    }
}
