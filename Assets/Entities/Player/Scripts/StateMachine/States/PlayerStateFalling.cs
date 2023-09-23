using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HighDefinition.CameraSettings;

public class PlayerStateFalling : PlayerState
{
    public PlayerStateFalling(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory , PlayerStateType.Falling) { IsRootState = true; }
    Vector3 strafeMovement = Vector3.zero;
    Vector3 gravity = Vector3.zero;

    public override void EnterState()
    {
        Context.TimeWaitingToFall = 0;
        Context.Falling = true;
        Context.Controller.center = Context.FallSettings.ControllerCenter;
        Context.Controller.height = Context.FallSettings.ControllerHeight;
        Context.Controller.stepOffset = Context.JumpSettings.ControllerStepOffset;
        Context.Controller.radius = Context.DefaultSettings.ControllerRadius;
        Context.GravityForce = Vector3.zero;

        if (!Physics.Raycast(Context.MiddleRayStart, Vector3.down, out _, Context.FallSettings.MinFallDistance))
        {
            Context.Animator.SetBool(AnimationHashUtility.Stand, false);
            Context.Animator.SetBool(AnimationHashUtility.Falling, true);
        }
    }

    public override void UpdateState()
    {
        Context.HandleMouseLook(true, false);
        Context.SetPlayerCameraPosition(Context.Speed.FallingCameraSpeed);
        strafeMovement = ((Context.ForwardMovement + Context.RightMovement) * (Context.PressingRunKey ? Context.Speed.RunSpeed : Context.Speed.WalkSpeed)) * Time.deltaTime;
        gravity = Context.GravityForce * Time.deltaTime;

        if ((Context.GravityForce + Context.FallSettings.GravityForce * Time.deltaTime * Vector3.down).y <= Context.FallSettings.GravityForce)
        {
            Context.GravityForce += Context.FallSettings.GravityForce * Time.deltaTime * Vector3.down;
        }
        else
        {
            Context.GravityForce = new Vector3(0, Context.FallSettings.GravityForce, 0);
        }
        Context.Controller.Move(gravity + strafeMovement);
        CheckSwitchStates();
        // Debug.Log("Falling");
    }

    public override void ExitState()
    {
        Context.GravityForce = Vector3.zero;
        Context.Falling = false;
        //Debug.Log("End falling");
    }

    public override void CheckSwitchStates()
    {
        if (Context.OnGround)
        {
            SwitchState(Factory.Land());
            return;
        }

        if (Context.CanStartToHang)
        {
            SwitchState(Factory.Climb());
            return;
        }
    }

    public override void InitializeSubState()
    {
        
    }

    public override void OnCollisionEnter()
    {
        
    }

    public override void OnCollisionExit()
    {
        
    }

    public override void OnCollisionStay()
    {
        
    }

   
}
