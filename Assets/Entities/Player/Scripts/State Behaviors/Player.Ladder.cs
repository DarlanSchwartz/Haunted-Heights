using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool InLadderState { get; private set; }
    private Ladder currentLadder;
    private bool m_goingToLadderStartPos = false;
    private bool m_movingInLadder = false;
    private int currentLadderSegment = 0;
    private float defaultCameraMaxX = 75;
    private bool ladderClimbingEnd = false;

    public void EnterLadder(Ladder ladder)
    {
        currentLadder = ladder;
        InLadderState = true;
        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 83;
        MouseLook.MinY = -83;
        defaultCameraMaxX = MouseLook.MaxX;
        MouseLook.MaxX = 33;

        HandIK.ManualWeights = true;
        HandIK.LeftWeight = 1;
        HandIK.RightWeight = 1;
        HandIK.SetTargetsParent(currentLadder.transform);
        HandIK.SetTargetPositions(currentLadder.LeftHandIK.position, currentLadder.RightHandIK.position, false);
        HandIK.SetTargetRotations(currentLadder.LeftHandIK.rotation, currentLadder.RightHandIK.rotation);
        HandIK.StartFollowingTargets(true, true);

        FeetIK.IsEnabled = false;
        FeetIK.forceFeetIk = true;

        FeetIK.SetIKHints(true, thisTransform.position + ((-thisTransform.right * 0.8f) + thisTransform.forward * 2), thisTransform.position + ((thisTransform.right * 0.8f) + thisTransform.forward * 2));


        Controller.detectCollisions = false;

        animator.SetBool(AnimationHashUtility.ClimbLadderIdle, true);

        currentLadderSegment = 0;
        currentLadder.UpdateDelta = true;
        m_goingToLadderStartPos = true;
        StopAllCoroutines();
        StartCoroutine(GoToLadderPoint(ladder.startPoint.transform));
    }

    private IEnumerator GoToLadderPoint(Transform TargetReference)
    {
        bool conditionMet;
        do
        {
            thisTransform.SetPositionAndRotation(Vector3.Lerp(thisTransform.position, TargetReference.position, 10 * Time.deltaTime), Quaternion.Lerp(thisTransform.rotation, TargetReference.rotation, 10 * Time.deltaTime));
            m_goingToLadderStartPos = true;
            float difference = Quaternion.Angle(thisTransform.rotation, TargetReference.rotation);
            conditionMet = Vector3.Distance(thisTransform.position, TargetReference.position) <= 0.9f && difference < 0.2f;
            yield return null;
        } while (!conditionMet);

        m_goingToLadderStartPos = false;
        thisTransform.SetPositionAndRotation(TargetReference.position, TargetReference.rotation);

        yield break;
    }

    private IEnumerator ClimbLadderPoint(Transform TargetReference)
    {
        animator.SetTrigger(AnimationHashUtility.Climb);
        animator.SetBool(AnimationHashUtility.ClimbLadderIdle, false);
        ladderClimbingEnd = true;
        currentLadder.ResetDelta(false);
        float delta = 0;
        float ikWeight = 1;
        UpdateIKPositionsInLadder(1);
        currentLadder.Climb();
        do
        {
            thisTransform.SetPositionAndRotation(currentLadder.playerReferencePosition.position, currentLadder.playerReferencePosition.rotation);
            delta += 0.5f * Time.deltaTime;

            if (delta >= 0.5f)
            {
                ikWeight -= 2 * Time.deltaTime;
            }

            UpdateIKPositionsInLadder(ikWeight);
            if (delta >= 1)
            {
                delta = 1;
            }

            currentLadder.SetTargetDelta(delta);

            yield return null;
        } while (delta != 1 || thisTransform.rotation != TargetReference.rotation);

        yield return new WaitForEndOfFrame();

        currentLadder.UpdateDelta = false;
        currentLadder.ResetDelta(true);
        animator.ResetTrigger(AnimationHashUtility.Climb);
        thisTransform.position = TargetReference.position;
        ClearLadderConstraints();
        ladderClimbingEnd = false;

        yield break;
    }

    private void HandleLadderMovement()
    {

        MouseLook.LookRotation(false);
        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CrouchCameraSpeed * Time.deltaTime);

        if (!ladderClimbingEnd)
        {
            UpdateIKPositionsInLadder(1);
        }

        if (m_goingToLadderStartPos || ladderClimbingEnd)
        {
            return;
        }

        if (!m_movingInLadder)
        {
            if (Input.GetKey(KeyCode.W))
            {
                GoToLadderSegment(currentLadderSegment + 1);
                return;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                GoToLadderSegment(currentLadderSegment - 1);
                return;
            }
        }
    }

    private void GoToLadderSegment(int thisLadderSegmentIndex)
    {
        if (thisLadderSegmentIndex < 0)
        {
            Exitladder(true);
            return;
        }

        if (thisLadderSegmentIndex >= currentLadder.points.Count)
        {
            Exitladder(false);
            return;
        }

        if (m_movingInLadder)
        {
            return;
        }

        currentLadderSegment = thisLadderSegmentIndex;
        currentLadder.SetTargetDelta(currentLadder.points[thisLadderSegmentIndex].deltaTarget);
        StartCoroutine(MoveToLadderSegment(currentLadder.points[thisLadderSegmentIndex].transform));
    }

    private void UpdateIKPositionsInLadder(float weights)
    {
        HandIK.LeftWeight = weights;
        HandIK.RightWeight = weights;
        HandIK.SetTargetPositions(currentLadder.LeftHandIK.position, currentLadder.RightHandIK.position, false);
        HandIK.SetTargetRotations(currentLadder.LeftHandIK.rotation, currentLadder.RightHandIK.rotation);

        FeetIK.leftFootIkPosition = currentLadder.LeftFootIK.position;
        FeetIK.rightFootIkPosition = currentLadder.RightFootIK.position;
        FeetIK.rightFootIkRotation = currentLadder.RightFootIK.rotation;
        FeetIK.leftFootIkRotation = currentLadder.LeftFootIK.rotation;

        FeetIK.SetIKHints(true, thisTransform.position + ((-thisTransform.right * 0.8f) + thisTransform.forward * 2), thisTransform.position + ((thisTransform.right * 0.8f) + thisTransform.forward * 2));
    }

    private IEnumerator MoveToLadderSegment(Transform targetRef)
    {
        do
        {
            thisTransform.position = Vector3.LerpUnclamped(thisTransform.position, targetRef.position, Speed.LadderSpeed * Time.deltaTime);
            m_movingInLadder = true;
            yield return null;
        } while (Vector3.Distance(thisTransform.position, targetRef.position) > 0.01f);

        thisTransform.position = targetRef.position;
        m_movingInLadder = false;

        yield break;
    }

    public void Exitladder(bool clearIkAndConstraints)
    {
        if (ladderClimbingEnd)
        {
            return;
        }

        if (clearIkAndConstraints)
        {
            animator.SetTrigger(AnimationHashUtility.CancelHang);
            HandIK.DisableHandIK();
            ClearLadderConstraints();
            return;
        }
        else
        {
            StartCoroutine(ClimbLadderPoint(currentLadder.endClimbReference));
        }
    }

    private void ClearLadderConstraints()
    {
        HandIK.DisableHandIK();
        currentLadder = null;
        MouseLook.ClampHorizontalRotation = false;
        FeetIK.forceFeetIk = false;
        InLadderState = false;
        FeetIK.IsEnabled = true;
        animator.SetBool(AnimationHashUtility.ClimbLadderIdle, false);
        MouseLook.MaxX = defaultCameraMaxX;
        Controller.detectCollisions = true;
    }
}
