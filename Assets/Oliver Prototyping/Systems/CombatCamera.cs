using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCamera : MonoBehaviour {

    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float lookSpeed, followSpeed;

    private Vector2 positionVel;
    private Vector3 rotationVel;

    private void LateUpdate() {

        transform.position
            = (Vector3)Vector2.SmoothDamp(transform.position, cameraTarget.position, ref positionVel, followSpeed)
            + Vector3.forward * transform.position.z;

        transform.eulerAngles = SmoothDampAngle(
            current:    transform.eulerAngles,
            target:     Quaternion.LookRotation(cameraTarget.position - transform.position).eulerAngles,
            velocity:   ref rotationVel,
            smoothTime: Vector3.one * lookSpeed);
    }

    private Vector3 SmoothDampAngle(Vector3 current, Vector3 target, ref Vector3 velocity, Vector3 smoothTime) => new(
        Mathf.SmoothDampAngle(current.x, target.x, ref velocity.x, smoothTime.x),
        Mathf.SmoothDampAngle(current.y, target.y, ref velocity.y, smoothTime.y),
        Mathf.SmoothDampAngle(current.z, target.z, ref velocity.z, smoothTime.z));
}
