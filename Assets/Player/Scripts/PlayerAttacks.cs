using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StateMachine;

public class PlayerAttacks : Player.Component {

    #region Parameters

    [Header("Headbutting")]
    [SerializeField] private float headbuttDamage;
    [SerializeField] private float headbuttKnockback;
    [SerializeField] private EntityHealthCollisionTrigger headbuttTrigger;
    [SerializeField] private float headbuttHitTimestop;
    [SerializeField] private CameraShakeProfile headbuttHitShake;
    [SerializeField] private CameraBounceProfile headbuttHitBounce;
    [SerializeField] private SoundEffect headbuttActionSound, headbuttHitSound;

    [Header("Swinging")]
    [SerializeField] private float swingDamage;
    [SerializeField] private float swingKnockback, swingDuration, swingCooldown;
    [SerializeField] private BufferTimer swingBuffer;
    [SerializeField] private EntityHealthCollisionTrigger swingTrigger;
    [SerializeField] private Transform swingTriggerPivot;
    [SerializeField] private CameraShakeProfile swingHitShake;
    [SerializeField] private SoundEffect swingActionSound, swingHitSound;

    [Header("Effects")]
    [SerializeField] private GameObject hitParticles;

    #endregion

    private StateMachine<PlayerAttacks> stateMachine;

    private Vector2
        headbuttDirection,  // direction of the current headbutt
        swingDirection;     // direction of the current swing    

    private void Awake() {

        InitializeStateMachine();

        headbuttTrigger.OnEntityCollision.AddListener(OnHeadbuttCollision);
        swingTrigger.OnEntityCollision.AddListener(OnSwingCollision);
    }

    private void Update() {

        swingBuffer.Buffer(Input.Swing.Down);

        stateMachine.Update(Time.deltaTime);
    }

    #region Attack Hooks for PlayerMovement

    public void EnterHeadbutt(Vector2 direction) {
        stateMachine.ChangeState(headbutting);
        headbuttDirection = direction;
    }

    public void ExitHeadbutt() {
        stateMachine.ChangeState(idle);
    }

    #endregion

    #region Collision Functions

    private void OnHeadbuttCollision(EntityHealth entity) {

        if (entity.Team == Health.Team) return;

        if (stateMachine.currentState == headbutting) {

            entity.TakeDamage(new(headbuttDamage * PlayerHealth.DamageMultiplier, headbuttDirection, headbuttDirection * headbuttKnockback));
            CameraEffects.AddShake(headbuttHitShake);
            CameraEffects.AddBounce(headbuttHitBounce, headbuttDirection);
            TimeManager.FreezeTime(headbuttHitTimestop, this);
            headbuttHitSound.Play(this);
            Instantiate(hitParticles, entity.transform.position, Quaternion.FromToRotation(Vector2.right, headbuttDirection));
        }
    }

    private void OnSwingCollision(EntityHealth entity) {

        if (entity.Team == Health.Team) return;

        if (stateMachine.currentState == swinging) {

            entity.TakeDamage(new(swingDamage * PlayerHealth.DamageMultiplier, InputDirection, swingDirection * swingKnockback));
            CameraEffects.AddShake(swingHitShake);
            swingHitSound.Play(this);
            Instantiate(hitParticles, entity.transform.position, Quaternion.FromToRotation(Vector2.right, swingDirection));

            if (swingDirection.y < 0) Movement.RefillAirMovement();
        }
    }

    #endregion

    #region State Machine

    #region Helper Classes

    private class State : State<PlayerAttacks> {

        public State(PlayerAttacks context) : base(context) {

        }
    }

    private class SubState : SubState<PlayerAttacks, State> {

        public SubState(PlayerAttacks context, State superState) : base(context, superState) {

        }
    }

    #endregion

    // instances of each headbutt state class
    private Idle idle;
    private Headbutting headbutting;
    private Swinging swinging;

    private void InitializeStateMachine() {

        idle          = new(this);
        headbutting   = new(this);
        swinging      = new(this);

        TransitionDelegate

            startSwing      = () => swingBuffer && (stateMachine.previousState != swinging || stateMachine.stateDuration >= swingCooldown),
            stopSwing       = () => stateMachine.stateDuration > swingDuration;

        stateMachine = new(

            firstState: idle,

            transitions: new() {

                { idle, new() {
                    new(swinging, startSwing),
                } },

                { swinging, new() {
                    new(idle, stopSwing)
                } },
            }
        );
    }

    [Serializable]
    private class Idle : State {

        public Idle(PlayerAttacks context) : base(context) { }
    }

    [Serializable]
    private class Headbutting : State {

        public Headbutting(PlayerAttacks context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.headbuttActionSound.Play(context);

            context.headbuttTrigger.gameObject.SetActive(true);
        }

        public override void Exit() {

            context.headbuttTrigger.gameObject.SetActive(false);

            base.Exit();
        }
    }

    [Serializable]
    private class Swinging : State {

        public Swinging(PlayerAttacks context) : base(context) { }

        public override void Enter() {

            base.Enter();

            Vector2 dir = context.swingDirection = context.InputDirection;

            // no downwards swings when grounded
            if (context.Movement.OnGround()) dir.y = Mathf.Max(0, dir.y);

            // if no input, default to facing direction
            if (dir == Vector2.zero) dir = Vector2.right * context.Facing;

            context.swingTriggerPivot.localEulerAngles = Vector3.forward * Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
            context.swingTrigger.gameObject.SetActive(true);

            context.swingActionSound.Play(context);
        }

        public override void Exit() {

            context.swingTrigger.gameObject.SetActive(false);

            base.Exit();
        }
    }

    #endregion
}
