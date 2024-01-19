using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // holds all the component references for the player so that each script can just get them from here

    [SerializeField] private PlayerInput inputManager;
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
        if (inputManager.Debug2.Down) ;
        if (inputManager.Debug3.Down) ;
        if (inputManager.Debug4.Down) ;
    }

    private Vector2Int inputDirection => new(
        Mathf.RoundToInt(inputManager.Movement.Vector.x),
        Mathf.RoundToInt(inputManager.Movement.Vector.y));

    public class Component : MonoBehaviour {

        // components can inherit from this to get easy access to all the references as well as some helper properties

        [SerializeField] private Player player;

        // references
        protected Player            Player          => player;
        protected PlayerInput       Input           => player.  inputManager;
        protected PlayerMovement    Movement        => player.  playerMovement;
        protected Rigidbody2D       Rigidbody       => player.  rigidbody;
        protected BoxCollider2D     Collider        => player.  collider;
        protected SpriteRenderer    Renderer        => player.  spriteRenderer;

        // helper properties
        protected Vector2Int        InputDirection  => player.inputDirection;
        protected int               Facing          => player.facing;
    }
}
