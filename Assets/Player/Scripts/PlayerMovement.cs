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

    [Header("Eating")]
    [SerializeField] private float eatDuration;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float minJumpTime, jumpGravity, fallGravity, peakVelThreshold, peakGravity, maxFallSpeed, groundDetectDist;
    [SerializeField] private BufferTimer jumpBuffer;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private SoundEffect jumpSound;//

    [Header("Flying")]
    [SerializeField] private float flightForce;
    [SerializeField] private float minStartFlightVel, maxFlightVel, maxFlightStamina, timeAfterJumpingBeforeFlight, dontFlyAboveGroundDist;

    [Header("Headbutting")]
    [SerializeField] private float headbuttCooldown;
    [SerializeField] private SmartCurve headbuttCurve;
    [SerializeField] private int maxHeadbuttsInAir;
    [SerializeField] private BufferTimer headbuttBuffer;

    [Header("Taking Knockback")]
    [SerializeField] private float knockbackFriction;

    [Header("Animation")]
    [SerializeField] private float maxSpriteAngle;
    [SerializeField] private float maxAngleVelocity, spriteRotationSpeed;
    [SerializeField] private Transform wingPivot;

    #endregion

    #region Variables

    private StateMachine<PlayerMovement> stateMachine;

    private Vector2 velocity;               // current velocity (stored so that I can edit the x and y components individually)
    private RaycastHit2D groundHit;         // raycast hit for the ground
    private bool onGround;                  // is the player on the ground?
    private float groundDist;               // distance to the ground

    private float remainingFlightStamina;   // how much flight stamina reminas
    private float spriteRotationVelocity;   // current veloctiy of sprite rotation

    private int aerialHeadbuttsRemaining;   // how many headbutts the player has left after leaving the ground

    #endregion

    #region Public Functions

    public bool OnGround() => onGround;

    /// <summary> Set player x and/or y velocity individually. </summary>
    public void SetVelocity(float? x = null, float? y = null)
        => SetVelocity(new(x ?? Rigidbody.velocity.x, y ?? Rigidbody.velocity.y));

    /// <summary> Set player velocity. </summary>
    public void SetVelocity(Vector2 velocity) {
        Rigidbody.velocity = velocity;
        stateMachine.ChangeState(falling);
    }

    /// <summary> Knocks the player back by distance. </summary>
    public void TakeKnockback(Vector2 distance) {

        Rigidbody.velocity = new Vector2(
            Mathf.Sqrt(Mathf.Abs(distance.x) * knockbackFriction * 2f) * Mathf.Sign(distance.x),
            Mathf.Sqrt(Mathf.Abs(distance.y) * fallGravity * 2) * Mathf.Sign(distance.y));

        stateMachine.ChangeState(knockbacking);
    }

    #endregion

    #region Awake and Update

    private void Awake() {

        InitializeStateMachine();
    }

    private void Update() {

        // input

        jumpBuffer.Buffer(Input.Jump.Down);
        headbuttBuffer.Buffer(Input.Attack.Down);

        // get information about current physical state

        velocity = Rigidbody.velocity;
        groundHit = Physics2D.BoxCast(transform.position, Collider.size, 0, Vector2.down, groundDetectDist, groundMask);
        onGround = groundHit;
        var groundDistHit = Physics2D.BoxCast(transform.position, Collider.size, 0, Vector2.down, Mathf.Infinity, groundMask);
        groundDist = groundDistHit ? transform.position.y - Collider.bounds.extents.y - groundDistHit.point.y : Mathf.Infinity;

        // run state machines

        stateMachine.Update(Time.deltaTime);

        if (Input.Attack.Pressed && !onGround && aerialHeadbuttsRemaining == 0) Attacks.EnterSwingCharge();

        // apply velocity

        Rigidbody.velocity = velocity;

        // visuals

        float targetAngle = maxSpriteAngle * Mathf.Clamp(velocity.y / maxAngleVelocity, -1, 1) * Facing;

        BodyPivot.eulerAngles = Vector3.forward * Mathf.SmoothDampAngle(BodyPivot.eulerAngles.z, targetAngle, ref spriteRotationVelocity, spriteRotationSpeed);

        BodyPivot.localScale = new(Facing, 1, 1);
    }

    #endregion

    #region Helper Functions

    private void Run(bool withMomentum) {

        float accel = onGround
                ? InputDirection.x != 0 ? groundAccel : groundDeccel
                : InputDirection.x != 0 ? airAccel    : airDeccel,
              speed = withMomentum
                ? Mathf.Max(runSpeed, Mathf.Abs(velocity.x))
                : runSpeed;

        velocity.x = Mathf.MoveTowards(velocity.x, speed * InputDirection.x, accel * Time.deltaTime);
    }

    private void Fall(float gravity) {

        // apply peak gravity if necessary
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
    private Grounded        grounded;
    private Jumping         jumping;
    private Falling         falling;
    private Flying          flying;
    private Headbutting     headbutting;
    private Knockbacking    knockbacking;

    private void InitializeStateMachine() {

        // initialize states
        grounded        = new(this);
        jumping         = new(this);
        falling         = new(this);
        flying          = new(this);
        headbutting     = new(this);
        knockbacking    = new(this);

        // define transition requirements
        TransitionDelegate

            toGrounded      = () => onGround,

            startJump       = () => jumpBuffer && onGround,
            endJump         = () => (!Input.Jump.Pressed && stateMachine.stateDuration > minJumpTime) || velocity.y <= 0,

            toFalling       = () => !onGround,

            toFlight        = () => remainingFlightStamina > 0 && !onGround
                                && ((Input.Jump.Pressed && !jumpBuffer) || (groundDist > dontFlyAboveGroundDist && jumpBuffer))          // so you don't accidentally start flying if you try to buffer a jump
                                && (stateMachine.previousState != jumping || stateMachine.stateDuration > timeAfterJumpingBeforeFlight), // so you can't immeditaley fly after jumping
            endFlying       = () => !Input.Jump.Pressed || remainingFlightStamina <= 0,

            startHeadbutt   = () => headbuttBuffer && (onGround || aerialHeadbuttsRemaining > 0)
                                && (stateMachine.previousState != headbutting || stateMachine.stateDuration > headbuttCooldown), // headbutt cooldown

            stopHeadbutt    = () => stateMachine.stateDuration > headbuttCurve.timeScale,
            stopHbGrounded  = () => stopHeadbutt() && onGround,
            stopHbFalling   = () => stopHeadbutt() && !onGround,

            stopKnockback   = () => Mathf.Abs(velocity.x) <= runSpeed,
            stopKbGrounded  = () => stopKnockback() && onGround,
            stopKbFalling   = () => stopKnockback() && !onGround;

        // common transitions
        StateMachine<PlayerMovement>.Transition

            toHeadbutt = new(headbutting, startHeadbutt);

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
                    toHeadbutt,
                } },

                { jumping, new() {
                    new(falling, endJump),
                    toHeadbutt,
                } },

                { falling, new() {
                    new(grounded, toGrounded),
                    new(flying, toFlight),
                    toHeadbutt,
                } },

                { flying, new() {
                    new(falling, endFlying),
                    toHeadbutt,
                } },

                { headbutting, new() {
                    new(grounded, stopHbGrounded),
                    new(falling, stopHbFalling),
                    new(jumping, startJump), // a dash of the wave variety perhaps?
                } },

                { knockbacking, new() {
                    new(grounded, stopKbGrounded),
                    new(falling, stopKbGrounded),
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

        public override void Update() {

            context.Run(false);

            context.Fall(1000);

            base.Update();
        }

        public override void Exit() {

            context.remainingFlightStamina = context.maxFlightStamina;
            context.aerialHeadbuttsRemaining = context.maxHeadbuttsInAir;

            base.Exit();
        }
    }

    [Serializable]
    private class Eating : State {

        public Eating(PlayerMovement context) : base(context) { }

        public override void Update() {



            base.Update();
        }
    }

    [Serializable]
    private class Jumping : State {

        public Jumping(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.jumpSound.Play(context);

            context.jumpBuffer.Reset();
            context.velocity.y = Mathf.Sqrt(context.jumpHeight * context.jumpGravity * 2f);
        }

        public override void Update() {

            context.Fall(context.jumpGravity);

            context.Run(true);

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

            context.Run(true);

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

            context.Run(true);

            base.Update();
        }
    }

    [Serializable]
    private class Headbutting : State {

        public Headbutting(PlayerMovement context) : base(context) { }

        private Vector2 direction;

        public override void Enter() {

            base.Enter();

            context.aerialHeadbuttsRemaining--;

            context.headbuttBuffer.Reset();

            direction = context.InputDirection != Vector2Int.zero
                ? context.Input.Movement.Vector.normalized
                : Vector2.right * context.Facing;

            context.headbuttCurve.Start();

            context.Attacks.EnterHeadbutt(direction);
        }

        public override void Update() {

            context.velocity = context.headbuttCurve.Evaluate(1) * direction;

            base.Update();
        }

        public override void Exit() {

            if (!context.onGround) context.velocity = Vector2.zero;
            else context.velocity *= 0.5f;
            context.Attacks.ExitHeadbutt();

            base.Exit();
        }
    }

    [Serializable]
    private class Knockbacking : State {

        public Knockbacking(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.velocity.x = Mathf.MoveTowards(context.velocity.x, 0, context.knockbackFriction * Time.deltaTime);
            context.Fall(context.fallGravity);

            base.Update();
        }
    }

    #endregion
}
