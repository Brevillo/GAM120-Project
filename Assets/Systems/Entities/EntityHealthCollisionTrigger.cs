using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EntityHealthCollisionTrigger : MonoBehaviour {

    public UnityEvent<EntityHealth> OnEntityCollision;
    public UnityEvent<GameObject> OnNonEntityCollision;

    private void OnTriggerEnter2D(Collider2D collision) => CheckCollision(collision.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => CheckCollision(collision.gameObject);

    private void CheckCollision(GameObject go) {

        if (go.TryGetComponent(out EntityHealth entity))
            OnEntityCollision.Invoke(entity);

        else OnNonEntityCollision.Invoke(go);
    }
}
