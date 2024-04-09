using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;
using OliverBeebe.UnityUtilities.Runtime.Camera;
using System.Linq;

public class PlayerWhip : Player.Component {

    #region Parameters

    [SerializeField] private float whipBackForce;
    [SerializeField] private float
        whipForwardForce,
        whipForwardDelay,
        minWhipLength,
        maxWhipLength,
        maxLengthExtendDuration,
        minWhipDuration,
        maxWhipDuration,
        whipTriggerDelay,
        whipRetractSpeed,
        whipPullSpeed,
        whipMaxLength,
        enemyGrappleVerticalBoost,
        whipAutoAimRadius;
    [SerializeField] private VerletRope.Parameters whipRopeParameters;
    [SerializeField] private LineRenderer whipRenderer;
    [SerializeField] private PlayerWhipTrigger whipTrigger;

    [Header("EFfects")]
    [SerializeField] private float whipHitFreezeFrame;
    [SerializeField] private CameraShakeProfile hitEnemyShake;
    [SerializeField] private SoundEffect whipThrow, whipHit;
    #endregion

    #region Variables

    private IWhippable whipping;            // currently whipped thing
    private VerletRope whipRopeSim;         // current whip simulation

    private StateMachine<PlayerWhip> stateMachine;

    #endregion

    #region Awake and Update

    private void Awake() {
        InitializeWhipStateMachine();
    }

    private void Update() {
        stateMachine.Update(Time.deltaTime);
    }

    private void FixedUpdate() {

        // question mark means that it only runs these functions if whipRopeSim isn't null
        Vector2 whipOrigin = stateMachine.currentState == pullingSelf && whipping as Object != null ? whipping.WhippablePosition : transform.position;
        whipRopeSim?.Update(whipOrigin);
        whipRopeSim?.ApplyToLineRenderer(whipRenderer);
    }

    #endregion

    #region Helper Functions

    private void OnWhipCollision(Collider2D collision) {
        if (collision.TryGetComponent(out IWhippable whippable))
            whipping = whippable;
    }

    private void RetractWhip(float speed) => whipRopeSim.Length = Mathf.MoveTowards(whipRopeSim.Length, 0, speed * Time.deltaTime);

    #endregion

    #region State Machine

    #region Helper Classes

    private class State : State<PlayerWhip> {

        public State(PlayerWhip context) : base(context) {

        }
    }

    private class SubState : SubState<PlayerWhip, State> {

        public SubState(PlayerWhip context, State superState) : base(context, superState) {

        }
    }

    #endregion

    // state instances
    private Idle idle;
    private Extending extending;
    private Retracting retracting;
    private PullingEnemy pullingEnemy;
    private PullingSelf pullingSelf;

    private void InitializeWhipStateMachine() {

        idle            = new(this);
        extending       = new(this);
        retracting      = new(this);
        pullingEnemy    = new(this);
        pullingSelf     = new(this);

        TransitionDelegate

            startWhip       = () => Input.Whip.Down,
            stopWhip        = () => (stateMachine.stateDuration > minWhipDuration && !Input.Whip.Pressed) || stateMachine.stateDuration > maxWhipDuration,

            whipRetracted   = () => whipRopeSim.Length == 0,

            pullEnemy       = () => whipping != null && whipping.WhippableType == IWhippable.Type.Light,

            pullSelf        = () => whipping != null && whipping.WhippableType == IWhippable.Type.Heavy,

            targetIsNull    = () => whipping == null;

        stateMachine = new(

            firstState: idle,

            transitions: new() {

                { idle, new() {
                    new(extending,      startWhip),
                } },

                { extending, new() {
                    new(retracting,     stopWhip),
                    new(pullingEnemy,   pullEnemy),
                    new(pullingSelf,    pullSelf),
                } },

                { retracting, new() {
                    new(idle,           whipRetracted),
                } },

                { pullingEnemy, new() {
                    new(idle,           whipRetracted),
                    new(retracting,     targetIsNull),
                } },

                { pullingSelf, new() {
                    new(idle,           whipRetracted),
                    new(retracting,     targetIsNull),
                } },
            }
        );
    }

    private class Idle : State {

        public Idle(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.whipRopeSim = null;
            context.whipRenderer.enabled = false;
        }
    }

    private class Extending : State {

        public Extending(PlayerWhip context) : base(context) { }

        private Vector2 aimDirection;
        private PlayerWhipTrigger activeWhipTrigger;
        private bool whippedForward, triggerActive;

