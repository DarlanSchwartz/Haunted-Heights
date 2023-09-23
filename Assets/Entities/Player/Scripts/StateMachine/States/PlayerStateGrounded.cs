using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HighDefinition.CameraSettings;

public class PlayerStateGrounded : PlayerState
{
    public PlayerStateGrounded(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory,PlayerStateType.Grounded)
    {
        IsRootState = true;
    }
    public override void CheckSwitchStates()
    {
        if (Context.IsJumpPressed && !Context.IsCrouched && !Context.IsProne)
        {
            if (Context.CanVault)
            {
                SwitchState(Factory.Vault());
                return;
            }
            if (Context.CanJumpOnto)
            {
                SwitchState(Factory.JumpOnto());
                return;
            }

            if (Context.CanJump)
            {
                SwitchState(Factory.Jump());
            }
        }
    }

    public override void EnterState()
    {
        InitializeSubState();
    }
    public override void UpdateState()
    {
        Context.LastGroundedPositionY = Context.ThisTransform.position.y;

        if (!Context.OnGround && !Context.IsJumping)
        {
            Context.TimeWaitingToFall += Time.deltaTime;
            if (Context.FallSettings.MinFallTime == 0 || Context.TimeWaitingToFall >= Context.FallSettings.MinFallTime)
            {
                SwitchState(Factory.Falling());
                return;
            }
        }

        RegenerateCrouchStamina();
        RegenerateProneStamina();
        CheckSwitchStates();
    }

    private void RegenerateCrouchStamina()
    {
        if (Context.CrouchSettings.UseStamina && Context.CrouchStamina < Context.CrouchSettings.MaxStamina)
        {
            Context.CrouchStamina += Context.CrouchSettings.StaminaRegenerationRate * Time.deltaTime;
        }
    }
    private void RegenerateProneStamina()
    {
        if (Context.ProneSettings.UseStamina && Context.ProneStamina < Context.ProneSettings.MaxStamina)
        {
            Context.ProneStamina += Context.ProneSettings.StaminaRegenerationRate * Time.deltaTime;
        }
    }
    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        //Debug.Log("Initialized");
        if (Context.CrouchStamina < Context.CrouchSettings.MinStaminaToCrouch)
        {
            switch (Context.CrouchSettings.Mode)
            {
                case CrouchMode.Hold:
                    if (Input.GetKey(Context.crouchKey))
                    {
                        SetSubState(Factory.Crouched());
                        return;
                    }
                    break;
                case CrouchMode.Toggle:
                    if (Input.GetKeyDown(Context.crouchKey))
                    {
                        SetSubState(Factory.Crouched());
                        return;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(Context.proneKey) && Context.CanProne)
        {
            SetSubState(Factory.Prone());
            return;
        }

        if (!Context.IsMovementPressed)
        {
            SetSubState(Factory.Idle());
            CurrentSubState.EnterState();
            return;
        }
        else if (Context.IsMovementPressed && !Context.PressingRunKey)
        {
            SetSubState(Factory.Walk());
            CurrentSubState.EnterState();
            return;
        }
        else
        {
            SetSubState(Factory.Run());
            CurrentSubState.EnterState();
            return;
        }
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
