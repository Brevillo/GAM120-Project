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
    [SerializeField] private float minStartFlightVel, maxFlightVel, maxFlightStamina, timeAfterJumpingBeforeFlight, dontFlyAboveGroundDist;
    [SerializeField] private Transform wing1, wing2;

    [Header("Whipping")]
    [SerializeField] private float whipExtendSpeed;
    [SerializeField] private float whipExtendDist, whipRetractSpeed, whipPullSpeed, whipMaxLength;
    [SerializeField] private VerletRope.Parameters whipRopeParameters;
    [SerializeField] private LineRenderer whipRenderer;
    [SerializeField] private PlayerWhipTrigger whipTrigger;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private InputManager inputManager;

    #endregion

    #region Variables

    private StateMachine<PlayerMovement>
        stateMachine,
        whipStateMachine;

    private Vector2Int inputDir;               // current movement input
    private bool jumpBuffered;              // is jump buffered?

    private Vector2 velocity;               // current velocity (stored so that I can edit the x and y components individually)
    private RaycastHit2D groundHit;         // raycast hit for the ground
    private bool onGround;                  // is the player on the ground?
    private float groundDist;               // distance to the ground

    private float remainingFlightStamina;   // how much flight stamina reminas

    private Vector2 whipPosition;           // end point of the whip

    private IWhippable whipping;
    private VerletRope whipRope;

    #endregion

    #region Awake and Update

    private void Awake() {
        InitializeStateMachine();
        InitializeWhipStateMachine();
    }

    private void Update() {

        // debug helpers

        if (inputManager.Debug1.Down) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // input

        inputDir = new(
            Mathf.RoundToInt(inputManager.Movement.Vector.x),
            Mathf.RoundToInt(inputManager.Movement.Vector.y));
        jumpBuffered = jumpBuffer.Buffer(inputManager.Jump.Down);

        // get information about current physical state

        velocity = rigidbody.velocity;
        groundHit = Physics2D.BoxCast(transform.position, col.size, 0, Vector2.down, groundDetectDist, groundMask);
        onGround = groundHit && groundHit.normal == Vector2.up;
        var groundDistHit = Physics2D.BoxCast(transform.position, col.size, 0, Vector2.down, Mathf.Infinity, groundMask);
        groundDist = groundDistHit ? transform.position.y - col.bounds.extents.y - groundDistHit.point.y : Mathf.Infinity;

        // run state machines

        stateMachine.Update(Time.deltaTime);
        whipStateMachine.Update(Time.deltaTime);

        // apply velocity

        rigidbody.velocity = velocity;
    }

    private void FixedUpdate() {

        whipRope?.Update(transform.position);

        whipRope?.ApplyToLineRenderer(whipRenderer);
    }

    #endregion

    #region Helper Functions

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

    private void UpdateWhip() {
        whipRenderer.useWorldSpace = true;
        whipRenderer.positionCount = 2;
        whipRenderer.SetPositions(new Vector3[] { transform.position, whipPosition });
    }

    private void OnWhipCollision(Collider2D collision) {
        if (collision.TryGetComponent(out IWhippable whippable))
            whipping = whippable;
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
            endJump     = () => !inputManager.Jump.Pressed || velocity.y <= 0,

            toFalling   = () => !onGround,

            toFlight    = () => remainingFlightStamina > 0
                                && !onGround
                                && ((inputManager.Jump.Pressed && !jumpBuffered) || (groundDist > dontFlyAboveGroundDist && jumpBuffered))
                                && (stateMachine.previousState != jumping || stateMachine.stateDuration > timeAfterJumpingBeforeFlight),
            endFlying   = () => !inputManager.Jump.Pressed || remainingFlightStamina <= 0;

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

    #region Whip State Machine

    // whip state instances
    private WhipIdle            whipIdle;
    private WhipExtending       whipExtending;
    private WhipRetracting      whipRetracting;
    private WhipPullingEnemy    whipPullingEnemy;

    private void InitializeWhipStateMachine() {

        whipIdle            = new(this);
        whipExtending       = new(this);
        whipRetracting      = new(this);
        whipPullingEnemy    = new(this);

        TransitionDelegate

            startWhip       = () => inputManager.Whip.Down && inputDir != Vector2Int.zero,
            stopWhip        = () => !inputManager.Whip.Pressed || whipExtending.reachedTarget,

            whipRetracted   = () => whipPosition == (Vector2)transform.position,

            pullEnemy       = () => whipping != null && whipping.type == IWhippable.Type.Light;

        whipStateMachine = new(

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
                    new(whipIdle, whipRetracted)
                } },
            }
        );
    }

    private class WhipIdle : State {

        public WhipIdle(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.whipRenderer.enabled = false;
        }
    }

    private class WhipExtending : State {

        public WhipExtending(PlayerMovement context) : base(context) { }

        private Vector2 aimDirection, targetPosition;
        private PlayerWhipTrigger activeWhipTrigger;

        public bool reachedTarget => context.whipPosition == targetPosition;

        public override void Enter() {

            base.Enter();

            context.whipPosition = context.transform.position;
            aimDirection = ((Vector2)context.inputDir).normalized;
            targetPosition = context.whipPosition + aimDirection * context.whipMaxLength;

            activeWhipTrigger = Instantiate(context.whipTrigger);
            activeWhipTrigger.OnCollision += context.OnWhipCollision;

            context.whipRenderer.enabled = true;

            context.whipRope = new(context.whipRopeParameters, (Vector2)context.transform.position + aimDirection * 0.5f);
            context.whipRope.OnUpdate += point => {
                context.whipPosition = point;
                activeWhipTrigger.MoveTo(point);
            };
            context.whipRope.AddForce(aimDirection * context.whipExtendSpeed + Vector2.one * 0.01f);
        }

        public override void Update() {

            //context.whipPosition = Vector2.MoveTowards(context.whipPosition, targetPosition, context.whipExtendSpeed * Time.deltaTime);
            //context.UpdateWhip();

            base.Update();
        }

        public override void Exit() {

            Destroy(activeWhipTrigger.gameObject);
            context.whipRope = null;

            base.Exit();
        }
    }

    private class WhipRetracting : State {

        public WhipRetracting(PlayerMovement context) : base(context) { }

        public override void Update() {

            context.whipPosition = Vector2.MoveTowards(context.whipPosition, context.transform.position, context.whipRetractSpeed * Time.deltaTime);
            context.UpdateWhip();

            base.Update();
        }
    }

    private class WhipPullingEnemy : State {

        public WhipPullingEnemy(PlayerMovement context) : base(context) { }

        public override void Enter() {

            base.Enter();

            context.whipping.DisableMovement();
        }

        public override void Update() {

            context.whipPosition = Vector2.MoveTowards(context.whipPosition, context.transform.position, context.whipPullSpeed * Time.deltaTime);
            context.whipping.MoveTo(context.whipPosition);
            context.UpdateWhip();

            base.Update();
        }

        public override void Exit() {

            context.whipping.EnableMovement();
            context.whipping = null;

            base.Exit();
        }
    }

    #endregion
}
