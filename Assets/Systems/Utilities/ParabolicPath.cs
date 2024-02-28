using UnityEngine;

// demonstration https://www.desmos.com/calculator/ncftcv8ydo

/// <summary> A parabolic path between two points with a specific gravity </summary>
public readonly struct ParabolicPath {

    private readonly Vector2 start, end;
    private readonly float speed, a, b, c;

    /// <summary> Constructs a new parabolic path from start to end, with gravity. </summary>
    /// <param name="startPoint"> Starting point of the path. </param>
    /// <param name="endPoint"> Ending point of the path. </param>
    /// <param name="gravity"> Gravity or shape of the path. </param>
    public ParabolicPath(Vector2 startPoint, Vector2 endPoint, float gravity, float horizontalSpeed) {

        start = startPoint;
        end = endPoint;
        a = gravity / (2f * horizontalSpeed * horizontalSpeed);
        speed = horizontalSpeed;

        b = (a * (start.x * start.x - end.x * end.x) + end.y - start.y) / (end.x - start.x);
        c = start.y - a * start.x * start.x - b * start.x;

        peakPoint = new Vector2(0.5f * -b / a, c - 0.25f * b * b / a);
        maxHeight = peakPoint.y - start.y;
    }

    /// <summary> Point of the peak of the path. </summary>
    public readonly Vector2 peakPoint;
    /// <summary> Max height of the path starting from the start position. </summary>
    public readonly float maxHeight;

    /// <summary> Get the velocity of the curve at x </summary>
    public float GetVelocity(float x) => 2f * a * x * speed + b * speed;

    /// <summary> Is the parabolic path finished at x? </summary>
    public bool IsFinished(float x) => x >= end.x;

    const float delta = 0.01f;
    private float Y(float x) => a * x * x + b * x + c;
    /// <summary> Render the parabolic path in color. </summary>
    public void RenderPath(Color color) {

        float left = Mathf.Min(start.x, end.x),
              right = Mathf.Max(start.x, end.x);

        for (float x = left; x < right; x += delta)
            Debug.DrawLine(new(x, Y(x)), new(x + delta, Y(x + delta)), color);

        Debug.DrawLine(peakPoint, new(peakPoint.x, start.y), color);
    }
}
