using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadButt : Player.Component {

    [SerializeField] private float maxChargeTime;
    [SerializeField] private float minDamage, cooldown, maxDamage, minDashDistance, maxDashDistance, dashSpeed, minChargeShake, maxChargeShake;

    private State state;
    private float stateDuration;
    private float attackStrengthPercent;

    private enum State { Idle, Charging, Headbutting }

    private void ChangeState(State newState) {
        stateDuration = 0;
        state = newState;
    }

    private void Update() {

        stateDuration += Time.deltaTime;

        switch (state) {

            case State.Idle:

                if (Input.Attack.Down && stateDuration > cooldown)
                    ChangeState(State.Charging);

                break;

            case State.Charging:

                if (!Input.Attack.Pressed) {
                    attackStrengthPercent = Mathf.Clamp01(stateDuration / maxChargeTime);
                    ChangeState(State.Headbutting);
                }

                break;

            case State.Headbutting:

                float headbuttDuration = Mathf.Lerp(minDashDistance, maxDashDistance, attackStrengthPercent) / dashSpeed;

                if (stateDuration > headbuttDuration)
                    ChangeState(State.Idle);

                break;
        }
    }
}
