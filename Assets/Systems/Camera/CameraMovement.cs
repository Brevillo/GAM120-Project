using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverUtils;

public class CameraMovement : MonoBehaviour {

    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private Camera cam;

    [SerializeField] private Vector2 defaultCameraSize;

    private void Awake() {
        I = this;
    }

    // instance
    private static CameraMovement I;

    public static void CombatLock(CombatBound bound) => I.combatBound = bound;
    public static void CombatUnlock() => I.combatBound = null;

    private CombatBound combatBound;
    private Vector2 velocity;

    private void LateUpdate() {

        Vector2 focus = target.position,
                size  = defaultCameraSize;

        CameraBound bound
            = combatBound != null                                 ? combatBound
            : RoomBound.Contains(focus).TryIndex(0, out var room) ? room
            : null;

        if (bound != null) {
            focus = bound.Clamp(focus);
            size = bound.CameraSize;
        }

        transform.position

            // move towards focus point
            = (Vector3)Vector2.SmoothDamp(transform.position, focus, ref velocity, smoothSpeed)

            // move backwards based on camera size and field of view
            + Vector3.back * size.y / 2f * Mathf.Tan((90f - cam.fieldOfView / 2f) * Mathf.Deg2Rad);
    }
}
