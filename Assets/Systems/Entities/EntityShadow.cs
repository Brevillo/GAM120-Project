using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityShadow : MonoBehaviour {

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform shadow;

    private void Update() {

        var groundHit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundMask);
        if (groundHit) shadow.transform.position = groundHit.point;
        shadow.gameObject.SetActive(groundHit && groundHit.point != (Vector2)transform.position);
    }
}
