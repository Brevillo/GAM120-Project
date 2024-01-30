using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private Camera cam;

    [SerializeField] private float defaultCameraHeight;

    public bool locked;

    private Vector2 velocity;

    private void LateUpdate() {

        Vector2 position = transform.position;
        float height = defaultCameraHeight;

        position = Vector2.SmoothDamp(position, target.position, ref velocity, smoothSpeed);

        var volumes = CameraBoundsVolume.Contains(target.position);

        if (volumes.Count > 0) {
            position = volumes[0].Clamp(position);
            height = volumes[0].Height;
        }

        float zOffset = height / 2f * Mathf.Tan((90f - cam.fieldOfView / 2f) * Mathf.Deg2Rad);
        transform.position = (Vector3)position + Vector3.back * zOffset;
    }
}
