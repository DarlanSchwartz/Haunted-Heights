using UnityEngine;

public class AnimationBehaviorGrab : StateMachineBehaviour
{
    public float AnimationDelta { get; private set; } = 0;
    public bool Complete { get; private set; } = false;
    [Range(0f, 2f)]
    public float AnimationLenght = 0.27f;

    public int animationID = 0;

    private float _timeDelta;


    public void Reset()
    {
        AnimationDelta = 0;
        _timeDelta = 0;
        Complete = false;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Reset();
    }

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
}
