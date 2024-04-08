using System.Collections;
using System.Collections.Generic;
using OliverUtils;
using UnityEngine;

public class YellowHummingbird : GenericEnemyBehaviour
{
    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Nectar")]
    [SerializeField] private GameObject nectarPrefab;

    [Header("Moving")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float minMoveDist, maxMoveDist;
    [SerializeField] private int leftRightCycleCount;

    [Header("Return to Ceiling")]
    [SerializeField] private float ceilingFloorReturnSpeed;
    [SerializeField] private float maxAllowedDistFromHeightTarget;
    [SerializeField] private float heightTarget;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    private List<bool> moveDirections = new List<bool>();

    private void DropNectar()
    {
        Instantiate(nectarPrefab, Position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, heightTarget));
        float width = 6;
        Gizmos.DrawRay(new Vector3(transform.position.x - width / 2, heightTarget), Vector2.right * width);
    }

    protected override IEnumerator Behaviour() 
    {
        while(true) {

            float distFromHeightTarget = heightTarget - transform.position.y;
            if (distFromHeightTarget > maxAllowedDistFromHeightTarget)
            {
                yield return ReturnToCeiling();
            }
            
            yield return Idle();

            DropNectar();
            yield return HorizontalMove();
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

    protected IEnumerator MoveTo(Vector2 targetPosition, float speed)
    {

        Vector2 vectorToTarget = targetPosition - Position;

        Velocity = vectorToTarget.normalized * speed;

        float distanceToTarget = vectorToTarget.magnitude,
              timeToTarget = distanceToTarget / speed;

        yield return new WaitForSeconds(timeToTarget);
    }

    protected IEnumerator HorizontalMove()
    {
        //if statement is used to refresh the list back to 6 once all "moves" have been used
        if (moveDirections.Count == 0)
        {
            for (int i = leftRightCycleCount; i > 0; i--)
            {
                moveDirections.Add(true);
                moveDirections.Add(false);
            }
        }

        int moveIndex = Random.Range(0, moveDirections.Count);
        bool moveRight = moveDirections[moveIndex];
        moveDirections.RemoveAt(moveIndex);

        //Move right at an index, else move left (noted by the -1)
        float moveDirection = moveRight ? 1 : -1;

        float moveDistance = Random.Range(minMoveDist, maxMoveDist);

        //Random Value takes two different directions, left or right
        //Used later for movePosition to determine a random direction for the hummingbird to go to

        Vector2 movePosition = Position + new Vector2(moveDirection * moveDistance, 0);
        yield return MoveTo(movePosition, moveSpeed);
    }

    protected IEnumerator ReturnToCeiling()
    {
        Vector2 targetPosition = new Vector2(transform.position.x, heightTarget);

        yield return Idle();
        yield return MoveTo(targetPosition, ceilingFloorReturnSpeed);

        Velocity = Vector2.zero;
    }
}
