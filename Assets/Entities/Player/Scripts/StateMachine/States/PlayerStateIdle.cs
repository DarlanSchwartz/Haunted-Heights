using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateIdle : PlayerState
{
    public PlayerStateIdle(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory ,PlayerStateType.Idle) { }
    public override void CheckSwitchStates()
    {
        if (Context.CrouchStamina >= Context.CrouchSettings.MinStaminaToCrouch)
        {
            switch (Context.CrouchSettings.Mode)
            {
                case CrouchMode.Hold:
                    if (Input.GetKey(Context.crouchKey))
                    {
                        SwitchState(Factory.Crouched());
                        return;
                    }
                    break;
                case CrouchMode.Toggle:
                    if (Input.GetKeyDown(Context.crouchKey))
                    {
                        SwitchState(Factory.Crouched());
                        return;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(Context.proneKey) && Context.CanProne)
        {
            SwitchState(Factory.Prone());
            return;
        }

        if (Context.PressingRunKey && Context.IsMovementPressed)
        {
            SwitchState(Factory.Run());
        }
        else if(Context.IsMovementPressed)
        {
            SwitchState(Factory.Walk());
        }
    }

    public override void EnterState()
    {
        Context.Animator.SetBool(AnimationHashUtility.Stand,true);
        Context.Controller.height = Context.DefaultSettings.ControllerHeight;
        Context.Controller.center = Context.DefaultSettings.ControllerCenter;
        Context.Controller.stepOffset = Context.DefaultSettings.ControllerStepOffset;
        Context.Controller.radius = Context.DefaultSettings.ControllerRadius;
        //Debug.Log("idle");
    }

    public override void ExitState()
    {
        
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

    public override void UpdateState()
    {
        CheckSwitchStates();
        Context.UpdateGroundedDirectionalAnimations(0);
        Context.HandleMouseLook(true, false);
        Context.HandleFovChange();
        Context.SetPlayerCameraPosition(Context.Speed.IdleCameraSpeed);
    }
}
