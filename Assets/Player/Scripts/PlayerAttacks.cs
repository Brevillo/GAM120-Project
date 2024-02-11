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
    [SerializeField] private CameraShakeProfile headbuttHitShake;
    [SerializeField] private CameraBounceProfile headbuttHitBounce;
    [SerializeField] private SoundEffect headbuttActionSound, headbuttHitSound;

    [Header("Swinging")]
    [SerializeField] private float swingDamage;
    [SerializeField] private float swingKnockback, swingChargeTime, swingDuration, minChargeShake, maxChargeShake, chargingRunSpeed;
    [SerializeField] private EntityHealthCollisionTrigger swingTrigger;
    [SerializeField] private Transform swingTriggerPivot;
    [SerializeField] private CameraShakeProfile swingHitShake;
    [SerializeField] private List<SpriteRenderer> swingChargeRends;
    [SerializeField] private SoundEffect swingActionSound, swingHitSound;

    #endregion

    private StateMachine<PlayerAttacks> stateMachine;

    private Vector2 headbuttDirection;  // direction of the current headbutt

    private void Awake() {

        InitializeStateMachine();

        headbuttTrigger.OnEntityCollision.AddListener(OnHeadbuttCollision);
        swingTrigger.OnEntityCollision.AddListener(OnSwingCollision);
    }

    private void Update() {

        stateMachine.Update(Time.deltaTime);
    }

    #region Attack Hooks for PlayerMovement

    public void EnterHeadbutt(Vector2 direction) {
        stateMachine.ChangeState(headbutting);
        headbuttDirection = direction;
    }

    public void ExitHeadbutt() {
        stateMachine.ChangeState(Input.Attack.Pressed ? charging : idle);
    }

    public void EnterSwingCharge() {
        if (stateMachine.currentState != charging)
            stateMachine.ChangeState(charging);
    }

    #endregion

    #region Collision Functions

    private void OnHeadbuttCollision(EntityHealth entity) {

        if (entity.Team == Health.Team) return;

        if (stateMachine.currentState == headbutting) {

            entity.TakeDamage(new(headbuttDamage * PlayerHealth.DamageMultiplier, headbuttDirection, headbuttDirection * headbuttKnockback));
            CameraEffects.AddShake(headbuttHitShake);
            CameraEffects.AddBounce(headbuttHitBounce, headbuttDirection);
            headbuttHitSound.Play(this);
        }
    }

    private void OnSwingCollision(EntityHealth entity) {

        if (entity.Team == Health.Team) return;

        if (stateMachine.currentState == swinging) {

            entity.TakeDamage(new(swingDamage * PlayerHealth.DamageMultiplier, InputDirection, headbuttDirection * swingKnockback));
            CameraEffects.AddShake(swingHitShake);
            swingHitSound.Play(this);
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
    private Charging charging;
    private Swinging swinging;

    private void InitializeStateMachine() {

        idle          = new(this);
        headbutting   = new(this);
        charging      = new(this);
        swinging      = new(this);

        TransitionDelegate

            stopSwingCharge = () => !Input.Attack.Pressed && stateMachine.stateDuration < swingChargeTime,

            startSwing      = () => !Input.Attack.Pressed && stateMachine.stateDuration >= swingChargeTime,
            stopSwing       = () => stateMachine.stateDuration > swingDuration;

        stateMachine = new(

            firstState: idle,

            transitions: new() {

                { charging , new() {
                    new(idle, stopSwingCharge),
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
    private class Charging : State {

        public Charging(PlayerAttacks context) : base(context) { }

        private bool flicker;
        private float flickerTimer;

        public override void Update() {

            float chargePercent = context.stateMachine.stateDuration / context.swingChargeTime;

            context.BodyPivot.localPosition = UnityEngine.Random.insideUnitCircle * Mathf.Lerp(context.minChargeShake, context.maxChargeShake, chargePercent);

            if (chargePercent >= 1) {

                flickerTimer += Time.deltaTime;
                if (flickerTimer > 0.1f) {
                    flicker = !flicker;
                    flickerTimer = 0f;
                }

                var color = flicker ? Color.white : Color.red;

                context.swingChargeRends.ForEach(rend => rend.color = color);
            }

            base.Update();
        }

        public override void Exit() {

            context.BodyPivot.localPosition = Vector2.zero;
            context.swingChargeRends.ForEach(rend => rend.color = Color.white);

            base.Exit();
        }
    }

    [Serializable]
    private class Swinging : State {

        public Swinging(PlayerAttacks context) : base(context) { }

        public override void Enter() {

            base.Enter();

            Vector2 dir = context.InputDirection;

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
