using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCrouched : PlayerState
{
    public PlayerStateCrouched(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Crouched) {  }
    public override void CheckSwitchStates()
    {
        if (Context.IsJumpPressed)
        {
            if (!Context.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else if (Context.IsMovementPressed && !Context.PressingRunKey)
            {
                SwitchState(Factory.Walk());
            }
            else
            {
                SwitchState(Factory.Run());
            }
        }

        switch (Context.CrouchSettings.Mode)
        {
            case CrouchMode.Hold:
                if (!Input.GetKey(Context.crouchKey))
                {
                    if (!Context.IsMovementPressed)
                    {
                        SwitchState(Factory.Idle());
                    }
                    else if (Context.IsMovementPressed && !Context.PressingRunKey)
                    {
                        SwitchState(Factory.Walk());
                    }
                    else
                    {
                        SwitchState(Factory.Run());
                    }

                }
                break;
            case CrouchMode.Toggle:
               
                if (Input.GetKeyDown(Context.crouchKey))
                {
                    if (!Context.IsMovementPressed)
                    {
                        SwitchState(Factory.Idle());
                    }
                    else if (Context.IsMovementPressed && !Context.PressingRunKey)
                    {
                        SwitchState(Factory.Walk());
                    }
                    else
                    {
                        SwitchState(Factory.Run());
                    }
                }
                break;
        }
    }

    public override void EnterState()
    {
        if (Context.CrouchSettings.UseStamina)
        {
            Context.CrouchStamina -= Context.CrouchSettings.StaminaDecreasePerCrouch;
        }

        Context.Controller.height = Context.CrouchSettings.ControllerHeight;
        Context.Controller.center = Context.CrouchSettings.ControllerCenter;
        Context.Controller.stepOffset = Context.CrouchSettings.ControllerStepOffset;
        Context.Animator.SetBool(AnimationHashUtility.Crouched, true);
        Context.Animator.SetBool(AnimationHashUtility.Stand, false);
        /// Debug.Log("crouched");
    }
    public override void ExitState()
    {
        Context.Animator.SetBool(AnimationHashUtility.Crouched, false);
        ///Debug.Log("end crouch");
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
        Context.MoveSimple(Context.Speed.CrouchedSpeed);
        Context.UpdateGroundedDirectionalAnimations(2);
        Context.HandleMouseLook(true, false);
        Context.SetPlayerCameraPosition(Context.Speed.CrouchCameraSpeed);
    }
}
