/* Made by Oliver Beebe 2024 */
using UnityEngine;

public class VerletRope {

    private readonly Parameters parameters;
    private readonly float segmentLength;
    private readonly Vector2[] points, prevPoints;

    public event System.Action<Vector2> OnUpdate;

    [System.Serializable]
    public class Parameters {

        [Header("Physics")]
        [Tooltip("How heavy the rope is")]
        public float gravity = 50.0f;
        [Tooltip("How jittery is the rope (0 - 1)")]
        public float dampening = 0.99f;
        [Tooltip("How stiff the rope is\n(high values can have severe performance impact)")]
        public int iterations = 300;

        [Header("Rope")]
        [Tooltip("How long the rope is. Can be stretche beyond this.")]
        public float length = 10f;
        [Tooltip("How many points along the rope.")]
        public int pointCount = 20;
    }

    public VerletRope(Parameters parameters, Vector2 start) {

        this.parameters = parameters;
        segmentLength = parameters.length / parameters.pointCount;

        points = new Vector2[parameters.pointCount];
        prevPoints = new Vector2[parameters.pointCount];

        System.Array.Fill(points, start);
        System.Array.Fill(prevPoints, start);
    }

    // constraint equations from https://toqoz.fyi/game-rope.html

    public void AddForce(Vector2 force) {
        for (int i = 0; i < parameters.pointCount; i++)
            points[i] += force;
    }

    public Vector2[] Update(Vector2 start) {

        // simulate

        float deltaTimeSqr = Time.fixedDeltaTime * Time.fixedDeltaTime;
        for (int i = 0; i < parameters.pointCount; i++) {

            Vector2 prev = points[i];

            points[i] += (points[i] - prevPoints[i]) * parameters.dampening;
            points[i].y -= parameters.gravity * deltaTimeSqr;

            prevPoints[i] = prev;
        }

        // constrain

        for (int iteration = 0; iteration < parameters.iterations; iteration++) {

            points[0] = start;

            for (int i = 1; i < parameters.pointCount; i++) {

                Vector2 vector     = points[i - 1] - points[i];
                float   distance   = vector.magnitude,
                        difference = distance == 0 ? 0 : (segmentLength - distance) / distance;
                Vector2 adjustment = vector * difference / 2f;

                points[i - 1] += adjustment;
                points[i] -= adjustment;
            }
        }

        // invoke OnUpdate with last point
        OnUpdate?.Invoke(points[^1]);

        return points;
    }

    public void ApplyToLineRenderer(LineRenderer line) {
        line.positionCount = parameters.pointCount;
        line.SetPositions(System.Array.ConvertAll(points, v => (Vector3)v));
    }
}
