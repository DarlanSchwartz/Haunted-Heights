using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationClimb : StateMachineBehaviour
{
    public float DoneAfter = 1;
    private float timerDoneAfter = 0;
    
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingClimbAnimation, true);
        animator.SetFloat(AnimationHashUtility.Vertical, 0);
        animator.SetFloat(AnimationHashUtility.Horizontal, 0);
        animator.ResetTrigger(AnimationHashUtility.Climb);

        if (DoneAfter > 0)
        {
            timerDoneAfter = 0;
            timerDoneAfter += Time.deltaTime;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(DoneAfter > 0)
        {
            timerDoneAfter += Time.deltaTime;
            if (timerDoneAfter >= DoneAfter)
            {
                animator.SetInteger(AnimationHashUtility.FreeHangStage, 1);
            }
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(DoneAfter == 0)
        {
            animator.SetBool(AnimationHashUtility.PlayingClimbAnimation, false);
        }
       
    }
}
