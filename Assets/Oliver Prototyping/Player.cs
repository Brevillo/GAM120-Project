using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private new BoxCollider2D collider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int facing; // current direction being faced, 1 = right, -1 = left

    private void Awake() {
        facing = 1;
    }

    private void Update() {

        if (inputDirection.x != 0) facing = inputDirection.x;

        // debug helpers

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private Vector2Int inputDirection => new(
        Mathf.RoundToInt(inputManager.Movement.Vector.x),
        Mathf.RoundToInt(inputManager.Movement.Vector.y));

    public class Component : MonoBehaviour {

        [SerializeField] private Player _player;

        // references
        protected Player            Player          => _player;
        protected InputManager      Input           => Player.  inputManager;
        protected PlayerMovement    Movement        => Player.  playerMovement;
        protected Rigidbody2D       Rigidbody       => Player.  rigidbody;
        protected BoxCollider2D     Collider        => Player.  collider;
        protected SpriteRenderer    Renderer        => Player.  spriteRenderer;

        // helper properties
        protected Vector2Int        InputDirection  => Player.inputDirection;
        protected int               Facing          => Player.facing;
    }
}
