using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using OliverBeebe.UnityUtilities.Runtime.Camera;
using OliverBeebe.UnityUtilities.Runtime;

public class PlayerAttacks : Player.Component {

    #region Parameters

    [SerializeField] private float attackCooldown;

    [Header("Headbutting")]
    [SerializeField] private float headbuttDamage;
    [SerializeField] private float headbuttEnemyKnockback;
    [SerializeField] private EntityHealthCollisionTrigger headbuttTrigger;
    [SerializeField] private float headbuttHitTimestop;
    [SerializeField] private CameraShakeProfile headbuttHitShake;
    [SerializeField] private CameraBounceProfile headbuttHitBounce;
    [SerializeField] private SoundEffect headbuttActionSound, headbuttHitSound;

    [Header("Swinging")]
    [SerializeField] private float swingDamage;
    [SerializeField] private float swingEnemyKnockback, swingSelfKnockback, swingWallKnockback, swingDuration;
    [SerializeField] private BufferTimer swingBuffer;
    [SerializeField] private EntityHealthCollisionTrigger swingTrigger;
    [SerializeField] private Transform swingTriggerPivot;
    [SerializeField] private CameraShakeProfile swingHitShake;
    [SerializeField] private SoundEffect swingActionSound, swingHitSound;

    [Header("Effects")]
    [SerializeField] private GameObject enemyHitParticles;
    [SerializeField] private GameObject wallHitParticles;

    #endregion

    private StateMachine<PlayerAttacks> stateMachine;

    private float attackCooldownRemaining;

    public bool AttackCooldownReady => attackCooldownRemaining <= 0;
    private void ResetAttackCooldown() => attackCooldownRemaining = attackCooldown;

    private Vector2
        headbuttDirection,  // direction of the current headbutt
        swingDirection;     // direction of the current swing    

    private void Awake() {

        InitializeStateMachine();

        headbuttTrigger.OnEntityEnter.AddListener(OnHeadbuttEntityCollision);
        headbuttTrigger.OnNonEntityEnter.AddListener(OnHeadbuttNonEntityCollision);

        swingTrigger.OnEntityEnter.AddListener(OnSwingEntityCollision);
        swingTrigger.OnNonEntityEnter.AddListener(OnSwingNonEntityCollision);
    }

    private void Start() {
        
        headbuttTrigger.enabled = false;
        swingTrigger.enabled = false;
    }

    private void Update() {

        attackCooldownRemaining -= Time.deltaTime;

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

    private Vector2 NoDownwards(Vector2 vector) => new(vector.x, MathF.Max(vector.y, 0));

    private void EnemyHitEffects(EntityHealth entity, Vector2 direction) {
        Instantiate(enemyHitParticles, entity.transform.position, Quaternion.FromToRotation(Vector2.right, direction));
    }

    private void WallHitEffects(Collider2D collision, Vector2 direction) {
        Instantiate(wallHitParticles, collision.ClosestPoint(transform.position), Quaternion.FromToRotation(Vector2.right, -direction));
    }

    private void HeadbuttHitEffects() {
        CameraEffects.Effects.AddShake(headbuttHitShake);
        CameraEffects.Effects.AddBounce(headbuttHitBounce, headbuttDirection);
    }

    private void OnHeadbuttEntityCollision(EntityHealthCollision collision) {

        if (collision.entity.Team == Health.Team) return;

        if (stateMachine.currentState == headbutting) {

            collision.entity.TakeDamage(new(headbuttDamage * PlayerHealth.DamageMultiplier, headbuttDirection, headbuttDirection * headbuttEnemyKnockback));

            Movement.RefillOneAerialHeadbutt();

            HeadbuttHitEffects();
            TimeManager.FreezeTime(headbuttHitTimestop, this);
            headbuttHitSound.Play(this);

            EnemyHitEffects(collision.entity, headbuttDirection);
        }
    }

    private void OnHeadbuttNonEntityCollision(Collider2D collision) {

        if (collision.gameObject.layer == GameInfo.GroundLayer && !Movement.OnGround) {

            HeadbuttHitEffects();
            WallHitEffects(collision, headbuttDirection);
        }
    }

    private void SwingHitEffects() {
        CameraEffects.Effects.AddShake(swingHitShake);
    }

    private void OnSwingEntityCollision(EntityHealthCollision collision) {

        if (collision.entity.Team == Health.Team) return;

        if (stateMachine.currentState == swinging) {

            collision.entity.TakeDamage(new(swingDamage * PlayerHealth.DamageMultiplier, InputDirection, swingDirection * swingEnemyKnockback));

            if (swingDirection.y < 0) Movement.RefillAirMovement();

            Movement.TakeKnockback(NoDownwards(-swingDirection) * swingSelfKnockback);

            SwingHitEffects();
            swingHitSound.Play(this);
            EnemyHitEffects(collision.entity, swingDirection);
        }
    }

    private void OnSwingNonEntityCollision(Collider2D collision) {

        if (collision.gameObject.layer == GameInfo.GroundLayer && !Movement.OnGround) {

            SwingHitEffects();
            WallHitEffects(collision, swingDirection);
            Movement.TakeKnockback(NoDownwards(-swingDirection) * swingWallKnockback);
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

            startSwing      = () => swingBuffer && AttackCooldownReady,
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

            context.ResetAttackCooldown();

            context.headbuttActionSound.Play(context);

            context.headbuttTrigger.enabled = true;
        }

        public override void Exit() {

            context.headbuttTrigger.enabled = false;

            base.Exit();
        }
    }

    [Serializable]
    private class Swinging : State {

        public Swinging(PlayerAttacks context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.ResetAttackCooldown();

            Vector2 swingDirection = context.InputDirection * new Vector2(context.Facing, context.CrawlOrientation);

            // no downwards swings when grounded
            if (context.Movement.OnGround) swingDirection.y = Mathf.Max(0, swingDirection.y);

            // if no input, default to facing direction
            if (swingDirection == Vector2.zero) swingDirection = Vector2.right * context.Facing;

            context.swingDirection = swingDirection;
            context.BodyAnimation.SwingAnimation(swingDirection);

            context.swingTriggerPivot.localEulerAngles = Vector3.forward * Mathf.Atan2(swingDirection.y, Mathf.Abs(swingDirection.x)) * Mathf.Rad2Deg;
            context.swingTrigger.enabled = true;

            context.swingActionSound.Play(context);
        }

        public override void Exit() {

            context.swingTrigger.enabled = false;

            base.Exit();
        }
    }

    #endregion
}
