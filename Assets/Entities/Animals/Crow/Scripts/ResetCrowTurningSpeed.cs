using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCrowTurningSpeed : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    [SerializeField] private RandomFlyer flyer;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(flyer == null)
        {
            flyer = animator.transform.GetComponent<RandomFlyer>();
        }

        flyer.ResetTurningSpeed();
    }
}
