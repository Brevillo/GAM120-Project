using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour {

    // holds all the component references for the player so that each script can just get them from here

    [Header("Behaviours")]
    [SerializeField] private PlayerInput inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWhip playerWhip;
    [SerializeField] private PlayerAttacks playerAttacks;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerColor playerColor;
    [SerializeField] private PlayerBodyAnimation bodyAnimation;

    [SerializeField] private EntityHealth health;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private new Collider2D collider;
    [SerializeField] private PlayerColorProfileReference colorProfileReference;

    [Header("Helper")]
    [SerializeField] private Transform bodyPivot;

    public PlayerInput Input => inputManager;


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

    public void Respawn() {

        foreach (var resetable in FindObjectsOfType<GameObject>(true)
            .Select(go => go.GetComponent<IResetable>())
            .Where(resetable => resetable != null))
            resetable.ResetableReset();

        foreach (var component in GetComponentsInChildren<Component>())
            component.Respawn();
    }

    private void Start() {
        playerColor.SetProfile(colorProfileReference.PlayerColorProfile);
    }

    private void Update() {

        // debug helpers

        #if UNITY_EDITOR

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (inputManager.Debug2.Down) health.TakeDamage(new(1, Vector2.up, Vector2.zero));
        if (inputManager.Debug3.Down) health.TakeDamage(new(99999, Vector2.up, Vector2.zero));
        //if (inputManager.Debug4.Down) ;

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
        protected PlayerBodyAnimation BodyAnimation     => player.  bodyAnimation;

        protected EntityHealth      Health              => player.  health;

        protected Rigidbody2D       Rigidbody           => player.  rigidbody;
        protected Collider2D        Collider            => player.  collider;

        protected Transform         BodyPivot           => player.  bodyPivot;

        // helper properties
        protected Vector2Int        InputDirection      => player.inputDirection;
        protected int               Facing              => Movement.Facing;
        protected int               CrawlOrientation    => Movement.CrawlOrientation;

        public virtual void Respawn() { }
    }
}
