using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

public class PlayerWhip : Player.Component {

    #region Parameters

    [SerializeField] private float whipBackForce;
    [SerializeField] private float whipForwardForce, whipForwardDelay, whipTriggerDelay, whipRetractSpeed, whipPullSpeed, whipMaxLength, enemyGrappleVerticalBoost;
    [SerializeField] private VerletRope.Parameters whipRopeParameters;
    [SerializeField] private LineRenderer whipRenderer;
    [SerializeField] private PlayerWhipTrigger whipTrigger;
    [SerializeField] private float whipHitFreezeFrame;
    [SerializeField] private CameraShakeProfile hitEnemyShake;

    #endregion

    #region Variables

    private IWhippable whipping;            // currently whipped thing
    private VerletRope whipRopeSim;         // current whip simulation

    private StateMachine<PlayerWhip> stateMachine;

    // state instances
    private WhipIdle whipIdle;
    private WhipExtending whipExtending;
    private WhipRetracting whipRetracting;
    private WhipPullingEnemy whipPullingEnemy;

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
        whipRopeSim?.Update(transform.position);
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

    private void InitializeWhipStateMachine() {

        whipIdle            = new(this);
        whipExtending       = new(this);
        whipRetracting      = new(this);
        whipPullingEnemy    = new(this);

        TransitionDelegate

            startWhip       = () => Input.Whip.Down,
            stopWhip        = () => !Input.Whip.Pressed,

            whipRetracted   = () => whipRopeSim.Length == 0,

            pullEnemy       = () => whipping != null && whipping.WhippableType == IWhippable.Type.Light,
            stopPullEnemy   = () => whipping == null;

        stateMachine = new(

            firstState: whipIdle,

            transitions: new() {

                { whipIdle, new() {
                    new(whipExtending, startWhip),
                } },

                { whipExtending, new() {
                    new(whipRetracting, stopWhip),
                    new(whipPullingEnemy, pullEnemy),
                } },

                { whipRetracting, new() {
                    new(whipIdle, whipRetracted),
                } },

                { whipPullingEnemy, new() {
                    new(whipIdle, whipRetracted),
                    new(whipRetracting, stopPullEnemy),
                } },
            }
        );
    }

    private class WhipIdle : State {

        public WhipIdle(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.whipRopeSim = null;
            context.whipRenderer.enabled = false;
        }
    }

    private class WhipExtending : State {

        public WhipExtending(PlayerWhip context) : base(context) { }

        private Vector2 aimDirection => context.InputDirection != Vector2Int.zero
            ? context.Input.Movement.Vector.normalized
            : Vector2.right * context.Facing;

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

            context.whipRopeSim.AddForce(-aimDirection * context.whipBackForce + Vector2.one * 0.01f);
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

            base.Update();
        }

        public override void Exit() {

            if (activeWhipTrigger != null)
                Destroy(activeWhipTrigger.gameObject);

            base.Exit();
        }
    }

    private class WhipRetracting : State {

        public WhipRetracting(PlayerWhip context) : base(context) { }

        public override void Update() {

            context.RetractWhip(context.whipRetractSpeed);

            base.Update();
        }
    }

    private class WhipPullingEnemy : State {

        public WhipPullingEnemy(PlayerWhip context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.whipping.DisableMovement();

            context.Movement.SetVelocity(y: Mathf.Max(context.Rigidbody.velocity.y, context.enemyGrappleVerticalBoost));

            TimeManager.FreezeTime(context.whipHitFreezeFrame, context);
            CameraShake.AddShake(context.hitEnemyShake);

            // move enemy with whip
            context.whipRopeSim = new(context.whipRopeParameters, context.transform.position, context.whipping.WhippablePosition) {
                Length = ((Vector2)context.transform.position - context.whipping.WhippablePosition).magnitude
            };
            context.whipRopeSim.OnUpdate += position => {
                if (context.whipping != null)
                    context.whipping.MoveTo(position);
            };
        }

        public override void Update() {

            context.RetractWhip(context.whipPullSpeed);

            base.Update();
        }

        public override void Exit() {

            if (context.whipping != null)
                context.whipping.EnableMovement();
            context.whipping = null;

            base.Exit();
        }
    }

    #endregion
}
