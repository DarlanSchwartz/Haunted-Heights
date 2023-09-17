using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RollLandingBehavior : StateMachineBehaviour
{
    [HideInInspector] public UnityEvent onEnd;
    [HideInInspector] public UnityEvent onStart;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.RollLanding, true);
        onStart.Invoke();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.RollLanding, true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.RollLanding, false);
        onEnd.Invoke();
    }

}
