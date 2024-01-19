using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private new BoxCollider2D collider;

    private void Update() {

        // debug helpers

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public class Component : MonoBehaviour {

        [SerializeField] private Player _player;

        protected Player            Player      => _player;
        protected InputManager      Input       => Player.inputManager;
        protected PlayerMovement    Movement    => Player.playerMovement;
        protected Rigidbody2D       Rigidbody   => Player.rigidbody;
        protected BoxCollider2D     Collider    => Player.collider;

        protected Vector2Int InputDirection => new(
            Mathf.RoundToInt(Input.Movement.Vector.x),
            Mathf.RoundToInt(Input.Movement.Vector.y));
    }
}
