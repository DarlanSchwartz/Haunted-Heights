using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool CanHang { get; private set; } = false;
    public bool Hanging { get; private set; } = false;
    public bool Climbing { get; private set; } = false;
    public bool GoingToHangTarget { get; private set; } = false;
    public bool GrabableObjectForward { get { return ClimbSettings.HangableTags.Contains(m_fwdHit.transform.tag); } }
    public bool InHangableAngle { get { return (CurrentHangAngle >= ClimbSettings.MaxAngle && CurrentHangAngle <= 180); } }
    public float CurrentHangAngle { get { return Vector3.Angle(thisTransform.forward, m_fwdHit.normal); } }
    public Transform LastHangObject { get; private set; } = null;
    public bool HasSomethingForward { get { return Physics.Raycast(PlayerCamera.position, PlayerCamera.forward, out m_fwdHit, ClimbSettings.MaxDistance, ClimbSettings.Layers, QueryTriggerInteraction.Collide); } }
    public bool EnableHandsIK { get; set; }

    public bool CanClimb { get { return !MovingBackwards && !Landing && (!OnGround || Jumping) && HasSomethingForward && FarFromGround && GrabableObjectForward && InHangableAngle && m_fwdHit.transform != LastHangObject; } }

    public RaycastHit m_fwdHit;
    private Vector3 m_target_GrabPos = Vector3.zero;
    private Vector3 m_target_GrabRot;
    private Vector3 m_target_EndClimbPos = Vector3.zero;
    private Vector2 m_hangingInput;
    private GrabableObject m_current_GrabObject;
    private bool m_hangingon_DynamicObject = false;
    private float m_stored_maxX;

    private void StartHang()
    {
        bool isAFreeHangObject;
        LastHangObject = m_fwdHit.transform;

        m_current_GrabObject = m_fwdHit.transform.GetComponent<GrabableObject>();

        m_target_GrabPos = m_current_GrabObject.startTarget.position;
        m_target_GrabRot = m_current_GrabObject.startTarget.eulerAngles;
        m_target_EndClimbPos = m_current_GrabObject.endTarget.position;

        m_stored_maxX = MouseLook.MaxX;

        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 83;
        MouseLook.MinY = -83;

        HandIK.LeftWeight = 0;
        HandIK.RightWeight = 0;

        switch (m_current_GrabObject.Movement)
        {
            case BragableMovementType.Static:
                m_hangingon_DynamicObject = false;
                break;
            case BragableMovementType.Dynamic:
                thisTransform.SetParent(m_current_GrabObject.transform.parent);
                m_hangingon_DynamicObject = true;
                break;
        }

        switch (m_current_GrabObject.GrabType)
        {
            case GrabTypes.FreeHang:
                animator.SetInteger(AnimationHashUtility.HangType, 1);
                HandIK.SetBodyLook(m_current_GrabObject.LookAtHanging.position, true, 1);
                HandIK.SetElbows(m_current_GrabObject.leftElbowTarget, m_current_GrabObject.rightElbowTarget, true);
                isAFreeHangObject = true;
                break;
            case GrabTypes.BracedHang:
                animator.SetInteger(AnimationHashUtility.HangType, 0);
                HandIK.SetBodyLook(m_current_GrabObject.startTarget.position + ClimbSettings.StartOffset, true, 1);
                isAFreeHangObject = false;
                break;
            default:
                animator.SetInteger(AnimationHashUtility.HangType, 0);
                isAFreeHangObject = false;
                break;
        }

        animator.SetLayerWeight(1, 1);
        HandIK.SetWeightSpeed(2);
        Hanging = true;
        CheckGround = false;
        animator.SetBool(AnimationHashUtility.FarFromGround, false);
        animator.SetBool(AnimationHashUtility.OnGround, false);
        animator.SetBool(AnimationHashUtility.Hanging, true);

        if (m_current_GrabObject.fixedPositions)
        {
            PlaceHandOnHangTarget();
        }

        StartCoroutine(Hang(GetBehaviourGrab(isAFreeHangObject ? 1 : 0)));
    }

    private AnimationBehaviorGrab GetBehaviourGrab(int id)
    {
        foreach (AnimationBehaviorGrab currentAnimBehaviour in animator.GetBehaviours<AnimationBehaviorGrab>())
        {
            if (currentAnimBehaviour.animationID == id)
            {
                return currentAnimBehaviour;
            }
        }

        return null;
    }

    private IEnumerator Hang(AnimationBehaviorGrab grabAnim)
    {
        if (GoingToHangTarget)
        {
            yield break;
        }

        if (DebugSettings.DebugHang)
        {
            Debug.Log("Started Hanging.");
        }

        bool freeHang = grabAnim.animationID == 0 ? false : true;
        float maxX = freeHang ? ClimbSettings.MouseLookMaxXFreeHang : ClimbSettings.MouseLookMaxXGrab;
        GoingToHangTarget = true;
        Destroy(Controller);
        yield return new WaitForEndOfFrame();
        Controller = gameObject.AddComponent<CharacterController>();
        Controller.center = ClimbSettings.ControllerCenter;
        Controller.height = ClimbSettings.ControllerHeight;
        Controller.detectCollisions = false;
        Controller.minMoveDistance = 0;
        Controller.skinWidth = 0.001f;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;
        Controller.slopeLimit = DefaultSettings.ControllerSlopeLimit;
        Controller.SimpleMove(Vector3.zero);

        yield return null;

        // Go to climb point
        do
        {
            thisTransform.SetPositionAndRotation(Vector3.MoveTowards(thisTransform.position, m_target_GrabPos, Speed.HangSpeed * Time.deltaTime), Quaternion.Lerp(thisTransform.rotation, Quaternion.Euler(m_target_GrabRot), Speed.HangSpeed * 5 * Time.deltaTime));
            HandIK.LeftWeight = Mathf.LerpUnclamped(HandIK.LeftWeight, 1, 5 * Time.deltaTime);
            HandIK.RightWeight = Mathf.LerpUnclamped(HandIK.RightWeight, 1, 5 * Time.deltaTime);

            if (freeHang)
            {
                if (MouseLook.MaxX > maxX)
                {
                    MouseLook.MaxX -= 5 * Time.deltaTime;
                }
                else
                {
                    MouseLook.MaxX = maxX;
                }

                MouseLook.MaxX = Mathf.Clamp(MouseLook.MaxX, 0, maxX);
            }

            yield return null;
        } while (!grabAnim.Complete && Vector3.Distance(thisTransform.position, m_target_GrabPos) > 0.1f && MouseLook.MaxX < maxX);

        thisTransform.position = m_current_GrabObject.startTarget.position;
        GoingToHangTarget = false;

        // Check inputs to cancel grab or climb up
        do
        {
            // Check inputs and look for neighboos
            if (m_current_GrabObject.Neighboors.Count > 0)
            {
                m_hangingInput.x = HorizontalInput;
                m_hangingInput.y = VerticalInput;
            }

            SetHandsIkOnClimbTarget();

            if (Input.GetKeyDown(crouchKey) || Input.GetKey(crouchKey) || Input.GetKeyUp(crouchKey) || VerticalInput < -0.1f)
            {
                CancelHang();
                yield break;
            }
            else if (m_target_EndClimbPos != Vector3.zero && (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(KeyCode.W)) && !GoingToHangTarget && !PlayingHangStartAnimation)
            {
                StartClimb(freeHang);
                yield break;
            }

            yield return null;

        } while (true);
    }

    private void SetHandsIkOnClimbTarget()
    {
        if (ClosestNeighboor(m_hangingInput, m_current_GrabObject) != null)
        {
            if (m_hangingInput.x > 0)
            {
                HandIK.SetRightTargetPosition(ClosestNeighboor(m_hangingInput, m_current_GrabObject).rightHandIKTarget.position, true);
                HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
            }
            else if (m_hangingInput.x < 0)
            {
                HandIK.SetLeftTargetPosition(ClosestNeighboor(m_hangingInput, m_current_GrabObject).leftHandIKTarget.position, true);
                HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
            }
            else
            {
                HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
                HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
            }
        }
        else
        {
            HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
            HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
        }
    }
    private void StartClimb(bool isFreehangObject)
    {
        if (isFreehangObject)
        {
            SetHandsIkOnClimbTarget();
        }

        HandIK.SetElbows(null, null, false);
        animator.SetTrigger(AnimationHashUtility.Climb);
        animator.SetBool(AnimationHashUtility.PlayingClimbAnimation, true);
        animator.SetBool(AnimationHashUtility.PlayerFalling, false);
        animator.SetBool(AnimationHashUtility.OnGround, true);
        animator.SetFloat(AnimationHashUtility.Vertical, 0);
        animator.SetFloat(AnimationHashUtility.Horizontal, 0);

        animator.SetBool(AnimationHashUtility.Hanging, false);
        HandIK.ResetBodyLook();
        Climbing = true;
        StartCoroutine(GoToClimbEndTarget(isFreehangObject));
    }
    private IEnumerator GoToClimbEndTarget(bool fh)
    {
        float speed = fh ? Speed.ClimbFreeSpeed : Speed.ClimbSpeed;

        do
        {// TODO : Climb animation behavior to sync animation with this movement
            thisTransform.position = Vector3.Slerp(thisTransform.position, m_target_EndClimbPos, speed * Time.deltaTime);
            if (MouseLook.MaxX - 5 * Time.deltaTime >= ClimbSettings.MouseLookMaxXGrab)
            {
                MouseLook.MaxX -= 5 * Time.deltaTime;
            }

            if (MouseLook.MaxY > 85)
            {
                MouseLook.MaxX -= 5 * Time.deltaTime;
            }
            else if (MouseLook.MaxY < -85)
            {
                MouseLook.MaxX += 5 * Time.deltaTime;
            }

            if (HasSomethingAboveHead)
            {
                Crouched = true;
            }

            MouseLook.LookRotation(false);
            yield return null;

        } while (Vector3.Distance(thisTransform.position, m_target_EndClimbPos) > 0.01f && animator.GetBool(AnimationHashUtility.PlayingClimbAnimation));

        EndClimb();

        yield break;
    }
    private void EndClimb()
    {
        if (GoingToHangTarget)
        {
            return;
        }

        if (m_hangingon_DynamicObject)
        {
            thisTransform.SetParent(null);
            m_hangingon_DynamicObject = false;
        }

        m_gravityForceV.Set(0, 0, 0);
        HandIK.StopFollowingTargets();
        HandIK.ResetWeightSpeed();
        animator.SetLayerWeight(1, 0);
        thisTransform.rotation = m_current_GrabObject.endTarget.rotation;
        MouseLook.ClampHorizontalRotation = false;
        MouseLook.MaxX = m_stored_maxX;

        if (!Crouched)
        {
            Controller.enabled = true;
            Controller.center = DefaultSettings.ControllerCenter;
            Controller.height = DefaultSettings.ControllerHeight;
            Controller.detectCollisions = true;
        }

        thisTransform.position = m_target_EndClimbPos;

        Climbing = false;
        Hanging = false;
        CheckGround = true;
        HandIK.DisableHandIK();
        m_current_GrabObject = null;
        LastHangObject = null;

        if (DebugSettings.DebugHang)
        {
            Debug.Log("End Climb");
        }
    }
    private void CancelHang()
    {
        m_gravityForceV.Set(0, 0, 0);
        HandIK.StopFollowingTargets();
        HandIK.ResetWeightSpeed();
        animator.SetLayerWeight(1, 0);
        HandIK.SetElbows(null, null, false);

        MouseLook.CharacterTargetRot.Set(0, thisTransform.rotation.eulerAngles.y, 0);
        MouseLook.ClampHorizontalRotation = false;
        MouseLook.MaxX = m_stored_maxX;
        Hanging = false;
        CheckGround = true;
        HandIK.DisableHandIK();
        Controller.detectCollisions = true;
        animator.SetBool(AnimationHashUtility.Hanging, false);
        animator.SetBool(AnimationHashUtility.FarFromGround, true);
        animator.SetBool(AnimationHashUtility.OnGround, false);
        animator.SetBool(AnimationHashUtility.PlayerFalling, true);
        animator.SetTrigger(AnimationHashUtility.CancelHang);
        m_current_GrabObject = null;
        StartFalling();
        if (DebugSettings.DebugHang)
        {
            Debug.Log("Cancel Hang");
        }
    }
    private void PlaceHandOnHangTarget()
    {
        HandIK.SetTargetPositions(m_current_GrabObject.leftHandIKTarget.position, m_current_GrabObject.rightHandIKTarget.position, false);
        HandIK.SetTargetsParent(m_current_GrabObject.transform);
        HandIK.SetTargetRotations(m_current_GrabObject.leftHandIKTarget.rotation, m_current_GrabObject.rightHandIKTarget.rotation);
        HandIK.WeightSpeed = 1;
        HandIK.StartFollowingTargets(true, true);
    }

    private GrabableObject ClosestNeighboor(Vector2 input, GrabableObject grabableObject)
    {
        foreach (GrabableObjectNeighBoor neighBoor in grabableObject.Neighboors)
        {
            if (neighBoor.Direction == input)
            {
                return neighBoor.grabableObject;
            }
        }

        return null;
    }
}
