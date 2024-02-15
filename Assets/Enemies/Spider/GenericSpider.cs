using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericSpider : GenericEnemyBehaviour {

    [Header("Damage")]
    [SerializeField] private float touchingDamageAmount;
    [SerializeField] private float attackKnockback;

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    protected bool attacking;

    [System.Serializable]
    protected class CrawlParameters {

        public float crawlSpeed, floorSuctionForce, floorDetectDist;
        public int floorDetectWhiskers;
        public CircleCollider2D collider;
    }

    protected IEnumerator Crawl(float duration, int direction, CrawlParameters parameters) {

        List<RaycastHit2D> GetCollisions(float startAngle, float range, bool debug) {

            List<RaycastHit2D> hits = new();
            for (int i = 0; i < parameters.floorDetectWhiskers; i++) {

                float angle = startAngle + (float)i / parameters.floorDetectWhiskers * range * Mathf.Deg2Rad;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

                var hit = Physics2D.Raycast(Position + direction * parameters.collider.radius, direction, parameters.floorDetectDist, GameInfo.GroundMask);
                if (hit) hits.Add(hit);
            }

            return hits;
        }

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
                        forward = -Vector2.Perpendicular(toClosest) * direction;

                var groundHits = GetCollisions(Mathf.Atan2(forward.y, forward.x), 180f * direction, true);

                Vector2 averageNormal = Vector2.zero;
                foreach (var hit in groundHits) averageNormal += hit.normal;
                averageNormal = (averageNormal / groundHits.Count).normalized;

                Vector2 currentForward = Vector2.Perpendicular(averageNormal) * direction;
                Vector2 currentDown = -averageNormal;

                Velocity = currentForward * parameters.crawlSpeed + toClosest.normalized * parameters.floorSuctionForce;
                transform.up = -currentDown;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (attacking && collision.TryGetComponent(out EntityHealth entity) && entity.Team != Health.Team)
            entity.TakeDamage(new(
                damageAmount: touchingDamageAmount,
                direction: Velocity.normalized,
                knockbackPercent: Vector2.right * attackKnockback * Mathf.Sign(Velocity.x)));
    }
}
