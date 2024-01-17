using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DraggableRope : MonoBehaviour {

    [SerializeField] private VerletRope.Parameters parameters;
    [SerializeField] private Vector2 force;
    [SerializeField] private bool addForce;

    private LineRenderer line;
    private VerletRope rope;

    private void OnValidate() {

        if (addForce) {
            addForce = false;
            rope.AddForce(force);
        }
    }

    private void Awake() {
        rope = new(parameters, transform.position);
        line = GetComponent<LineRenderer>();
    }

    private void FixedUpdate() {
        rope.Update(transform.position);
        rope.ApplyToLineRenderer(line);
    }
}
