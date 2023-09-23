using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateProne : PlayerState
{
    public PlayerStateProne(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Proning) { }
    bool hasSomethingAboveHead = false;
    public override void CheckSwitchStates()
    {
        hasSomethingAboveHead = Physics.SphereCast(Context.ThisTransform.position + Context.ProneSettings.ControllerCenter, Context.Controller.radius, Vector3.up, out _, Context.ProneSettings.HeadHitAboveRayLenght);
        if (Input.GetKeyDown(Context.proneKey) || Input.GetKeyDown(Context.jumpKey))
        {
            if (hasSomethingAboveHead)
            {
                return;
            }

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
    }

    public override void EnterState()
    {
        Context.IsProne = true;
        Context.Controller.height = Context.ProneSettings.ControllerHeight;
        Context.Controller.center = Context.ProneSettings.ControllerCenter;
        Context.Controller.stepOffset = Context.ProneSettings.ControllerStepOffset;
        Context.FeetIK.IsEnabled = false;
        Context.Animator.SetBool(AnimationHashUtility.Prone, true);
        Context.Animator.SetBool(AnimationHashUtility.Stand, false);
        ConsumeProneStamina();
        ///Debug.Log("prone");
    }
    public override void ExitState()
    {
        Context.IsProne = false;
        Context.FeetIK.IsEnabled = true;
        Context.Animator.SetBool(AnimationHashUtility.Prone, false);
        /// Debug.Log("end prone");
    }

    private void ConsumeProneStamina()
    {
        if (Context.ProneSettings.UseStamina)
        {
            Context.ProneStamina -= Context.ProneSettings.StaminaDecreasePerProne;
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

    public override void UpdateState()
    {
        CheckSwitchStates();
        Context.MoveSimple(Context.Speed.PronedSpeed);
        Context.UpdateGroundedDirectionalAnimations(1);
        Context.HandleMouseLook(true, false);
        Context.SetPlayerCameraPosition(Context.Speed.CrouchCameraSpeed);
    }
}
