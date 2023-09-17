using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool InBalanceMode { get; private set; }
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
        InBalanceMode = true;
        animator.SetBool(AnimationHashUtility.Balance, InBalanceMode);
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
        InBalanceMode = false;
        animator.SetFloat(AnimationHashUtility.MotionTimeDelta, 1);
        animator.SetBool(AnimationHashUtility.Balance, InBalanceMode);
        MouseLook.ClampHorizontalRotation = false;
        inBetweenBalanceMode = false;
        currentBalanceBean = null;
        animator.SetLayerWeight(2, 0);
        Debug.Log("ExitBalancemode");
    }
}
