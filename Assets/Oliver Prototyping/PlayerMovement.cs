using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel, groundDeccel, airAccel, airDeccel, jumpHeight, jumpGravity, fallGravity, groundDetectDist;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private KeyCode jumpKey;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D col;

    private StateMachine<PlayerMovement> stateMachine;

    private RaycastHit2D groundHit;
    private bool onGround;
    private Vector2 velocity;
    private Vector2Int input;

    private Grounded grounded;
    private Jumping jumping;
    private Falling falling;

    private void Awake() {

        grounded = new(this);
        jumping = new(this);
        falling = new(this);

        TransitionDelegate

            toGrounded = () => onGround,

            startJump = () => Input.GetKeyDown(jumpKey) && onGround,
            endJump = () => !Input.GetKey(jumpKey) || velocity.y <= 0,

            toFalling = () => !onGround;

        stateMachine = new(grounded, new() {

            { grounded, new() {
                new(jumping, startJump),
                new(falling, toFalling),
            } },

            { jumping, new() {
                new(falling, endJump),
            } },

            { falling, new() {
                new(grounded, toGrounded),
            } },
        });
    }

    private void Update() {

        input = new(
            Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")),
            Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));

        velocity = rb.velocity;

        groundHit = Physics2D.BoxCast(transform.position, col.size, 0, Vector2.down, groundDetectDist, groundMask);
        onGround = groundHit && groundHit.normal == Vector2.up;

        stateMachine.Update(Time.deltaTime);

        rb.velocity = velocity;
    }

    private void Run() {

        float accel = onGround
            ? input.x != 0 ? groundAccel : groundDeccel
            : input.x != 0 ? airAccel    : airDeccel;

        velocity.x = Mathf.MoveTowards(velocity.x, input.x * runSpeed, accel * Time.deltaTime);
    }

    private class State : State<PlayerMovement> {
        public State(PlayerMovement context) : base(context) { }
    }

    private class SubState : SubState<PlayerMovement, State> {
        public SubState(PlayerMovement context, State superState) : base(context, superState) { }
    }

    private class Grounded : State {

        public Grounded(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.Run();

            base.Update();
        }
    }

    private class Jumping : State {

        public Jumping(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.velocity.y = Mathf.Sqrt(context.jumpHeight * context.jumpGravity * 2f);
        }

        public override void Update() {

            context.velocity += Vector2.down * context.jumpGravity * Time.deltaTime;

            context.Run();

            base.Update();
        }
    }

    private class Falling : State {

        public Falling(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.velocity.y = 0;
        }

        public override void Update() {

            context.velocity += Vector2.down * context.fallGravity * Time.deltaTime;

            context.Run();

            base.Update();
        }
    }
}
