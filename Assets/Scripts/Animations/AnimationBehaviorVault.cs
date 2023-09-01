using UnityEngine;

public class AnimationBehaviorVault : StateMachineBehaviour
{
    public float AnimationDelta { get; private set; } = 0;
    public bool Complete { get; private set; } = false;
    [Range(0f, 2f)]
    public float AnimationLenght = 0.78f;

    public VaultSpeed vaultMode = VaultSpeed.Idle;
    public VaultDirection direction = VaultDirection.Left;

    private float _timeDelta;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.ResetTrigger(AnimationHashUtility.Vault);
        animator.SetBool(AnimationHashUtility.PlayingVaultAnimation, true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(AnimationHashUtility.PlayingVaultAnimation, false);
    }

    public void Reset()
    {
        AnimationDelta = 0;
        _timeDelta = 0;
        Complete = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if(!Complete)
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
