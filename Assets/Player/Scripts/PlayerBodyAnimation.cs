using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverUtils;

public class PlayerBodyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip
        swingDown,
        swingDownDiagonal,
        swingHorizontal,
        swingUpDiagonal,
        swingUp;
    [SerializeField] private AnimationClip
        eatStart,
        eatEnd;

    private void PlayAnimation(AnimationClip animation)
    {
        animator.Play(animation.name);
    }

    public void SwingAnimation(Vector2 direction)
    {
        direction = new(Mathf.Abs(direction.x).Sign0(), direction.y.Sign0());

        var animation = (direction.x, direction.y) switch
        {
            (0, -1) => swingDown,
            (1, -1) => swingDownDiagonal,
            (1,  0) => swingHorizontal,
            (1,  1) => swingUpDiagonal,
            (0,  1) => swingUp,
            _ => swingHorizontal,
        };

        PlayAnimation(animation);
    }

    public void StartEat() => PlayAnimation(eatStart);
    public void EndEat() => PlayAnimation(eatEnd);
}
