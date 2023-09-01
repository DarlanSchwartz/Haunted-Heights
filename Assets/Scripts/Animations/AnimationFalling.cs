﻿using UnityEngine;

public class AnimationFalling : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    { 
        animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, true);
        animator.ResetTrigger(AnimationHashUtility.CancelHang);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, false);
        animator.ResetTrigger(AnimationHashUtility.Jump);
        animator.SetBool(AnimationHashUtility.PlayerFalling, false);
    }
}
