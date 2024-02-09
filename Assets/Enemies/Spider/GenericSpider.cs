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

        [SerializeField] private float legLength;
        [SerializeField] private Vector2 offset;

        private Vector2 footPosition;

        public void Update(Vector2 origin, Vector2 down) {

            Vector2 jointPosition = origin + (Vector2)(Quaternion.FromToRotation(Vector2.down, down) * offset);

            if ((footPosition - jointPosition).magnitude > legLength) {

                var hit = Physics2D.Raycast(jointPosition, down, legLength, GameInfo.GroundMask);

                if (hit) footPosition = hit.point;
                else footPosition = jointPosition;
            }

            line.positionCount = 3;
            line.SetPositions(new Vector3[] { origin, jointPosition, footPosition });
        }
    }

    private void OnValidate() {
        foreach (var leg in legs) leg.Update(Position, Vector2.down);
    }

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    private Vector2 currentForward, currentDown;

    protected override IEnumerator Behaviour() {

        yield return Crawl(Mathf.Infinity);
    }

    private IEnumerator Crawl(float duration) {

        for (float timer = 0; timer < duration; timer += Time.deltaTime) {

            float startAngle = Mathf.Atan2(currentForward.y, currentForward.x);

            List<RaycastHit2D> groundHits = new();
            for (int i = 0; i < floorDetectWhiskers; i++) {

                float angle = (float)i / floorDetectWhiskers * 360f * Mathf.Deg2Rad + startAngle;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

                var hit = Physics2D.Raycast(Position + direction * collider.radius, direction, floorDetectDist, GameInfo.GroundMask);
                if (hit) groundHits.Add(hit);

                Debug.DrawLine(
                    Position + direction * collider.radius,
                    hit ? hit.point : Position + direction * (collider.radius + floorDetectDist),
                    hit ? Color.green : Color.Lerp(Color.red, Color.blue, (float)i / floorDetectWhiskers));
            }

            RaycastHit2D closest = default;
            float closestDist = Mathf.Infinity;

            foreach (var hit in groundHits) {

                float dist = (Position - hit.point).magnitude;

                if (dist < closestDist) {
                    closestDist = dist;
                    closest = hit;
                }
            }

            if (closest) {

                Debug.DrawLine(Position, closest.point, Color.white);

                Vector2 up = closest.normal;
                currentForward = Vector2.Perpendicular(up);
                currentDown = -up;
                Velocity = currentForward * crawlSpeed + (closest.point - Position).normalized * floorSuctionForce;
            }

            transform.localEulerAngles = Vector3.forward * Mathf.Atan2(currentForward.y, currentForward.x) * Mathf.Rad2Deg;

            foreach (var leg in legs) leg.Update(Position, currentDown);

            yield return null;
        }
    }
}
