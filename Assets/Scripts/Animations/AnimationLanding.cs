using UnityEngine;

public class AnimationLanding : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingLandAnimation, true);
        animator.ResetTrigger(AnimationHashUtility.Land);
        animator.ResetTrigger(AnimationHashUtility.Jump);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingLandAnimation, false);
    }
}
