using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhipTrigger : MonoBehaviour {

    [SerializeField] private Rigidbody2D rigidbody;

    public event System.Action<Collider2D> OnCollision;

    private void OnTriggerEnter2D(Collider2D collision) {

        OnCollision?.Invoke(collision);
    }

    public void MoveTo(Vector2 position) => rigidbody.MovePosition(position);
}
