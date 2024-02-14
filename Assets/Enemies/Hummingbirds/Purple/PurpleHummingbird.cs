using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PurpleHummingbird : GenericHummingbird
{
    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Babies")]
    [SerializeField] private GameObject miniPurple;

    [Header("Diving")]
    [SerializeField] private float diveSpeed;
    [SerializeField] private float maxDiveSpeed, playerPassDistance;

    // Start is called before the first frame update
    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    protected IEnumerator Idle()
    {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0)
        {

            idleTime -= Time.deltaTime;

            // move velocity to zero if bumped
            Velocity = Vector2.MoveTowards(Velocity, Vector2.zero, airFriction * Time.deltaTime);

            yield return null;
        }
    }
    protected override IEnumerator Attack()
    {
        yield return null;
        /*Vector2 diveVector = TargetPosition - Position,
                diveDirection = diveVector.normalized,
                divePosition = diveDirection * (Mathf.Min(diveVector.magnitude, maxDiveDist) + playerPassDistance) + Position;

        yield return MoveTo(divePosition, diveSpeed);

        Velocity = Vector2.zero;*/
    }
}
