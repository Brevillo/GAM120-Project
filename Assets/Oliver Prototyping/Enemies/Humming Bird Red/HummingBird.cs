using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingBird : MonoBehaviour, IWhippable {

    [SerializeField] private float damage;
    [SerializeField] private float minIdleTime, maxIdleTime, minWanderDist, maxWanderDist, wanderSpeed, diveSpeed, maxDiveDist, airFriction, damageFlashDur, knockbackForce;
    [SerializeField] private int minWanderMoves, maxWanderMoves;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private EntityHealth health;

    private Transform target;
    private Coroutine behaviour;
    private Color color;

    private Vector2 position => transform.position;
    private Vector2 velocity {
        get => rigidbody.velocity;
        set => rigidbody.velocity = value;
    }

    private bool attacking;

    private void Start() {

        target = FindObjectOfType<PlayerMovement>().transform;

        behaviour = StartCoroutine(Behaviour());

        color = rend.color;
        health.OnTakeDamage += OnTakeDamage;
    }

    #region Movement

    public void RestartBehaviour() {
        StopCoroutine(behaviour);
        behaviour = StartCoroutine(Behaviour());
    }

    private IEnumerator Behaviour() {

        while (true) {

            attacking = false;

            yield return Idle();

            int wanderMoves = Random.Range(minWanderMoves, maxWanderMoves);

            while (wanderMoves > 0) {

                wanderMoves--;

                yield return Wander();
                yield return Idle();
            }

            attacking = true;
            yield return Dive();
        }
    }

    private IEnumerator Idle() {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0) {

            idleTime -= Time.deltaTime;

            // move velocity to zero if bumped
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, airFriction * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator Wander() {

        float wanderAngle = Random.value * 360f * Mathf.Deg2Rad,
              wanderDistance = Random.Range(minWanderDist, maxWanderDist);
        Vector2 wanderPosition = position + new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderDistance;

        yield return MoveTo(wanderPosition, wanderSpeed);
    }

    private IEnumerator Dive() {

        Vector2 divePosition = Vector2.ClampMagnitude((Vector2)target.position - position, maxDiveDist) + position;

        yield return MoveTo(divePosition, diveSpeed);
    }

    private IEnumerator MoveTo(Vector2 position, float speed) {

        Vector2 vector = position - this.position;

        velocity = vector.normalized * speed;

        float distance = vector.magnitude,
              time = distance / speed;

        yield return new WaitForSeconds(time);
    }

    #endregion

    #region Whippable

    public IWhippable.Type WhippableType => IWhippable.Type.Light;
    public Vector2 WhippablePosition => transform.position;

    public void DisableMovement() {
        StopCoroutine(behaviour);
    }

    public void EnableMovement() {
        if (this != null)
            behaviour = StartCoroutine(Behaviour());
    }

    public void MoveTo(Vector2 position) {
        if (this != null)
            rigidbody.MovePosition(position);
    }

    #endregion

    #region Health

    private void OnTakeDamage(DamageInfo info) {

        if (health.Health <= 0) {
            Death();
            return;
        }

        StartCoroutine(Flash());

        IEnumerator Flash() {

            rend.color = Color.white;
            velocity = Vector2.zero;

            yield return new WaitForSeconds(damageFlashDur);

            rend.color = color;
            velocity = -info.direction * knockbackForce;
            RestartBehaviour();
        }
    }

    private void Death() {
        Destroy(gameObject);
    }

    #endregion

    #region Attacking

    private void OnTriggerEnter2D(Collider2D collision) {

        if (attacking && collision.TryGetComponent(out EntityHealth entity) && entity.Team != health.Team) 
            entity.TakeDamage(new(damage, velocity.normalized));
    }

    #endregion
}
