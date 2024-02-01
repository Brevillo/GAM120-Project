using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingBird : GenericEnemy {

    [SerializeField] private float damage;
    [SerializeField] private float knockback, minIdleTime, maxIdleTime, minWanderDist, maxWanderDist, wanderSpeed, diveSpeed, maxDiveDist, airFriction, damageFlashDur, knockbackForce;
    [SerializeField] private int minWanderMoves, maxWanderMoves;
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private SpriteRenderer rend;

    private Transform target;
    private Color color;

    private bool attacking;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    protected override void Start() {

        target = FindObjectOfType<Player>().transform;

        color = rend.color;

        hoverOscillation.offset = Random.value;
    }

    private void Update() {

        // hover effect
        if (BehaviourActive)
            rend.transform.localPosition = Vector2.up * hoverOscillation.Evaluate();
    }

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
            yield return Dive();
            attacking = false;
        }
    }

    private IEnumerator Idle() {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0) {

            idleTime -= Time.deltaTime;

            // move velocity to zero if bumped
            Velocity = Vector2.MoveTowards(Velocity, Vector2.zero, airFriction * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator Wander() {

        float wanderAngle = Random.value * 360f * Mathf.Deg2Rad,
              wanderDistance = Random.Range(minWanderDist, maxWanderDist);
        Vector2 wanderPosition = Position + new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderDistance;

        yield return MoveTo(wanderPosition, wanderSpeed);
    }

    private IEnumerator Dive() {

        Vector2 divePosition = Vector2.ClampMagnitude((Vector2)target.position - Position, maxDiveDist) + Position;

        yield return MoveTo(divePosition, diveSpeed);
    }

    private IEnumerator MoveTo(Vector2 targetPosition, float speed) {

        Vector2 vectorToTarget = targetPosition - Position;

        Velocity = vectorToTarget.normalized * speed;

        float distanceToTarget = vectorToTarget.magnitude,
              timeToTarget = distanceToTarget / speed;

        yield return new WaitForSeconds(timeToTarget);
    }

    protected override void OnTakeDamage(DamageInfo info) {

        StartCoroutine(Flash());

        IEnumerator Flash() {

            rend.color = Color.white;
            Velocity = Vector2.zero;

            yield return new WaitForSeconds(damageFlashDur);

            rend.color = color;
            Velocity = -info.direction * knockbackForce;
            StartBehaviour();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (attacking && collision.TryGetComponent(out EntityHealth entity) && entity.Team != health.Team) 
            entity.TakeDamage(new(damage, Velocity.normalized, Vector2.right * knockback * Mathf.Sign(Velocity.x)));
    }
}
