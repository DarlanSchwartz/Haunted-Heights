using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourJumpingOnto : StateMachineBehaviour
{
    public float AnimationDelta { get; private set; } = 0;
    public bool Complete { get; private set; } = false;
    [Range(0f, 2f)] public float AnimationLenght = 0.78f;
    private float _timeDelta;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
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

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {
        AnimationDelta = 0;
        _timeDelta = 0;
        Complete = false;
    }
}