        public override void Enter() {

            base.Enter();

            context.whipRenderer.enabled = true;

            context.whipRopeSim = new(
                parameters: context.whipRopeParameters,
                start: context.transform.position,
                end:   context.transform.position);

            whippedForward = false;
            triggerActive = false;

            var autoAimHit = Physics2D.CircleCastAll(context.transform.position, context.whipAutoAimRadius, context.InputDirection)
                .Where(hit => hit.collider.TryGetComponent(out EntityHealth _))
                .Where(hit => hit.collider.gameObject != context.gameObject)
                .FirstOrDefault();

            aimDirection = context.InputDirection != Vector2Int.zero
                ? autoAimHit && (autoAimHit.point - (Vector2)context.transform.position).magnitude > context.whipAutoAimRadius
                    ? (autoAimHit.point - (Vector2)context.transform.position).normalized
                    : context.Input.Movement.Vector.normalized
                : Vector2.right * context.Facing;

            context.whipRopeSim.AddForce(-aimDirection * context.whipBackForce + Vector2.one * 0.01f);

            context.whipThrow.Play(context);
        }

        public override void Update() {

            float duration = context.stateMachine.stateDuration;

            // forward whip
            if (!whippedForward && duration > context.whipForwardDelay) {
                whippedForward = true;
                context.whipRopeSim.AddForce(aimDirection * context.whipForwardForce + Vector2.one * 0.01f);
            }

            if (!triggerActive && duration > context.whipTriggerDelay) {
                triggerActive = true;

                activeWhipTrigger = Instantiate(context.whipTrigger, context.transform.position, Quaternion.identity);
                activeWhipTrigger.OnCollision += context.OnWhipCollision;

                // move trigger with whip
                context.whipRopeSim.OnUpdate += position => {
                    if (activeWhipTrigger != null)
                        activeWhipTrigger.MoveTo(position);
                };
            }

            if (context.Input.Whip.Pressed)
            context.whipRopeSim.Length = Mathf.Lerp(context.minWhipLength, context.maxWhipLength, Mathf.InverseLerp(0, context.maxLengthExtendDuration, duration));

            base.Update();
        }

        public override void Exit() {

            if (activeWhipTrigger != null)
                Destroy(activeWhipTrigger.gameObject);

            base.Exit();
        }
    }

    private class Retracting : State {

        public Retracting(PlayerWhip context) : base(context) { }

        public override void Update() {

            context.RetractWhip(context.whipRetractSpeed);

            base.Update();
        }
    }

    private class Pulling : State {

        public Pulling(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            TimeManager.FreezeTime(context.whipHitFreezeFrame, context);
            CameraEffects.Effects.AddShake(context.hitEnemyShake);
            context.whipHit.Play(context);
        }
        
        public override void Update() {

            context.RetractWhip(context.whipPullSpeed);

            base.Update();
        }

        public override void Exit() {

            context.whipping = null;

            base.Exit();
        }
    }

    private class PullingEnemy : Pulling {

        public PullingEnemy(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.Movement.SetVelocity(y: Mathf.Max(context.Rigidbody.velocity.y, context.enemyGrappleVerticalBoost));
            context.Movement.RefillAirMovement();

            if (context.whipping == null) return;

            context.whipping.DisableMovement();

            // move enemy with whip
            context.whipRopeSim = new(context.whipRopeParameters, context.transform.position, context.whipping.WhippablePosition) {
                Length = ((Vector2)context.transform.position - context.whipping.WhippablePosition).magnitude
            };
            context.whipRopeSim.OnUpdate += position => {
                if (context.whipping as Object != null)
                    context.whipping.WhippablePosition = position;
            };
        }

        public override void Exit() {

            if (context.whipping as Object != null)
                context.whipping.EnableMovement();

            base.Exit();
        }
    }

    private class PullingSelf : Pulling {

        public PullingSelf(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.Movement.RefillAirMovement();

            if (context.whipping == null) return;

            context.Player.Freeze(movement: true);

            // move player with whip
            context.whipRopeSim = new(context.whipRopeParameters, context.whipping.WhippablePosition, context.transform.position) {
                Length = (context.whipping.WhippablePosition - (Vector2)context.transform.position).magnitude
            };

            context.whipRopeSim.OnUpdate += position => {
                if (context.whipping as Object != null)
                    context.Rigidbody.MovePosition(position);
            };
        }

        public override void Exit() {

            context.Player.Freeze(movement: false);

            base.Exit();
        }
    }

    #endregion
}
