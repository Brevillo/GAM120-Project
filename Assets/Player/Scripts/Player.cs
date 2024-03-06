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

    public PlayerInput Input => inputManager;

    private int facing;             // current direction being faced, 1 = right, -1 = left
    private int crawlOrientation;   // current orientation of the player, 1 if right side up, -1 if upside down

    private Vector2Int inputDirection => new(
        Mathf.RoundToInt(inputManager.Movement.Vector.x),
        Mathf.RoundToInt(inputManager.Movement.Vector.y));

    public void Freeze(bool? movement = null, bool? abilities = null, bool? health = null, bool? input = null) {

        playerMovement.enabled  = !movement     ?? playerMovement.enabled;

        playerWhip.enabled      = !abilities    ?? playerWhip.enabled;
        playerAttacks.enabled   = !abilities    ?? playerAttacks.enabled;

        this.health.enabled     = !health       ?? this.health.enabled;
        playerHealth.enabled    = !health       ?? playerHealth.enabled;

        inputManager.enabled    = !input        ?? inputManager.enabled;
    }

    private void ResetCrawlOrientation() => crawlOrientation = 1;

    private void Awake() {
        facing = 1;
        crawlOrientation = 1;
    }

    private void Update() {

        bool xInput = inputDirection.x != 0;

        if (!xInput) ResetCrawlOrientation();
        if (xInput) facing = inputDirection.x * crawlOrientation;

        // debug helpers

        #if UNITY_EDITOR

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (inputManager.Debug2.Down) playerHealth.DebugSetEnergy(1);
        if (inputManager.Debug3.Down) playerHealth.DebugSetEnergy(0f);
        if (inputManager.Debug4.Down) health.TakeDamage(new(1f, Vector2.down, Vector2.zero));

        #endif
    }

    public class Component : MonoBehaviour {

        // components can inherit from this to get easy access to all the references as well as some helper properties

        [SerializeField] private Player player;

        // references
        protected Player            Player              => player;
        protected PlayerInput       Input               => player.  inputManager;
        protected PlayerMovement    Movement            => player.  playerMovement;
        protected PlayerWhip        Whip                => player.  playerWhip;
        protected PlayerAttacks     Attacks             => player.  playerAttacks;
        protected PlayerHealth      PlayerHealth        => player.  playerHealth;

        protected EntityHealth      Health              => player.  health;

        protected Rigidbody2D       Rigidbody           => player.  rigidbody;
        protected Collider2D        Collider            => player.  collider;

        protected Transform         BodyPivot           => player.  bodyPivot;

        // helper properties
        protected Vector2Int        InputDirection      => player.inputDirection;
        protected int               Facing              => player.facing;
        protected int               CrawlOrientation     => player.crawlOrientation;

        protected void ResetCrawlOrientation() => player.ResetCrawlOrientation();
    }
}
