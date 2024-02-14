using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using StateMachine;

public class PlayerMovement : Player.Component {

    #region Parameters

    [Header("Running")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel, groundDeccel, airAccel, airDeccel, maxSlopeAngle, groundedOffset, groundGravity;
    [SerializeField] private float stepSoundFrequency;
    [SerializeField] private SoundEffect stepSound;

    [Header("Eating")]
    [SerializeField] private float eatDuration;
    [SerializeField] private SoundEffect eatSound;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float minJumpTime, jumpGravity, fallGravity, peakVelThreshold, peakGravity, maxFallSpeed, groundDetectDist, fastFallGravity, fastFallSpeed;
    [SerializeField] private BufferTimer jumpBuffer;
    [SerializeField] private SoundEffect jumpSound;

    [Header("Flying")]
    [SerializeField] private float flightForce;
    [SerializeField] private float minStartFlightVel, maxFlightVel, maxFlightStamina, timeAfterJumpingBeforeFlight, dontFlyAboveGroundDist;
    [SerializeField] private SoundEffect flightSound;

    [Header("Headbutting")]
    [SerializeField] private float headbuttCooldown;
    [SerializeField] private float headbuttDist, headbuttDuration, headbuttMaxChargeDuration, headbuttChargeLoss, headbuttChargeRunSpeed, headbuttChargeLightGravity, headbuttChargeGravity, wavedashVelocity, wavedashGroundDist;
    [SerializeField] private Vector2 headbuttExitForce;
    [SerializeField] private int maxHeadbuttsInAir;
    [SerializeField] private BufferTimer headbuttBuffer;
    [SerializeField] private SmartCurve headbuttChargeAnimation;
    [SerializeField] private CameraShakeProfile headbuttChargeShake;
    [SerializeField] private Transform headPivot;

    [Header("Taking Knockback")]
    [SerializeField] private float knockbackFriction;

    [Header("Animation")]
    [SerializeField] private float maxSpriteAngle;
    [SerializeField] private float maxAngleVelocity, groundRotateSpeed, airRotateSpeed;
    [SerializeField] private Transform wingPivot;
    [SerializeField] private Animator legsAnimator;
    [SerializeField] private AnimationClip legsIdleAnimation, legsCrawlingAnimation;

    #endregion

    #region Variables

    private StateMachine<PlayerMovement> stateMachine;

    private Vector2 velocity;               // current velocity (stored so that I can edit the x and y components individually)
    private RaycastHit2D groundHit;         // raycast hit for the ground
    private bool onGround;                  // is the player on the ground?
    private float groundDist;               // distance to the ground

    private float remainingFlightStamina;   // how much flight stamina reminas
    private float spriteRotationVelocity;   // current veloctiy of sprite rotation

    private int aerialHeadbuttsUsed;        // how many headbutts have been used after the player left the ground
    private int aerialHeadbuttsRemaining;   // how many headbutts the player has left after leaving the ground

    private AnimationClip currentLegAnimation;  // current leg animation being played by the leg animator

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

    /// <summary> Refills the player's flight stamina and aerial haedbutts. </summary>
    public void RefillAirMovement() {
        remainingFlightStamina = maxFlightStamina;
        aerialHeadbuttsRemaining = maxHeadbuttsInAir;
    }

    public void RefillOneAerialHeadbutt() => aerialHeadbuttsRemaining++;

    #endregion

    #region Awake and Update

    private void Awake() {

        InitializeStateMachine();
    }

    private void Update() {

        // input

        jumpBuffer.Buffer(Input.Jump.Down);
        headbuttBuffer.Buffer(Input.Headbutt.Down);

        // get information about current physical state

        RaycastHit2D GroundCast(float distance) {

            var hit1 = Physics2D.Raycast(transform.position, Vector2.down, distance, GameInfo.GroundMask);
            if (hit1) return hit1;

            var hit2 = Physics2D.CapsuleCast(transform.position, Collider.size, Collider.direction, 0, Vector2.down, distance, GameInfo.GroundMask);
            if (hit2) return hit2;

            return default;
        }

        velocity = Rigidbody.velocity;
        groundHit = GroundCast(groundDetectDist);
        float slopeAngle = Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(groundHit.normal.y, groundHit.normal.x) * Mathf.Rad2Deg, 90));
        onGround = groundHit && slopeAngle < maxSlopeAngle;

        var groundDistHit = GroundCast(Mathf.Infinity);
        groundDist = groundDistHit ? transform.position.y - Collider.bounds.extents.y - groundDistHit.point.y : Mathf.Infinity;

        // run state machines

        stateMachine.Update(Time.deltaTime);

        // apply velocity

        Rigidbody.velocity = velocity;

        // visuals

        Vector2 forward = -Vector2.Perpendicular(groundHit.normal);
        float targetAngle = onGround
            ? Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg
            : maxSpriteAngle * Mathf.Clamp(velocity.y / maxAngleVelocity, -1, 1) * Facing;

