using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateJumpOnto : PlayerState
{
    public PlayerStateJumpOnto(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory): base (currentContext,playerStateFactory, PlayerStateType.JumpingOnto) { IsRootState = true; }
    readonly string jumpOntoAnimationName = "Jump_Onto";
    Vector3 m_targetPosJumpOnto = Vector3.zero;
    public override void CheckSwitchStates() { }
    public override void EnterState()
    {
        m_targetPosJumpOnto = JumpOntoEndPosition;
        m_targetPosJumpOnto.y += 0.2f;
        Context.JumpingOnto = true;
        Context.Animator.SetBool(AnimationHashUtility.JumpingOnto,true);
        Context.Animator.SetBool(AnimationHashUtility.Stand,false);
        Context.StartCoroutine(JumpOntoLerpDelta(Context.Animator.GetBehaviour<BehaviourJumpingOnto>()));
        Context.Animator.Play(jumpOntoAnimationName, 0);
    }
    public override void ExitState()
    {
        Context.JumpingOnto = false;
        Context.Animator.SetBool(AnimationHashUtility.JumpingOnto, false);
    }

    private IEnumerator JumpOntoLerpDelta(BehaviourJumpingOnto jumpingOntoAnim)
    {
        Vector3 startPos = Context.ThisTransform.position;
        do
        {
            Context.ThisTransform.position = Vector3.SlerpUnclamped(startPos, m_targetPosJumpOnto, jumpingOntoAnim.AnimationDelta);
            Context.PlayerCamera.position = Context.PlayerHead.position;
            yield return null;
        } while (!jumpingOntoAnim.Complete);

        SwitchState(Factory.Grounded());
        yield break;
    }

    public Vector3 JumpOntoEndPosition
    {
        get
        {
            Vector3 start = Context.ThisTransform.position + (Context.ThisTransform.forward * Context.VaultSettings.JumpOntoMaxDistance);
            start.y += 4;
            Physics.Raycast(start, Vector3.down, out RaycastHit hitPoint, 10);
            return hitPoint.point;
        }
    }
    public override void InitializeSubState() { }
    public override void OnCollisionEnter() { }
    public override void OnCollisionExit() { }
    public override void OnCollisionStay() { }
    public override void UpdateState() { }
}
