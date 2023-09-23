using UnityEngine;

public class AnimationFalling : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    { 
        animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, false);
    }
}
