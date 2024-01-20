using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHornTrigger : MonoBehaviour {

    public event System.Action<EntityHealth> OnEntityCollision;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out EntityHealth entity))
            OnEntityCollision?.Invoke(entity);
    }
}
