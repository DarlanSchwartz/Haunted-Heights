using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowIdleBehaviour : StateMachineBehaviour
{
    private readonly int landHash = Animator.StringToHash("Land");
    private readonly int stateID = Animator.StringToHash("CurrentState");

    public float minTimeToChangeState;
    private float currentTime;
    public int AnimationsAmount;
    [SerializeField]private int currentState;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(landHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentTime += Time.deltaTime;

        if (currentTime >= minTimeToChangeState)
        {
           animator.SetInteger(stateID, Random.Range(0, AnimationsAmount));
            currentTime = 0;
            return;
        }
    }

}