        float rotateSpeed = onGround ? groundRotateSpeed : airRotateSpeed;
        Rigidbody.SetRotation(Mathf.SmoothDampAngle(Rigidbody.rotation, targetAngle, ref spriteRotationVelocity, rotateSpeed));
        Debug.DrawRay(groundHit.point, groundHit.normal, Color.green);

        var newLegAnimation = InputDirection.x != 0 && velocity.x != 0 && onGround ? legsCrawlingAnimation : legsIdleAnimation;
        if (newLegAnimation != currentLegAnimation) {
            legsAnimator.Play(newLegAnimation.name);
            currentLegAnimation = newLegAnimation;
        }

        BodyPivot.localScale = new(Facing, 1, 1);
    }

    #endregion

    #region Helper Functions

    private void Run(float? customSpeed = null) {

        (float accel, float speed, Vector2 vel) = onGround

            ? ( accel:  InputDirection.x != 0 ? groundAccel : groundDeccel,
                speed:  customSpeed ?? runSpeed,
                vel:    (Vector2)transform.InverseTransformDirection(velocity))

            : ( accel:  InputDirection.x != 0 ? airAccel : airDeccel,
                speed:  Mathf.Max(customSpeed ?? runSpeed, Mathf.Abs(velocity.x)),
                vel:    velocity);

        vel.x = Mathf.MoveTowards(vel.x, speed * InputDirection.x, accel * Time.deltaTime);

        velocity = onGround
            ? -(Vector2)transform.up * groundGravity + (Vector2)transform.right * vel.x
            : vel;
    }

    private void Fall(float gravity, float? customFallSpeed = null) => velocity.y = Mathf.MoveTowards(velocity.y, -(customFallSpeed ?? maxFallSpeed), gravity * Time.deltaTime);

    private void FallWithPeakGravity(float gravity, float? customFallSpeed = null) => Fall(Mathf.Abs(velocity.y) < peakVelThreshold ? peakGravity : gravity, customFallSpeed);

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
    private Eating          eating;
    private Jumping         jumping;
    private Falling         falling;
    private Flying          flying;
    private HeadbuttCharge  headbuttCharge;
    private Headbutting     headbutting;
    private Knockbacking    knockbacking;

    private void InitializeStateMachine() {

        // initialize states
        grounded        = new(this);
        eating          = new(this);
        jumping         = new(this);
        falling         = new(this);
        flying          = new(this);
        headbuttCharge  = new(this);
        headbutting     = new(this);
        knockbacking    = new(this);

        // define transition requirements
        TransitionDelegate

            toGrounded      = () => onGround,

            toEating        = () => Input.Eat.Pressed && PlayerHealth.CanEatMore,
            stopEating      = () => !Input.Eat.Pressed || !PlayerHealth.CanEatMore,

            toJump          = () => jumpBuffer && onGround,
            endJump         = () => (!Input.Jump.Pressed && stateMachine.stateDuration > minJumpTime) || velocity.y <= 0,

            toFalling       = () => !onGround,

            toFlight        = () => remainingFlightStamina > 0 && !onGround
                                 && ((Input.Jump.Pressed && !jumpBuffer) || (groundDist > dontFlyAboveGroundDist && jumpBuffer))          // so you don't accidentally start flying if you try to buffer a jump
                                 && (stateMachine.previousState != jumping || stateMachine.stateDuration > timeAfterJumpingBeforeFlight), // so you can't immediateley fly after jumping
            endFlying       = () => !Input.Jump.Pressed || remainingFlightStamina <= 0,

            toHbCharge      = () => headbuttBuffer && (onGround || aerialHeadbuttsRemaining > 0)
                                 && (stateMachine.previousState != headbutting || stateMachine.stateDuration > headbuttCooldown), // headbutt cooldown

            skipHbCharge    = () => toHbCharge() && headbuttChargeTime == 0,
            startHeadbutt   = () => !Input.Headbutt.Pressed || stateMachine.stateDuration > headbuttChargeTime,

            stopHeadbutt    = () => stateMachine.stateDuration > headbuttDuration,
            stopHbGrounded  = () => stopHeadbutt() && onGround,
            stopHbFalling   = () => stopHeadbutt() && !onGround,

            toWavedash      = () => jumpBuffer && groundDist < wavedashGroundDist,

            stopKnockback   = () => Mathf.Abs(velocity.x) <= runSpeed,
            stopKbGrounded  = () => stopKnockback() && onGround,
            stopKbFalling   = () => stopKnockback() && !onGround;

        // common transitions
        StateMachine<PlayerMovement>.Transition

            startHeadbuttCharge = new(headbuttCharge,   toHbCharge),
            skipHeadbuttCharge  = new(headbutting,      skipHbCharge);

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
                    new(jumping,    toJump),
                    new(falling,    toFalling),
                    new(eating,     toEating),
                    skipHeadbuttCharge,
                    startHeadbuttCharge,
                } },

                { eating, new() {
                    new(grounded,   stopEating),
                    new(falling,    toFalling),
                    skipHeadbuttCharge,
                    startHeadbuttCharge,
                } },

                { jumping, new() {
                    new(falling,    endJump),
                    skipHeadbuttCharge,
                    startHeadbuttCharge,
                } },

                { falling, new() {
                    new(grounded,   toGrounded),
                    new(flying,     toFlight),
                    skipHeadbuttCharge,
                    startHeadbuttCharge,
                } },

                { flying, new() {
                    new(falling,    endFlying),
                    skipHeadbuttCharge,
                    startHeadbuttCharge,
                } },

                { headbuttCharge, new() {
                    new(headbutting, startHeadbutt),
                } },

                { headbutting, new() {
                    new(grounded,   stopHbGrounded),
                    new(falling,    stopHbFalling),
                    new(jumping,    toWavedash),
                } },

                { knockbacking, new() {
                    new(grounded,   stopKbGrounded),
                    new(falling,    stopKbFalling),
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

        private float stepSoundTimer, velocity;

        public override void Enter() {

            base.Enter();

            velocity = context.velocity.x;
        }

        public override void Update() {

            stepSoundTimer += Time.deltaTime;
            if (stepSoundTimer > context.stepSoundFrequency && context.InputDirection.x != 0) {
                stepSoundTimer = 0;
                context.stepSound.Play(context);
            }

            context.Run();

            base.Update();
        }

        public override void Exit() {

            context.RefillAirMovement();
            context.aerialHeadbuttsUsed = 0;

            base.Exit();
        }
    }

    [Serializable]
    private class Eating : State {

        public Eating(PlayerMovement context) : base(context) { }

        private float eatTimer;

        public override void Enter() {

            base.Enter();

            context.velocity = Vector2.zero;

            context.eatSound.Play(context);

            eatTimer = 0;
        }

        public override void Update() {

            eatTimer += Time.deltaTime;

            if (eatTimer > context.eatDuration) {
                eatTimer = 0;
                context.PlayerHealth.EatingZenIncrease();
            }

            base.Update();
        }

        public override void Exit() {

            context.eatSound.Stop(context);

            base.Exit();
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

            context.FallWithPeakGravity(context.jumpGravity);

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

            context.flightSound.Play(context);
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

            context.flightSound.Stop(context);

            base.Exit();
        }
    }

    [Serializable]
    private class Falling : State {

        public Falling(PlayerMovement context) : base(context) { }

        public override void Update() {

            float fastFallPercent = -context.Input.Movement.Vector.y,
                  speed           = Mathf.Lerp(context.maxFallSpeed, context.fastFallSpeed, fastFallPercent),
                  gravity         = Mathf.Lerp(context.fallGravity, context.fastFallGravity, fastFallPercent);

            context.FallWithPeakGravity(gravity, speed);

            context.Run();

            base.Update();
        }
    }

    private float headbuttChargeTime => Mathf.Max(0, headbuttMaxChargeDuration - aerialHeadbuttsUsed * headbuttChargeLoss);

    [Serializable]
    private class HeadbuttCharge : State {

        public HeadbuttCharge(PlayerMovement context) : base(context) { }

        private CameraEffects.EffectToken shakeEffect;

        public override void Enter() {

            base.Enter();

            context.velocity.y = Mathf.Max(context.velocity.y, 0);

            context.aerialHeadbuttsUsed++;

            float maxChargeTime = context.headbuttChargeTime;

            context.headbuttChargeAnimation.timeScale = maxChargeTime;
            context.headbuttChargeAnimation.Start();
            context.headbuttChargeShake.duration = maxChargeTime;
            shakeEffect = CameraEffects.AddShake(context.headbuttChargeShake);
        }

        public override void Update() {

            context.Run(context.headbuttChargeRunSpeed);
            context.Fall(context.InputDirection.y > 0 ? context.headbuttChargeLightGravity : context.headbuttChargeGravity);

            context.headPivot.localPosition = Vector2.left * context.headbuttChargeAnimation.Evaluate();

            base.Update();
        }

        public override void Exit() {

            context.headPivot.localPosition = Vector2.zero;
            CameraEffects.RemoveEffect(shakeEffect);

            base.Exit();
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

            context.velocity = direction * context.headbuttDist / context.headbuttDuration;

            context.Attacks.EnterHeadbutt(direction);
        }

        public override void Exit() {

            context.velocity = direction * context.headbuttExitForce;

            if (context.jumpBuffer) context.velocity.x = Mathf.Sign(context.velocity.x) * context.wavedashVelocity;

            context.Attacks.ExitHeadbutt();

            base.Exit();
        }
    }

    [Serializable]
    private class Knockbacking : State {

        public Knockbacking(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.velocity.x = Mathf.MoveTowards(context.velocity.x, 0, context.knockbackFriction * Time.deltaTime);
            context.FallWithPeakGravity(context.fallGravity);

            base.Update();
        }
    }

    #endregion
}
