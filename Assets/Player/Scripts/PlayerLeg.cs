using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLeg : Player.Component {

    [SerializeField] private float wiggleSpeed, wiggleAngleRange;
    [SerializeField] private bool alternate;

    private void LateUpdate() {

        transform.eulerAngles = Movement.OnGround() && InputDirection.x != 0
            ? Vector3.forward * (2f * Mathf.PingPong(Time.time, wiggleSpeed) / wiggleSpeed - 1) * wiggleAngleRange * (alternate ? -1 : 1)
            : Vector3.zero;
    }
}
