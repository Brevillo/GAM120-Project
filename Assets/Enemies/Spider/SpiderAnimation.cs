using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimation : MonoBehaviour {

    [SerializeField] private float speed;
    [SerializeField] private List<Leg> legs;
    
    [System.Serializable]
    private class Leg {

        [SerializeField] private LineRenderer line;

        [SerializeField] private float legLength;

        private Vector2 footTarget, footPosition;

        private bool onGround;

        private void Update(Vector2 origin, float speed) {

            Vector2 jointPosition = line.transform.position;

            if ((footTarget - jointPosition).magnitude > legLength || !onGround) {

                var hit = Physics2D.Raycast(jointPosition, -line.transform.up, legLength, GameInfo.GroundMask);

                if (hit) footTarget = hit.point;

                onGround = hit;
            }

            footPosition = Vector2.MoveTowards(footPosition, footTarget, speed * Time.deltaTime);
            footPosition = Vector2.ClampMagnitude(footPosition - jointPosition, legLength) + jointPosition;

            line.positionCount = 3;
            line.SetPositions(new Vector3[] { origin, jointPosition, footPosition });
        }

        public static System.Action<Leg> UpdateAll(Vector2 position, float speed) => leg => leg.Update(position, speed);
    }

    private void Update() {
        legs.ForEach(Leg.UpdateAll(transform.position, speed));
    }
}
