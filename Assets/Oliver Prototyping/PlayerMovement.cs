using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using StateMachine;

public class PlayerMovement : Player.Component {

    #region Parameters

    [Header("Running")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel, groundDeccel, airAccel, airDeccel;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float minJumpTime, jumpGravity, fallGravity, peakVelThreshold, peakGravity, maxFallSpeed, groundDetectDist;
    [SerializeField] private BufferTimer jumpBuffer;
    [SerializeField] private LayerMask groundMask;

    [Header("Flying")]
    [SerializeField] private float flightForce;
    [SerializeField] private float minStartFlightVel, maxFlightVel, maxFlightStamina, timeAfterJumpingBeforeFlight, dontFlyAboveGroundDist;

    [Header("Animation")]
    [SerializeField] private float maxSpriteAngle;
    [SerializeField] private float maxAngleVelocity, spriteRotationSpeed;
    [SerializeField] private Transform bodyPivot, wingPivot;

    #endregion

    #region Variables

    private StateMachine<PlayerMovement> stateMachine;

    private bool jumpBuffered;              // is jump buffered?

    private Vector2 velocity;               // current velocity (stored so that I can edit the x and y components individually)
    private RaycastHit2D groundHit;         // raycast hit for the ground
    private bool onGround;                  // is the player on the ground?
    private float groundDist;               // distance to the ground

    private float remainingFlightStamina;   // how much flight stamina reminas
    private float spriteRotationVelocity;         // current veloctiy of sprite rotation

    #endregion

    #region Public Functions

    public bool OnGround() => onGround;

    /// <summary> Set player x and/or y velocity individually. </summary>
    public void SetVelocity(float? x = null, float? y = null)
        => SetVelocity(new(x ?? Rigidbody.velocity.x, y ?? Rigidbody.velocity.y));

    /// <summary> Set palyer velocity. </summary>
    public void SetVelocity(Vector2 velocity) {
        Rigidbody.velocity = velocity;
        stateMachine.ChangeState(falling);
    }

    /// <summary> Add to player's current x and/or y velocity.</summary>
    public void AddVelocity(float x = 0, float y = 0)
        => AddVelocity(new(x, y));

    /// <summary> Add to player's current velocity. </summary>
    public void AddVelocity(Vector2 velocity) {
        Rigidbody.velocity += velocity;
        stateMachine.ChangeState(falling);
    }

    #endregion

    #region Awake and Update

    private void Awake() {

        InitializeStateMachine();
    }

    private void Update() {

        // input
        jumpBuffered = jumpBuffer.Buffer(Input.Jump.Down);

        // get information about current physical state

        velocity = Rigidbody.velocity;
        groundHit = Physics2D.BoxCast(transform.position, Collider.size, 0, Vector2.down, groundDetectDist, groundMask);
        onGround = groundHit && groundHit.normal == Vector2.up;
        var groundDistHit = Physics2D.BoxCast(transform.position, Collider.size, 0, Vector2.down, Mathf.Infinity, groundMask);
        groundDist = groundDistHit ? transform.position.y - Collider.bounds.extents.y - groundDistHit.point.y : Mathf.Infinity;

        // run state machines

        stateMachine.Update(Time.deltaTime);

        // apply velocity

        Rigidbody.velocity = velocity;

        // visuals

        float targetAngle = maxSpriteAngle * Mathf.Clamp(velocity.y / maxAngleVelocity, -1, 1) * Facing;

        bodyPivot.eulerAngles = Vector3.forward * Mathf.SmoothDampAngle(bodyPivot.eulerAngles.z, targetAngle, ref spriteRotationVelocity, spriteRotationSpeed);

        bodyPivot.localScale = new(Facing, 1, 1);
    }

    #endregion

    #region Helper Functions

    private void Run() {

        float accel = onGround
            ? InputDirection.x != 0 ? groundAccel : groundDeccel
            : InputDirection.x != 0 ? airAccel    : airDeccel;

        velocity.x = Mathf.MoveTowards(velocity.x, InputDirection.x * runSpeed, accel * Time.deltaTime);
    }

    private void Fall(float gravity) {

        gravity = Mathf.Abs(velocity.y) < peakVelThreshold ? peakGravity : gravity;

        velocity.y = Mathf.MoveTowards(velocity.y, -maxFallSpeed, gravity * Time.deltaTime);
    }

    #endregion

    #region State Machine

    #region Helper Classes

    private class State : State<PlayerMovement> {

        public State(PlayerMovement context) : base(context) {

        }
    }

    private class SubState : SubState<PlayerMovement, State> {

        public SubState(PlayerMovement context, State superState) : base(context, superState) {

        }
    }

    #endregion

    // instances of each state class
    private Grounded grounded;
    private Jumping jumping;
    private Falling falling;
    private Flying flying;

    private void InitializeStateMachine() {

        // initialize states
        grounded    = new(this);
        jumping     = new(this);
        falling     = new(this);
        flying      = new(this);

        // define transition requirements
        TransitionDelegate

            toGrounded  = () => onGround,

            startJump   = () => jumpBuffered && onGround,
            endJump     = () => (!Input.Jump.Pressed && stateMachine.stateDuration > minJumpTime) || velocity.y <= 0,

            toFalling   = () => !onGround,

            toFlight    = () => remainingFlightStamina > 0
                                && !onGround
                                && ((Input.Jump.Pressed && !jumpBuffered) || (groundDist > dontFlyAboveGroundDist && jumpBuffered))
                                && (stateMachine.previousState != jumping || stateMachine.stateDuration > timeAfterJumpingBeforeFlight),
            endFlying   = () => !Input.Jump.Pressed || remainingFlightStamina <= 0;

        // initialize state machine
        stateMachine = new(

            firstState: grounded,

            // define transitions
            transitions: new() {

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

    [Serializable]
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

    [Serializable]
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

    [Serializable]
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
            context.wingPivot.localEulerAngles = Vector3.forward * (-45 + 15 * wingOscillation);

            context.Run();

            base.Update();
        }

        public override void Exit() {

            context.wingPivot.localEulerAngles = Vector3.zero;

            base.Exit();
        }
    }

    [Serializable]
    private class Falling : State {

        public Falling(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.Fall(context.fallGravity);

            context.Run();

            base.Update();
        }
    }

    private class Headbutting : State {

        public Headbutting(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();
        }
    }

    #endregion
}
