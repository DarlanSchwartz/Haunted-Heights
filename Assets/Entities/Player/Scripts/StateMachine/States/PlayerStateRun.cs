using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateRun : PlayerState
{
    public PlayerStateRun(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Running) { }
    public override void CheckSwitchStates()
    {
        if (Input.GetKeyDown(Context.slideKey) && Context.CanSlide)
        {
            SwitchState(Factory.Slide());
            return;
        }

        if (Input.GetKeyDown(Context.proneKey) && Context.CanProne)
        {
            SwitchState(Factory.Prone());
            return;
        }

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

        if (!Context.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
        else if (Context.IsMovementPressed && !Context.PressingRunKey)
        {
            SwitchState(Factory.Walk());
        }
    }

    public override void EnterState()
    {
        Context.Animator.SetBool(AnimationHashUtility.Stand, true);
        Debug.Log("Enter run");
        Context.Controller.height = Context.DefaultSettings.ControllerHeight;
        Context.Controller.center = Context.DefaultSettings.ControllerCenter;
        Context.Controller.stepOffset = Context.DefaultSettings.ControllerStepOffset;
        Context.Controller.radius = Context.DefaultSettings.ControllerRadius;
        // Debug.Log("run");
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
        Context.MoveSimple(Context.Speed.RunSpeed);
        Context.UpdateGroundedDirectionalAnimations(2);
        Context.HandleMouseLook(true, false);
        Context.HandleFovChange();
        Context.SetPlayerCameraPosition(Context.Speed.CameraSpeed);
    }
}
