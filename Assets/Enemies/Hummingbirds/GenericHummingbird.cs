using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverUtils;

public abstract class GenericHummingbird : GenericEnemy {

    [Header("Damage")]
    [SerializeField] private float attackKnockback;
    [SerializeField] private float damage, hurtKnockback, damageFlashDur;

    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Wandering")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float minWanderDist, maxWanderDist;
    [SerializeField] private int minWanderMoves, maxWanderMoves;

    [Header("Visuals")]
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private Transform visualsPivot;

    private Transform target;

    private bool attacking;

    protected Vector2 TargetPosition => target.position;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    protected override void Start() {

        base.Start();

        target = FindObjectOfType<Player>().transform;

        hoverOscillation.offset = Random.value;
    }

    protected virtual void Update() {

        // hover effect
        if (BehaviourActive)
            visualsPivot.localPosition = Vector2.up * hoverOscillation.Evaluate();
    }

    protected abstract IEnumerator Attack();

    protected override IEnumerator Behaviour() {

        while (true) {

            yield return Idle();

            int wanderMoves = Random.Range(minWanderMoves, maxWanderMoves);

            while (wanderMoves > 0) {

                wanderMoves--;

                yield return Wander();
                yield return Idle();
            }

            attacking = true;
            yield return Attack();
            attacking = false;
        }
    }

    protected IEnumerator Idle() {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0) {

            idleTime -= Time.deltaTime;

            // move velocity to zero if bumped
            Velocity = Vector2.MoveTowards(Velocity, Vector2.zero, airFriction * Time.deltaTime);

            yield return null;
        }
    }

    protected IEnumerator Wander() {

        float wanderAngle = Random.value * 360f,
              wanderDistance = Random.Range(minWanderDist, maxWanderDist);
        Vector2 wanderPosition = Position + wanderAngle.DegToVector() * wanderDistance;

        yield return MoveTo(wanderPosition, wanderSpeed);
    }

    protected IEnumerator MoveTo(Vector2 targetPosition, float speed) {

        Vector2 vectorToTarget = targetPosition - Position;

        Velocity = vectorToTarget.normalized * speed;

        float distanceToTarget = vectorToTarget.magnitude,
              timeToTarget = distanceToTarget / speed;

        yield return new WaitForSeconds(timeToTarget);
    }

    protected override void OnTakeDamage(DamageInfo info) {

        //StartCoroutine(Flash());

        //IEnumerator Flash() {

        //    rend.color = Color.white;
        //    Velocity = Vector2.zero;

        //    yield return new WaitForSeconds(damageFlashDur);

        //    rend.color = color;
        //    Velocity = -info.direction * hurtKnockback;
        //    StartBehaviour();
        //}
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (attacking && collision.TryGetComponent(out EntityHealth entity) && entity.Team != health.Team) 
            entity.TakeDamage(new(damage, Velocity.normalized, Vector2.right * attackKnockback * Mathf.Sign(Velocity.x)));
    }
}
