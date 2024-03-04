using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericForg : GenericEnemyBehaviour
{
    [Header("Leap to")]
    [SerializeField] private List<Transform> jumpPoints;

    [Header("Damage")]
    [SerializeField] private float damage;
    [SerializeField] private float attackKnockback, damageFlashDur;

    [Header("Idle")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime;


    [Header("Jump")]
    [SerializeField] private float gravity;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private EntityHealthCollisionTrigger attackTrigger;
    



    private Transform target;

    private bool damaging;

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    private List<Transform> remainingJumpPoints = new List<Transform>();


    private void OnAttackTriggerHit(EntityHealthCollision collision)
    {
        if (collision.entity.Team != Health.Team && damaging)
        {
            Vector2 knockbackDirection = new Vector2(Mathf.Sign(Velocity.x), 0);
            collision.entity.TakeDamage(new DamageInfo(damage, knockbackDirection, Vector2.one * attackKnockback));
        }
    }
    


    private void Awake()
    {
        attackTrigger.OnEntityEnter.AddListener(OnAttackTriggerHit);
    }

    protected override void StopBehaviour()
    {
        damaging = false;
        base.StopBehaviour();
    }

    protected override IEnumerator Behaviour()
    {
        while (true)
        {
            yield return Idle();

            yield return AboutToJump();
            damaging = true;
            yield return Jumping();
            damaging = false;

        }

    }
    protected IEnumerator Idle()
    {

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        while (idleTime > 0)
        {
            idleTime -= Time.deltaTime;
            yield return null;
        }
    }

    protected IEnumerator AboutToJump()
    {
        //this will just play the squatting animation
        yield return null;
    }

    protected IEnumerator Jumping() 
    {
        //refill list of jumpPoints at 0
        if (remainingJumpPoints.Count == 0)
        {
            remainingJumpPoints.AddRange(jumpPoints);
        }

        int jumpIndex = Random.Range(0, remainingJumpPoints.Count);
        Transform lastJump = remainingJumpPoints[jumpIndex];
        remainingJumpPoints.RemoveAt(jumpIndex);

        float speed = horizontalSpeed * Mathf.Sign(lastJump.position.x - Position.x);
        ParabolicPath jumpPath = new (Position, lastJump.position, -gravity, speed);

        while (!jumpPath.IsFinished(Position.x))
        {
            jumpPath.RenderPath(Color.red);
            Velocity = new Vector2(speed, jumpPath.GetVelocity(Position.x));
            yield return null;
        }
        Velocity = Vector2.zero;   

    }
}
