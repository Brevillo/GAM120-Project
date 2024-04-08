using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverUtils;

public abstract class GenericHummingbird : GenericEnemyBehaviour {

    [Header("Damage")]
    [SerializeField] private float damage;
    [SerializeField] private float attackKnockback, damageFlashDur;

    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Wandering")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float minWanderDist, maxWanderDist;
    [SerializeField] private int minWanderMoves, maxWanderMoves;

    [SerializeField] private new HummingbirdAnimation animation;

    private Transform target;

    private bool attacking;

    protected Vector2 TargetPosition => target.position;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    protected virtual void Start() {

        target = FindObjectOfType<Player>().transform;

        animation.target = target;
    }

    protected override void StopBehaviour() {
        attacking = false;
        base.StopBehaviour();
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
            animation.turnToTarget = false;
            yield return Attack();
            animation.turnToTarget = true;
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

    private void OnTriggerEnter2D(Collider2D collision) {

        if (attacking && collision.TryGetComponent(out EntityHealth entity) && entity.Team != Health.Team) 
            entity.TakeDamage(new(
                damageAmount: damage,
                direction: Velocity.normalized,
                knockbackPercent: Vector2.right * attackKnockback * Mathf.Sign(Velocity.x)));
    }
}
