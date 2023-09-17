using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReseterEvents
{
    OnEnter,
    OnExit,
    OnEnterAndExit
}

public class TriggerReseter : StateMachineBehaviour
{
    public ReseterEvents Reset = ReseterEvents.OnEnterAndExit;

    public string TriggerName;

    private int triggerHash;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(triggerHash == 0)
        {
            triggerHash = Animator.StringToHash(TriggerName);
        }

        switch (Reset)
        {
            case ReseterEvents.OnEnter:
                animator.ResetTrigger(triggerHash);
                break;
            case ReseterEvents.OnEnterAndExit:
                animator.ResetTrigger(triggerHash);
                break;
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (Reset)
        {
            case ReseterEvents.OnExit:
                animator.ResetTrigger(triggerHash);
                break;
            case ReseterEvents.OnEnterAndExit:
                animator.ResetTrigger(triggerHash);
                break;
        }
    }
}
