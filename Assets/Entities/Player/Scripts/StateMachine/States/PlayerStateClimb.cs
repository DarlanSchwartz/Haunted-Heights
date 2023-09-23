using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateClimb : PlayerState
{
    float storedMouseLookMaxX;
    GrabableObject currentGrabableObject;
    Vector3 startClimbTargetPosition;
    Vector3 startClimbTargetRotation;
    Vector3 endClimbTargetPosition;
    Vector3 m_hangingInput;
    bool m_hangingon_DynamicObject;
    BehaviourClimbUp behaviourClimbUp; 
    Vector3 offsetTop = new(0, 0.2f, 0);
    public PlayerStateClimb(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory): base (currentContext,playerStateFactory, PlayerStateType.Climbing) { IsRootState = true; }
    public override void CheckSwitchStates() { }
    public override void EnterState() 
    {
        StartHang();
    }
    public override void ExitState() 
    {
        Context.GravityForce.Set(0, 0, 0);
        Context.HandIK.StopFollowingTargets();
        Context.HandIK.ResetWeightSpeed();
        Context.Animator.SetLayerWeight(1, 0);
        Context.HandIK.SetElbows(null, null, false);

        Context.MouseLook.CharacterTargetRot.Set(0, Context.ThisTransform.rotation.eulerAngles.y, 0);
        Context.MouseLook.ClampHorizontalRotation = false;
        Context.MouseLook.MaxX = storedMouseLookMaxX;
        Context.Hanging = false;
        Context.CheckGround = true;
        Context.HandIK.DisableHandIK();
        Context.Controller.detectCollisions = true;
        Context.Animator.SetBool(AnimationHashUtility.Hanging, false);
        currentGrabableObject = null;
    }
    public override void InitializeSubState() { }
    public override void OnCollisionEnter() { }
    public override void OnCollisionExit() { }
    public override void OnCollisionStay() { }
    public override void UpdateState() { }

    private void StartHang() {
        bool isAFreeHangObject;
        Context.LastHangObject = Context.CameraForwardHit.transform;
        currentGrabableObject = Context.CameraForwardHit.transform.GetComponent<GrabableObject>();

        startClimbTargetPosition = currentGrabableObject.startTarget.position;
        startClimbTargetRotation = currentGrabableObject.startTarget.eulerAngles;
        endClimbTargetPosition = currentGrabableObject.endTarget.position;

        storedMouseLookMaxX = Context.MouseLook.MaxX;

        Context.MouseLook.ClampHorizontalRotation = true;
        Context.MouseLook.MaxY = 83;
        Context.MouseLook.MinY = -83;

        Context.HandIK.LeftWeight = 0;
        Context.HandIK.RightWeight = 0;

        Context.Controller.center = Context.ClimbSettings.ControllerCenter;
        Context.Controller.height = Context.ClimbSettings.ControllerHeight;
        Context.Controller.detectCollisions = false;
        Context.Controller.minMoveDistance = 0;
        Context.Controller.skinWidth = 0.001f;
        Context.Controller.stepOffset = Context.DefaultSettings.ControllerStepOffset;
        Context.Controller.slopeLimit = Context.DefaultSettings.ControllerSlopeLimit;

        switch (currentGrabableObject.Movement)
        {
            case BragableMovementType.Static:
                m_hangingon_DynamicObject = false;
                break;
            case BragableMovementType.Dynamic:
                Context.ThisTransform.SetParent(currentGrabableObject.transform.parent);
                m_hangingon_DynamicObject = true;
                break;
        }

        switch (currentGrabableObject.GrabType)
        {
            case GrabTypes.FreeHang:
                //play free hang
                Context.Animator.Play("Jump To Freehang", 0);
                Context.HandIK.SetBodyLook(currentGrabableObject.LookAtHanging.position, true, 1);
                Context.HandIK.SetElbows(currentGrabableObject.leftElbowTarget, currentGrabableObject.rightElbowTarget, true);
                behaviourClimbUp = GetCorrectClimbBehaviour(ClimbMode.FromFreeHang);
                isAFreeHangObject = true;
                break;
            case GrabTypes.BracedHang:
                //play braced hang
                Context.Animator.Play("Jump to Bracedhang", 0);
                Context.HandIK.SetBodyLook(currentGrabableObject.startTarget.position + Context.ClimbSettings.StartOffset, true, 1);
                behaviourClimbUp = GetCorrectClimbBehaviour(ClimbMode.FromBracedHang);
                isAFreeHangObject = false;
                break;
            default:
                Context.Animator.Play("Jump to Bracedhang", 0);
                Context.HandIK.SetBodyLook(currentGrabableObject.startTarget.position + Context.ClimbSettings.StartOffset, true, 1);
                behaviourClimbUp = GetCorrectClimbBehaviour(ClimbMode.FromBracedHang);
                isAFreeHangObject = false;
                break;
        }

        Context.Animator.SetLayerWeight(1, 1);
        Context.HandIK.SetWeightSpeed(2);
        Context.Hanging = true;
        Context.CheckGround = false;
        Context.Animator.SetBool(AnimationHashUtility.Stand, false);
        Context.Animator.SetBool(AnimationHashUtility.Falling, false);
        Context.Animator.SetBool(AnimationHashUtility.Hanging, true);

        if (currentGrabableObject.fixedPositions)
        {
            PlaceHandOnHangTarget();
        }

        Context.StartCoroutine(Hang(GetBehaviourGrab(isAFreeHangObject ? 1 : 0)));
    }

    public IEnumerator Hang(AnimationBehaviorGrab grabAnim)
    {
        bool freeHang = grabAnim.animationID != 0;
        float maxX = freeHang ? Context.ClimbSettings.MouseLookMaxXFreeHang : Context.ClimbSettings.MouseLookMaxXGrab;

        // Go to climb point
        do
        {
            Context.ThisTransform.SetPositionAndRotation(Vector3.MoveTowards(Context.ThisTransform.position, startClimbTargetPosition, Context.Speed.HangSpeed * Time.deltaTime), Quaternion.Lerp(Context.ThisTransform.rotation, Quaternion.Euler(startClimbTargetRotation), Context.Speed.HangSpeed * 5 * Time.deltaTime));
            Context.HandIK.LeftWeight = Mathf.LerpUnclamped(Context.HandIK.LeftWeight, 1, 5 * Time.deltaTime);
            Context.HandIK.RightWeight = Mathf.LerpUnclamped(Context.HandIK.RightWeight, 1, 5 * Time.deltaTime);

            if (freeHang)
            {
                if (Context.MouseLook.MaxX > maxX)
                {
                    Context.MouseLook.MaxX -= 5 * Time.deltaTime;
                }
                else
                {
                    Context.MouseLook.MaxX = maxX;
                }

                Context.MouseLook.MaxX = Mathf.Clamp(Context.MouseLook.MaxX, 0, maxX);
            }

            yield return null;
        } while (!grabAnim.Complete && Vector3.Distance(Context.ThisTransform.position, startClimbTargetPosition) > 0.1f && Context.MouseLook.MaxX < maxX);

        Context.ThisTransform.position = currentGrabableObject.startTarget.position;
        //GoingToHangTarget = false;

        // Check inputs to cancel grab or climb up
        do
        {
            // Check inputs and look for neighboos
            if (currentGrabableObject.Neighboors.Count > 0)
            {
                m_hangingInput.x = Context.HorizontalInput;
                m_hangingInput.y = Context.VerticalInput;
            }

            Context.MouseLook.LookRotation(false);
            Context.SetPlayerCameraPosition(Context.Speed.HangCameraSpeed);

            SetHandsIkOnClimbTarget();

            if (Input.GetKeyDown(Context.crouchKey) || Input.GetKey(Context.crouchKey) || Input.GetKeyUp(Context.crouchKey) || Context.VerticalInput < -0.1f)
            {
                SwitchState(Factory.Falling());
                yield break;
            }
            else if (endClimbTargetPosition != Vector3.zero && (Input.GetKeyDown(Context.jumpKey) || Input.GetKeyDown(KeyCode.W)))
            {
                StartClimb(freeHang);
                yield break;
            }


            yield return null;

        } while (true);
    }

    private void StartClimb(bool isFreehangObject)
    {
        if (isFreehangObject)
        {
            SetHandsIkOnClimbTarget();
        }

        Context.HandIK.SetElbows(null, null, false);
        Context.Animator.SetBool(AnimationHashUtility.Hanging, false);
        Context.Animator.SetTrigger(AnimationHashUtility.Climb_Up);
        Context.Animator.SetFloat(AnimationHashUtility.Vertical, 0);
        Context.Animator.SetFloat(AnimationHashUtility.Horizontal, 0);
        Context.HandIK.ResetBodyLook();
        Context.Climbing = true;
        Context.StartCoroutine(GoToClimbEndTarget(isFreehangObject));
    }
    private IEnumerator GoToClimbEndTarget(bool fh)
    {
        float speed = fh ? Context.Speed.ClimbFreeSpeed : Context.Speed.ClimbSpeed;
        
        do
        {// TODO : Climb animation behavior to sync animation with this movement
            
            if (Context.MouseLook.MaxX - 5 * Time.deltaTime >= Context.ClimbSettings.MouseLookMaxXGrab)
            {
                Context.MouseLook.MaxX -= 5 * Time.deltaTime;
            }

            if (Context.MouseLook.MaxY > 85)
            {
                Context.MouseLook.MaxX -= 5 * Time.deltaTime;
            }
            else if (Context.MouseLook.MaxY < -85)
            {
                Context.MouseLook.MaxX += 5 * Time.deltaTime;
            }

            //if (HasSomethingAboveHead)
            //{
            //    Crouched = true;
            //}
            Context.SetPlayerCameraPosition(Context.Speed.HangCameraSpeed);
            Context.ThisTransform.position = Vector3.Slerp(Context.ThisTransform.position, endClimbTargetPosition, speed * Time.deltaTime);
            Context.MouseLook.LookRotation(false);
            yield return null;

        } while (Vector3.Distance(Context.ThisTransform.position, endClimbTargetPosition) > 0.05f && !behaviourClimbUp.Complete);
        bool somethingAboveHead = Physics.SphereCast(Context.ColliderTop + offsetTop, Context.Controller.radius, Vector3.up, out _, Context.JumpSettings.HeadHitAboveRayLenght);
        if (somethingAboveHead)
        {
            Debug.Break();
            yield break;
        }

        SwitchState(Factory.Grounded());
        yield break;
    }

    private BehaviourClimbUp GetCorrectClimbBehaviour(ClimbMode mode)
    {
        BehaviourClimbUp[] climbUpBehaviours = Context.Animator.GetBehaviours<BehaviourClimbUp>();
        foreach (BehaviourClimbUp behavior in climbUpBehaviours)
        {
            if (behavior.ClimbMode == mode)
            {
                return behavior;
            }
        }
        Debug.LogError("Something went wrong while trying to find the correct climb behaviour state on" + this);
        Debug.Break();
        return null;
    }

    private void PlaceHandOnHangTarget()
    {
        Context.HandIK.SetTargetPositions(currentGrabableObject.leftHandIKTarget.position, currentGrabableObject.rightHandIKTarget.position, false);
        Context.HandIK.SetTargetsParent(currentGrabableObject.transform);
        Context.HandIK.SetTargetRotations(currentGrabableObject.leftHandIKTarget.rotation, currentGrabableObject.rightHandIKTarget.rotation);
        Context.HandIK.WeightSpeed = 1;
        Context.HandIK.StartFollowingTargets(true, true);
    }
    private void SetHandsIkOnClimbTarget()
    {
        if (ClosestNeighboor(m_hangingInput, currentGrabableObject) != null)
        {
            if (m_hangingInput.x > 0)
            {
                Context.HandIK.SetRightTargetPosition(ClosestNeighboor(m_hangingInput, currentGrabableObject).rightHandIKTarget.position, true);
                Context.HandIK.SetLeftTargetPosition(currentGrabableObject.leftHandIKTarget.position, true);
            }
            else if (m_hangingInput.x < 0)
            {
                Context.HandIK.SetLeftTargetPosition(ClosestNeighboor(m_hangingInput, currentGrabableObject).leftHandIKTarget.position, true);
                Context.HandIK.SetRightTargetPosition(currentGrabableObject.rightHandIKTarget.position, true);
            }
            else
            {
                Context.HandIK.SetLeftTargetPosition(currentGrabableObject.leftHandIKTarget.position, true);
                Context.HandIK.SetRightTargetPosition(currentGrabableObject.rightHandIKTarget.position, true);
            }
        }
        else
        {
            Context.HandIK.SetLeftTargetPosition(currentGrabableObject.leftHandIKTarget.position, true);
            Context.HandIK.SetRightTargetPosition(currentGrabableObject.rightHandIKTarget.position, true);
        }
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
    private AnimationBehaviorGrab GetBehaviourGrab(int id)
    {
        foreach (AnimationBehaviorGrab currentAnimBehaviour in Context.Animator.GetBehaviours<AnimationBehaviorGrab>())
        {
            if (currentAnimBehaviour.animationID == id)
            {
                return currentAnimBehaviour;
            }
        }

        return null;
    }
}
