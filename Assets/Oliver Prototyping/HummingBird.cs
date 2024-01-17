using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingBird : MonoBehaviour, IWhippable {

    [SerializeField] private float minIdleTime, maxIdleTime, minWanderDist, maxWanderDist, wanderSpeed, diveSpeed, maxDiveDist, airFriction, damageFlashDur;
    [SerializeField] private int minWanderMoves, maxWanderMoves;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private EntityTimeScale timeScale;
    [SerializeField] private EntityHealth health;

    private Transform target;
    private Coroutine behaviour;

    public IWhippable.Type type => IWhippable.Type.Light;
    public Vector2 Position => transform.position;

    private Vector2 position => transform.position;
    private Vector2 velocity {
        get => rigidbody.velocity / timeScale;
        set => rigidbody.velocity = value * timeScale;
    }

    private void Start() {

        target = FindObjectOfType<PlayerMovement>().transform;

        behaviour = StartCoroutine(Behaviour());

        health.OnTakeDamage += OnTakeDamage;
    }

    #region Movement

    private IEnumerator Behaviour() {

        while (true) {

            yield return Idle();

            int wanderMoves = Random.Range(minWanderMoves, maxWanderMoves);

            while (wanderMoves > 0) {

                wanderMoves--;

                yield return Wander();
                yield return Idle();
            }

            yield return Dive();
        }
    }

    private IEnumerator Idle() {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0) {

            idleTime -= Time.deltaTime * timeScale;

            // move velocity to zero if bumped
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, airFriction * Time.deltaTime * timeScale);

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

    public void DisableMovement() {
        StopCoroutine(behaviour);
    }

    public void EnableMovement() {
        behaviour = StartCoroutine(Behaviour());
    }

    public void MoveTo(Vector2 position) {
        rigidbody.MovePosition(position);
    }

    #endregion

    #region Health

    private void OnTakeDamage(DamageInfo info) {

        StartCoroutine(Flash());

        IEnumerator Flash() {

            rend.color = Color.red;

            yield return new WaitForSeconds(damageFlashDur * timeScale);

            rend.color = Color.white;
        }
    }

    #endregion
}
