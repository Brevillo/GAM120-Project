using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // holds all the component references for the player so that each script can just get them from here

    [Header("Behaviours")]
    [SerializeField] private PlayerInput inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWhip playerWhip;
    [SerializeField] private PlayerAttacks playerAttacks;
    [SerializeField] private PlayerHealth playerHealth;

    [SerializeField] private EntityHealth health;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private new Collider2D collider;

    [Header("Helper")]
    [SerializeField] private Transform bodyPivot;

    private int facing; // current direction being faced, 1 = right, -1 = left

    private Vector2Int inputDirection => new(
        Mathf.RoundToInt(inputManager.Movement.Vector.x),
        Mathf.RoundToInt(inputManager.Movement.Vector.y));

    public void Freeze(bool? movement = null, bool? abilities = null, bool? health = null) {

        playerMovement.enabled  = movement  ?? playerMovement.enabled;

        playerWhip.enabled      = abilities ?? playerWhip.enabled;
        playerAttacks.enabled   = abilities ?? playerAttacks.enabled;

        this.health.enabled     = health    ?? this.health.enabled;
        playerHealth.enabled    = health    ?? playerHealth.enabled;
    }

    private void Awake() {
        facing = 1;
    }

    private void Update() {

        if (inputDirection.x != 0) facing = inputDirection.x;

        // debug helpers

        #if UNITY_EDITOR

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (inputManager.Debug2.Down) health.Heal(1);
        //if (inputManager.Debug3.Down) ;
        if (inputManager.Debug4.Down) health.TakeDamage(new(5f, Vector2.zero, Vector2.zero));

        #endif
    }

    public class Component : MonoBehaviour {

        // components can inherit from this to get easy access to all the references as well as some helper properties

        [SerializeField] private Player player;

        // references
        protected Player            Player          => player;
        protected PlayerInput       Input           => player.  inputManager;
        protected PlayerMovement    Movement        => player.  playerMovement;
        protected PlayerWhip        Whip            => player.  playerWhip;
        protected PlayerAttacks     Attacks         => player.  playerAttacks;
        protected PlayerHealth      PlayerHealth    => player.  playerHealth;

        protected EntityHealth      Health          => player.  health;

        protected Rigidbody2D       Rigidbody       => player.  rigidbody;
        protected Collider2D        Collider        => player.  collider;

        protected Transform         BodyPivot       => player.  bodyPivot;

        // helper properties
        protected Vector2Int        InputDirection  => player.inputDirection;
        protected int               Facing          => player.facing;
    }
}
