using System.Collections;
using System.Collections.Generic;
using OliverUtils;
using UnityEngine;

public class YellowHummingbird : GenericEnemy
{

    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Moving")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float minMoveDist, maxMoveDist;
    [SerializeField] private float minMoves, maxMoves;

    [Header("Nectar")]
    [SerializeField] private float dropSpeed;
    [SerializeField] private float attackKnockback;
    [SerializeField] private float damage, hurtKnockback, damageFlashDur;

    [Header("Visuals")]
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private SpriteRenderer rend;

    private Transform target;
    private Color color;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;
    protected override IEnumerator Behaviour()
    {
        while(true) {

            yield return Idle(); {

            float moves = Random.Range(minMoves, maxMoves);

            while(moves > 0) {

                moves--;
                yield return Move();
                yield return Idle();
            }
            }

        }
    }
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

    protected IEnumerator Move()
    {

        float moveDistance = Random.Range(minMoveDist, maxMoveDist);

    }
}
