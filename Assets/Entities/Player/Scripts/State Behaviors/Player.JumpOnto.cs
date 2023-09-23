using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HighDefinition.CameraSettings;

public partial class PlayerMove
{
    private RaycastHit m_ForwardOntoHit;
    private Vector3 m_targetPosJumpOnto;
    public bool JumpingOnto { get { return animator.GetBool(AnimationHashUtility.JumpingOnto); } }
    public bool JumpOntoObjectForward { get { return Physics.Raycast(KneesHeightPosition, thisTransform.forward, out m_ForwardOntoHit, VaultSettings.JumpOntoMaxDistance, VaultSettings.JumpableOntoLayers); } }
    public bool CanJumpOnto { get { return !Vaulting && !Jumping && !Landing && OnGround && !PlayingFallingAnimation && !Falling && !animator.GetBool(AnimationHashUtility.PlayingJumpAnimation) && !JumpingOnto && JumpOntoObjectForward; } }
    private void StartJumpOnto()
    {
        if (JumpingOnto || animator.GetBool(AnimationHashUtility.JumpingOnto))
        {
            return;
        }
        m_targetPosJumpOnto = GetJumpOntoEndPosition();
        m_targetPosJumpOnto.y += 0.2f;
        animator.SetTrigger(AnimationHashUtility.JumpOnto);
        StartCoroutine(JumpOntoLerpDelta(animator.GetBehaviour<BehaviourJumpingOnto>()));
    }

    private IEnumerator JumpOntoLerpDelta(BehaviourJumpingOnto jumpingOntoAnim)
    {
        Vector3 startPos = thisTransform.position;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("Started Jump onto Coroutine.");
        }

        do
        {
            thisTransform.position = Vector3.SlerpUnclamped(startPos, m_targetPosJumpOnto, jumpingOntoAnim.AnimationDelta);
            PlayerCamera.position = PlayerHead.position;
            yield return null;
        } while (!jumpingOntoAnim.Complete);
        EndJumpOnto();
    }

    private void EndJumpOnto()
    {
        Falling = false;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("End Jump Onto");
        }
    }
}
