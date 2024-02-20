using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityShadow : MonoBehaviour {

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform shadow;

    private void LateUpdate() {

        var groundHit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundMask);
        shadow.SetPositionAndRotation(groundHit.point, Quaternion.identity);
        shadow.gameObject.SetActive(groundHit && groundHit.point != (Vector2)transform.position);
    }
}
