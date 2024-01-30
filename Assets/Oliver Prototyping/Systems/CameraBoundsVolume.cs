using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraBoundsVolume : MonoBehaviour {

    private static readonly Vector2 roomSize = new(16, 9);

    [Header("Camera Variables")]
    [SerializeField] private float cameraSize;
    [SerializeField] private Vector2 max;
    [SerializeField] private Vector2 min;

    [Header("Gizmos")]
    [SerializeField] private Vector2 snapTo;
    [SerializeField] private Color color;

    private void Reset() {

        cameraSize = 4f;

        max = roomSize * cameraSize / 2f;
        min = roomSize * cameraSize / -2f;

        snapTo = new(1, 1);
        color = Color.green;
    }

    private Rect rect {
        get => new() {
            min = min + (Vector2)transform.position,
            max = max + (Vector2)transform.position,
        };
        set {
            transform.position = value.center;
            min = value.min - (Vector2)transform.position;
            max = value.max - (Vector2)transform.position;
        }
    }

    #region Public Members

    public static List<CameraBoundsVolume> Contains(Vector2 position)
        => volumes.FindAll(v => v.rect.Contains(position));

    public Vector2 Clamp(Vector2 position) {

        Vector2 bounds = (rect.size - roomSize * cameraSize) / 2f,
                center = rect.center,
                min = center - bounds,
                max = center + bounds;

        return new(
            Mathf.Clamp(position.x, min.x, max.x),
            Mathf.Clamp(position.y, min.y, max.y));
    }

    public float Height => roomSize.y * cameraSize;

    #endregion

    #region Volumes Management

    private void OnEnable()  => volumes.Add(this);
    private void OnDisable() => volumes.Remove(this);

    private static readonly List<CameraBoundsVolume> volumes = new();

    #endregion

    #region Gizmos

    private static bool alwaysShowBorder = true;

    private void OnDrawGizmos() {
        if (alwaysShowBorder) DrawBorder();
    }

    private void OnDrawGizmosSelected() {

        if (!alwaysShowBorder) DrawBorder();

        // traversable area by the camera
        Gizmos.color = new(color.r, color.g, color.b, 0.15f);
        Gizmos.DrawWireCube(rect.center, rect.size - roomSize * cameraSize * Vector2.up);
        Gizmos.DrawWireCube(rect.center, rect.size - roomSize * cameraSize * Vector2.right);
    }

    private void DrawBorder() {
        Gizmos.color = color;
        Gizmos.DrawWireCube(rect.center, rect.size);
    }

    #endregion

    #region Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(CameraBoundsVolume)), CanEditMultipleObjects]
    private class CameraBoundsEditor : Editor {

        private const float handleSize = 2f;

        private CameraBoundsVolume Bounds => target as CameraBoundsVolume;

        public override void OnInspectorGUI() {

            static void Toggle(string name, ref bool toggle) {
                if (GUILayout.Button($"{name}     {(toggle ? "ON" : "OFF")}"))
                    toggle = !toggle;
            }

            base.OnInspectorGUI();

            GUI.enabled = false;
            EditorGUILayout.Vector2Field("Size", Bounds.max - Bounds.min);
            GUI.enabled = true;

            Toggle("Always Show Border", ref alwaysShowBorder);
        }

        private void OnSceneGUI() {

            var rect = Bounds.rect;

            Vector2 worldPos = Bounds.transform.position,
                    snapTo   = Bounds.snapTo,
                    camSize  = roomSize * Bounds.cameraSize;

            Handles.color = Bounds.color;

            Vector2 DrawHandle(Vector2 position, Vector2 minimum, Vector2 maximum, Vector2 anchor) {

                position = (Vector2)Handles.FreeMoveHandle(position, handleSize, Vector3.one, Handles.SphereHandleCap);

                Vector2 Snap(Vector2 position) => new(
                    Mathf.Round(position.x / snapTo.x) * snapTo.x,
                    Mathf.Round(position.y / snapTo.y) * snapTo.y);

                Vector2 Clamp(Vector2 position) => Vector2.Max(Vector2.Min(position, maximum), minimum);

                return Snap(Clamp(position) - anchor) + anchor;
            }

            EditorGUI.BeginChangeCheck();

            // bottom left
            rect.min = DrawHandle(
                position: rect.min,
                minimum:  Vector2.negativeInfinity,
                maximum:  rect.max - camSize,
                anchor:   rect.max);

            // left
            rect.xMin = DrawHandle(
                position: new(rect.xMin, rect.center.y),
                minimum:  new(Mathf.NegativeInfinity, 0),
                maximum:  new(rect.xMax - camSize.x, 0),
                anchor:   new(rect.xMax, rect.center.y)).x;

            // top left
            Vector2 topLeft = DrawHandle(
                position: new(rect.xMin, rect.yMax),
                minimum:  new(Mathf.NegativeInfinity, rect.yMin + camSize.y),
                maximum:  new(rect.xMax - camSize.x, Mathf.Infinity),
                anchor:   new(rect.xMax, rect.yMin));
            (rect.xMin, rect.yMax) = (topLeft.x, topLeft.y);

            // top
            rect.yMax = DrawHandle(
                position: new(rect.center.x, rect.yMax),
                minimum:  new(0, rect.yMin + camSize.y),
                maximum:  new(0, Mathf.Infinity),
                anchor:   new(rect.center.x, rect.yMin)).y;

            // top right
            rect.max = DrawHandle(
                position: rect.max,
                minimum:  rect.min + camSize,
                maximum:  Vector2.positiveInfinity,
                anchor:   rect.min);

            // right
            rect.xMax = DrawHandle(
                position: new(rect.xMax, rect.center.y),
                minimum:  new(rect.xMin + camSize.x, 0),
                maximum:  new(Mathf.Infinity, 0),
                anchor:   new(rect.xMin, rect.center.y)).x;

            // bottom right
            Vector2 bottomRight = DrawHandle(
                position: new(rect.xMax, rect.yMin),
                minimum:  new(rect.xMin + camSize.x, Mathf.NegativeInfinity),
                maximum:  new(Mathf.Infinity, rect.yMax - camSize.y),
                anchor:   new(rect.xMin, rect.yMax));
            (rect.xMax, rect.yMin) = (bottomRight.x, bottomRight.y);

            // bottom
            rect.yMin = DrawHandle(
                position: new(rect.center.x, rect.yMin),
                minimum:  new(0, Mathf.NegativeInfinity),
                maximum:  new(0, rect.yMax - camSize.y),
                anchor:   new(rect.center.x, rect.yMax)).y;

            if (EditorGUI.EndChangeCheck() || rect != Bounds.rect) {
                Undo.RecordObjects(new Object[] { Bounds, Bounds.transform }, "Room position and size adjustment.");
                Bounds.rect = rect;
                EditorUtility.SetDirty(Bounds);
            }
        }
    }

#endif
    #endregion
}
