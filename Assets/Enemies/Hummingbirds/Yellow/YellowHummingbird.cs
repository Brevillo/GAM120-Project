using System.Collections;
using System.Collections.Generic;
using OliverUtils;
using UnityEngine;

public class YellowHummingbird : GenericEnemy
{

    [Header("Idling")]
    [SerializeField] private float minIdleTime;
    [SerializeField] private float maxIdleTime, airFriction;

    [Header("Nectar")]
    [SerializeField] private GameObject nectarPrefab;

    [Header("Moving")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float minMoveDist, maxMoveDist;
    [SerializeField] private float minMoves, maxMoves;
    [SerializeField] private int leftRightCycleCount;

    [Header("Return to Ceiling")]
    [SerializeField] private float ceilingDetectDist;
    [SerializeField] private float ceilingStopDist;
    [SerializeField] private float ceilingReturnSpeed;

    [Header("Visuals")]
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private SpriteRenderer rend;

    private Transform target;
    private Color color;

    public override IWhippable.Type WhippableType => IWhippable.Type.Light;
    
    private void DropNectar()
    {
        Instantiate(nectarPrefab, Position, Quaternion.identity);
    }
    protected override IEnumerator Behaviour() 
    {
        while(true) {

            //The bird casts a line from it's current position to the ceiling indefinitely 
            RaycastHit2D ceilingHit = Physics2D.Raycast(Position, Vector2.up, ceilingDetectDist, GameInfo.GroundMask);
            if (!ceilingHit){
                yield return ReturnToCeiling();
            }

            yield return Idle();

            float moves = Random.Range(minMoves, maxMoves);
            
            while (moves > 0) {

                moves--;
                DropNectar();
                yield return HorizontalMove();
                yield return Idle();
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

    protected IEnumerator MoveTo(Vector2 targetPosition, float speed)
    {

        Vector2 vectorToTarget = targetPosition - Position;

        Velocity = vectorToTarget.normalized * speed;

        float distanceToTarget = vectorToTarget.magnitude,
              timeToTarget = distanceToTarget / speed;

        yield return new WaitForSeconds(timeToTarget);
    }

        List<bool> moveDirections = new List<bool>();
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

    protected virtual void Update()
    {
        // hover effect
        if (BehaviourActive)
            rend.transform.localPosition = Vector2.up * hoverOscillation.Evaluate();
    }
    protected IEnumerator ReturnToCeiling()
    {
        
        RaycastHit2D findCeiling = Physics2D.Raycast(Position, Vector2.up, Mathf.Infinity, GameInfo.GroundMask);
        Vector2 ceilingOffset = new Vector2(0, ceilingStopDist);
        Vector2 returnUp = findCeiling.point - ceilingOffset;


        yield return Idle();
        yield return MoveTo(returnUp, ceilingReturnSpeed);

        Velocity = Vector2.zero;
    }



}
