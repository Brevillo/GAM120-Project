using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedHummingbird : GenericHummingbird {

    [Header("Diving")]
    [SerializeField] private float diveSpeed;
    [SerializeField] private float maxDiveDist, playerPassDistance;

    protected override IEnumerator Attack() {

        Vector2 diveVector    = TargetPosition - Position,
                diveDirection = diveVector.normalized,
                divePosition  = diveDirection * (Mathf.Min(diveVector.magnitude, maxDiveDist) + playerPassDistance) + Position;

        yield return MoveTo(divePosition, diveSpeed);

        Velocity = Vector2.zero;
    }
}
