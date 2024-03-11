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
    [SerializeField] private MovementType movementType;
    [SerializeField] private float ceilingFloorDetectDist;
    [SerializeField] private float ceilingFloorOffset;
    [SerializeField] private float ceilingFloorReturnSpeed;

    private enum MovementType { RelativeToCeiling, RelativeToFloor }

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;

    private List<bool> moveDirections = new List<bool>();

    private void DropNectar()
    {
        Instantiate(nectarPrefab, Position, Quaternion.identity);
    }

    protected override IEnumerator Behaviour() 
    {
        while(true) {

            //The bird casts a line from it's current position to the ceiling indefinitely
            Vector2 checkDirection = movementType switch {
                MovementType.RelativeToCeiling => Vector2.up,
                MovementType.RelativeToFloor => Vector2.down,
                _ => Vector2.down,
            };
            RaycastHit2D ceilingHit = Physics2D.Raycast(Position, checkDirection, ceilingFloorDetectDist, GameInfo.GroundMask);
            if (!ceilingHit) yield return ReturnToCeiling();
            
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
        Vector2 targetPosition = movementType switch {
            MovementType.RelativeToCeiling => Physics2D.Raycast(Position, Vector2.up,   Mathf.Infinity, GameInfo.GroundMask).point + Vector2.down * ceilingFloorOffset,
            MovementType.RelativeToFloor   => Physics2D.Raycast(Position, Vector2.down, Mathf.Infinity, GameInfo.GroundMask).point + Vector2.up * ceilingFloorOffset,
            _ => Position
        };

        yield return Idle();
        yield return MoveTo(targetPosition, ceilingFloorReturnSpeed);

        Velocity = Vector2.zero;
    }
}
