using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClimbMode { Ladder, FromFreeHang,FromBracedHang }
public class BehaviourClimbUp : StateMachineBehaviour
{
    public float AnimationDelta { get; private set; } = 0;
    public ClimbMode ClimbMode;
    public bool Complete { get; private set; } = false;
    [Range(0f, 2f)] public float AnimationLenght = 0.78f;
    private float _timeDelta;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(AnimationHashUtility.Climb_Up);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!Complete)
        {
            _timeDelta += Time.deltaTime;
            _timeDelta = Mathf.Clamp(_timeDelta, 0, AnimationLenght);

            AnimationDelta = _timeDelta / AnimationLenght;

            if (_timeDelta == AnimationLenght)
            {
                Complete = true;
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimationDelta = 0;
        _timeDelta = 0;
        Complete = false;
    }
}
