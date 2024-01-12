using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

public class PlayerMovement : MonoBehaviour {

    #region Parameters

    [Header("Running")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel, groundDeccel, airAccel, airDeccel;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpGravity, fallGravity, peakVelThreshold, peakGravity, maxFallSpeed, groundDetectDist;
    [SerializeField] private BufferTimer jumpBuffer;
    [SerializeField] private LayerMask groundMask;

    [Header("Flying")]
    [SerializeField] private float flightForce;
    [SerializeField] private float minStartFlightVel, maxFlightVel, maxFlightStamina, timeAfterJumpingBeforeFlight;
    [SerializeField] private Transform wing1, wing2;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private InputManager inputManager;

    #endregion

    #region Variables

    [Header("State Machine Debug"), SerializeField] private StateMachine<PlayerMovement> stateMachine;

    // instances of each state class
    private Grounded    grounded;
    private Jumping     jumping;
    private Falling     falling;
    private Flying      flying;

    private Vector2Int inputDir;               // current movement input
    private bool jumpBuffered;              // is jump buffered?

    private Vector2 velocity;               // current velocity (stored so that I can edit the x and y components individually)
    private RaycastHit2D groundHit;         // raycast hit for the ground
    private bool onGround;                  // is the player on the ground?

    private float remainingFlightStamina;   // how much flight stamina reminas

    #endregion

    private void Awake() {
        InitializeStateMachine();
    }

    private void Update() {

        // input

        inputDir = new(
            Mathf.RoundToInt(inputManager.Movement.Vector.x),
            Mathf.RoundToInt(inputManager.Movement.Vector.y));
        jumpBuffered = jumpBuffer.Buffer(inputManager.Jump.Down);

        // get information about current physical state

        velocity = rigidbody.velocity;
        groundHit = Physics2D.BoxCast(transform.position, col.size, 0, Vector2.down, groundDetectDist, groundMask);
        onGround = groundHit && groundHit.normal == Vector2.up;

        // state machine

        stateMachine.Update(Time.deltaTime);

        // apply velocity

        rigidbody.velocity = velocity;
    }

    private void Run() {

        float accel = onGround
            ? inputDir.x != 0 ? groundAccel : groundDeccel
            : inputDir.x != 0 ? airAccel    : airDeccel;

        velocity.x = Mathf.MoveTowards(velocity.x, inputDir.x * runSpeed, accel * Time.deltaTime);
    }

    private void Fall(float gravity) {

        gravity = Mathf.Abs(velocity.y) < peakVelThreshold ? peakGravity : gravity;

        velocity.y = Mathf.MoveTowards(velocity.y, -maxFallSpeed, gravity * Time.deltaTime);
    }

    #region State Machine

    #region Helper Classes

    private class State : State<PlayerMovement> {
        public State(PlayerMovement context) : base(context) { }
    }

    private class SubState : SubState<PlayerMovement, State> {
        public SubState(PlayerMovement context, State superState) : base(context, superState) { }
    }

    #endregion

    private void InitializeStateMachine() {

        // initialize states
        grounded    = new(this);
        jumping     = new(this);
        falling     = new(this);
        flying      = new(this);

        // define transition requirements
        TransitionDelegate

            toGrounded  = () => onGround,
            toFlight    = () => inputManager.Jump.Pressed && remainingFlightStamina > 0 && !onGround && (stateMachine.previousState != jumping || stateMachine.stateDuration > timeAfterJumpingBeforeFlight),

            startJump   = () => jumpBuffered && onGround,
            endJump     = () => !inputManager.Jump.Pressed || velocity.y <= 0,

            toFalling   = () => !onGround,

            endFlying   = () => !inputManager.Jump.Pressed || remainingFlightStamina <= 0;

        // initialize state machine
        stateMachine = new(

            firstState: grounded,

            // define transitions
            new() {

                /* example

                defines all the transitions that can exit out of "fromState"

                in this case, there is only one transition, which
                will go to "toState" when "transitionRequirement" evaluates to true

                { fromState, new() {
                    new(toState, transitionRequirement),
                } },

                 */

                { grounded, new() {
                    new(jumping, startJump),
                    new(falling, toFalling),
                } },

                { jumping, new() {
                    new(falling, endJump),
                } },

                { falling, new() {
                    new(grounded, toGrounded),
                    new(flying, toFlight),
                } },

                { flying, new() {
                    new(falling, endFlying),
                } },
            }
        );
    }

    /* Notes

    - the "context" variable used below refers to the playerMovement script, and is
      used to reference parameters and variables that are stored on it.
      I can't just reference them normally because they are inside
      the state classes, which are separate from the playerMovement class.

     */

    [System.Serializable]
    private class Grounded : State {

        public Grounded(PlayerMovement context) : base(context) { }

        public override void Enter() {

            context.remainingFlightStamina = context.maxFlightStamina;

            base.Enter();
        }

        public override void Update() {

            context.Run();

            base.Update();
        }
    }

    [System.Serializable]
    private class Jumping : State {

        public Jumping(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.jumpBuffer.Reset();
            context.velocity.y = Mathf.Sqrt(context.jumpHeight * context.jumpGravity * 2f);
        }

        public override void Update() {

            context.Fall(context.jumpGravity);

            context.Run();

            base.Update();
        }

        public override void Exit() {

            context.velocity.y = 0;

            base.Exit();
        }
    }

    [System.Serializable]
    private class Flying : State {

        public Flying(PlayerMovement context) : base(context) { }

        private int wingOscillation;

        public override void Enter() {

            base.Enter();

            context.velocity.y = Mathf.Max(context.velocity.y, -context.minStartFlightVel);

            wingOscillation = 1;
        }

        public override void Update() {

            context.remainingFlightStamina -= Time.deltaTime;
            context.velocity.y = Mathf.MoveTowards(context.velocity.y, context.maxFlightVel, context.flightForce * Time.deltaTime);

            wingOscillation *= -1;
            context.wing1.localEulerAngles = Vector3.forward * 15 * wingOscillation;
            context.wing2.localEulerAngles = Vector3.forward * 15 * -wingOscillation;

            context.Run();

            base.Update();
        }

        public override void Exit() {

            context.wing1.localEulerAngles = Vector3.zero;
            context.wing2.localEulerAngles = Vector3.zero;

            base.Exit();
        }
    }

    [System.Serializable]
    private class Falling : State {

        public Falling(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.Fall(context.fallGravity);

            context.Run();

            base.Update();
        }
    }

    #endregion
}
