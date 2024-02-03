using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedHummingbird : GenericHummingbird {

    [Header("Diving")]
    [SerializeField] private float diveSpeed;
    [SerializeField] private float maxDiveDist;

    protected override IEnumerator Attack() {

        Vector2 divePosition = Vector2.ClampMagnitude(TargetPosition - Position, maxDiveDist) + Position;

        yield return MoveTo(divePosition, diveSpeed);
    }
}
