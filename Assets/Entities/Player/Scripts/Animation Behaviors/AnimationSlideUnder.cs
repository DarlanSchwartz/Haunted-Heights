using UnityEngine;

public class AnimationSlideUnder : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingSlideAnimation, true);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingSlideAnimation, false);
        animator.ResetTrigger(AnimationHashUtility.Slide);
    }
}
