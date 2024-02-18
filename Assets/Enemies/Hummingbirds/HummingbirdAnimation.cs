using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class HummingbirdAnimation : MonoBehaviour {

    [Header("Wings")]
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private float wingPivotAngle, wingPivotRange;
    [SerializeField] private Transform visualsPivot, wingPivot;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;

    public bool turnToTarget;
    public Transform target;

    private bool wingFlip;

    private void Start() {
        hoverOscillation.offset = Random.value;
    }

    private void Update() {

        visualsPivot.localPosition = Vector2.up * hoverOscillation.Evaluate();

        wingFlip = !wingFlip;
        wingPivot.localEulerAngles = Vector3.forward * (wingPivotAngle + (wingFlip ? -1 : 1) * wingPivotRange / 2f);

        int direction = (int)Mathf.Sign(turnToTarget ? (target.position - transform.position).x : rigidbody.velocity.x);
        visualsPivot.localScale = new(direction, 1, 1);
    }
}
