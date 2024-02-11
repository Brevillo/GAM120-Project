using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSpider : GenericEnemy {

    [SerializeField] private float crawlSpeed, floorSuctionForce;
    [SerializeField] private float floorDetectDist;
    [SerializeField] private int floorDetectWhiskers;
    [SerializeField] private new CircleCollider2D collider;
    [SerializeField] private Leg[] legs;

    [System.Serializable]
    private class Leg {

        [SerializeField] private LineRenderer line;

        [SerializeField] private float legLength, legSpeed;
        [SerializeField] private Vector2 offset;

        private Vector2 footTarget, footPosition;

        public void Update(Vector2 origin, Vector2 down) {

            Vector2 jointPosition = origin + (Vector2)(Quaternion.FromToRotation(Vector2.down, down) * offset);

            if ((footTarget - jointPosition).magnitude > legLength) {

                var hit = Physics2D.Raycast(jointPosition, down, legLength, GameInfo.GroundMask);

                if (hit) footTarget = hit.point;
                else footTarget = jointPosition;
            }

            footPosition = Vector2.MoveTowards(footPosition, footTarget, legSpeed * Time.deltaTime);

            line.positionCount = 3;
            line.SetPositions(new Vector3[] { origin, jointPosition, footPosition });
        }
    }

    private void OnDrawGizmos() {
        if (!Application.IsPlaying(this))
            foreach (var leg in legs) leg.Update(Position, Vector2.down);
    }

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    private Vector2 currentForward, currentDown;

    protected override IEnumerator Behaviour() {

        yield return Crawl(Mathf.Infinity);
    }

    private List<RaycastHit2D> GetCollisions(float startAngle, float range, bool debug) {

        List<RaycastHit2D> hits = new();
        for (int i = 0; i < floorDetectWhiskers; i++) {

            float angle = startAngle + (float)i / floorDetectWhiskers * range * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            var hit = Physics2D.Raycast(Position + direction * collider.radius, direction, floorDetectDist, GameInfo.GroundMask);
            if (hit) hits.Add(hit);

            if (debug) Debug.DrawLine(
                Position + direction * collider.radius,
                hit ? hit.point : Position + direction * (collider.radius + floorDetectDist),
                hit ? Color.green : Color.Lerp(Color.red, Color.blue, (float)i / floorDetectWhiskers));
        }

        return hits;
    }

    private IEnumerator Crawl(float duration) {

        for (float timer = 0; timer < duration; timer += Time.deltaTime) {

            RaycastHit2D closest = default;
            float closestDist = Mathf.Infinity;

            foreach (var hit in GetCollisions(0f, 360f, false)) {

                float dist = (Position - hit.point).magnitude;

                if (dist < closestDist) {
                    closestDist = dist;
                    closest = hit;
                }
            }

            if (closest) {

                Vector2 toClosest = closest.point - Position,
                        forward = -Vector2.Perpendicular(toClosest);

                var groundHits = GetCollisions(Mathf.Atan2(forward.y, forward.x), 180f, true);

                Vector2 averageNormal = Vector2.zero;
                foreach (var hit in groundHits) averageNormal += hit.normal;
                averageNormal = (averageNormal / groundHits.Count).normalized;

                currentForward = Vector2.Perpendicular(averageNormal);
                currentDown = -averageNormal;
                Velocity = currentForward * crawlSpeed + toClosest.normalized * floorSuctionForce;
            }

            transform.up = -currentDown;

            foreach (var leg in legs) leg.Update(Position, currentDown);

            yield return null;
        }
    }
}
