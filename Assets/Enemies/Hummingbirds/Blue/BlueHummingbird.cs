using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;

public class BlueHummingbird : GenericHummingbird
{
    [Header("Attacking")]
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackAcceleration;
    [SerializeField] private float attackDuration;

    protected override IEnumerator Attack()
    {
        float timer = 0;
        while (timer < attackDuration)
        {
            timer += Time.deltaTime;

            Vector2 toTarget = (TargetPosition - Position).normalized;
            Vector2 targetVelocity = toTarget * attackSpeed;

            Velocity = Vector2.MoveTowards(Velocity, targetVelocity, attackAcceleration * Time.deltaTime);
            
            yield return null;
        }
    }


}
