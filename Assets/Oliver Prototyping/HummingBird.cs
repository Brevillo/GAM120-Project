using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingBird : MonoBehaviour, IWhippable {

    [SerializeField] private float minIdleTime, maxIdleTime, minWanderDist, maxWanderDist, wanderSpeed, diveSpeed, maxDiveDist, airFriction;
    [SerializeField] private int minWanderMoves, maxWanderMoves;
    [SerializeField] private new Rigidbody2D rigidbody;

    private Transform target;
    private Coroutine behaviour;

    public IWhippable.Type type => IWhippable.Type.Light;

    private void Start() {

        target = FindObjectOfType<PlayerMovement>().transform;

        behaviour = StartCoroutine(Behaviour());
    }

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

            idleTime -= Time.deltaTime;

            // move velocity to zero if bumped
            rigidbody.velocity = Vector2.MoveTowards(rigidbody.velocity, Vector2.zero, airFriction * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator Wander() {

        float wanderAngle = Random.value * 360f * Mathf.Deg2Rad,
              wanderDistance = Random.Range(minWanderDist, maxWanderDist);
        Vector2 wanderPosition = (Vector2)transform.position + new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderDistance;

        yield return MoveTo(wanderPosition, wanderSpeed);
    }

    private IEnumerator Dive() {

        Vector2 divePosition = Vector2.ClampMagnitude(target.position - transform.position, maxDiveDist) + (Vector2)transform.position;

        yield return MoveTo(divePosition, diveSpeed);
    }

    private IEnumerator MoveTo(Vector2 position, float speed) {

        Vector2 vector = position - (Vector2)transform.position;

        rigidbody.velocity = vector.normalized * speed;

        float distance = vector.magnitude,
              time = distance / speed;

        yield return new WaitForSeconds(time);
    }

    public void DisableMovement() {
        StopCoroutine(behaviour);
    }

    public void EnableMovement() {
        behaviour = StartCoroutine(Behaviour());
    }

    public void MoveTo(Vector2 position) {
        rigidbody.MovePosition(position);
    }
}
