using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool InBalanceState { get; private set; }
    private float motionTime = 0;
    private BalanceBeam currentBalanceBean;
    private Vector3 currentBalanceBeanTarget;
    private bool m_goinToBalanceStartPos = false;
    public bool inBetweenBalanceMode = false;
    private Transform m_currentTriggerStartTransform;

    public void HandleEnterBalanceBeam(BalanceBeam balanceBeam, Transform EnterOrExitedTrigger, bool Entering)
    {
        if (Entering)
        {
            if (currentBalanceBean == null && !inBetweenBalanceMode)
            {
                currentBalanceBean = balanceBeam;
                if (balanceBeam.PointA == EnterOrExitedTrigger)
                {
                    EnterBalanceMode(currentBalanceBean.PointB.position, EnterOrExitedTrigger);
                }
                else
                {
                    EnterBalanceMode(currentBalanceBean.PointA.position, EnterOrExitedTrigger);
                }
                return;
            }
            else if (currentBalanceBean == balanceBeam && inBetweenBalanceMode || currentBalanceBean != balanceBeam && inBetweenBalanceMode)
            {
                ExitBalanceMode();
                return;
            }
        }
        else
        {
            if (balanceBeam == currentBalanceBean && !inBetweenBalanceMode || balanceBeam != currentBalanceBean && !inBetweenBalanceMode)
            {
                ExitBalanceMode();
                return;
            }
        }
    }
    public void EnterBalanceMode(Vector3 target, Transform startTrigger)
    {
        m_currentTriggerStartTransform = startTrigger;
        currentBalanceBeanTarget = target;
        InBalanceState = true;
        animator.SetBool(AnimationHashUtility.Balance, InBalanceState);
        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 70;
        MouseLook.MinY = -70;
        motionTime = animator.GetFloat(AnimationHashUtility.LeftFootCurve) >= 0.4f ? 0.5f : 0;
        animator.SetBool(AnimationHashUtility.Idle, false);
        animator.SetTrigger(AnimationHashUtility.StartBalance);
        animator.SetFloat(AnimationHashUtility.Vertical, 0.1f);
        animator.SetLayerWeight(2, 0.5f);
        StartCoroutine(GoToBalanceStart(m_currentTriggerStartTransform));
    }
    private IEnumerator GoToBalanceStart(Transform startPos)
    {
        do
        {
            thisTransform.SetPositionAndRotation(Vector3.Slerp(thisTransform.position, startPos.position, 50 * Time.deltaTime), Quaternion.Slerp(thisTransform.rotation, startPos.rotation, 10 * Time.deltaTime));
            m_goinToBalanceStartPos = true;
            yield return null;
        } while (Vector3.Distance(thisTransform.position, startPos.position) > 0.01f && thisTransform.rotation != startPos.rotation);

        m_goinToBalanceStartPos = false;

        yield break;
    }
    public void ExitBalanceMode()
    {
        InBalanceState = false;
        animator.SetFloat(AnimationHashUtility.MotionTimeDelta, 1);
        animator.SetBool(AnimationHashUtility.Balance, InBalanceState);
        MouseLook.ClampHorizontalRotation = false;
        inBetweenBalanceMode = false;
        currentBalanceBean = null;
        animator.SetLayerWeight(2, 0);
        Debug.Log("ExitBalancemode");
    }

    private void HandleBalanceMovement()
    {

        if (m_goinToBalanceStartPos)
        {
            return;
        }

        thisTransform.position = Vector3.MoveTowards(thisTransform.position, currentBalanceBeanTarget, (VerticalInput * GetTargetSpeed) * Time.deltaTime);
        animator.SetFloat(AnimationHashUtility.Vertical, VerticalInput);
        MouseLook.LookRotation(false);

        if (VerticalInput > 0)
        {
            motionTime += 0.5f * Time.deltaTime;
        }
        else if (VerticalInput < 0)
        {
            motionTime += 0.25f * Time.deltaTime;
        }

        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CrouchCameraSpeed * Time.deltaTime);

        animator.SetFloat(AnimationHashUtility.MotionTimeDelta, motionTime);
    }
}
