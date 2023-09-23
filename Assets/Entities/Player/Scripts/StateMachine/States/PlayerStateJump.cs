using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Tilemaps.Tile;

public class PlayerStateJump : PlayerState
{
    readonly float minimumJumpTime = 0.2f;
    readonly float maximumJumpTime = 0.5f;
    float jumpTime = 0;
    float jumpForce;
    bool HasSomethingAboveHead = false;
    Vector3 strafeMovement = Vector3.zero;
    Vector3 jumpMovement = Vector3.zero;
    Vector3 offsetTop = new(0, 0.2f, 0);
    public PlayerStateJump(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Jumping) { IsRootState = true; }
    public override void CheckSwitchStates(){}
    public override void EnterState()
    {
        Debug.Log("Jump");
        jumpTime = 0;
        jumpForce = 0;
        Context.IsJumping = true;
        Context.CheckGround = false;
        Context.GravityForce = Vector3.zero;

        Context.Controller.slopeLimit = 90;
        Context.Controller.center = Context.FallSettings.ControllerCenter;
        Context.Controller.height = Context.FallSettings.ControllerHeight;
        Context.Controller.stepOffset = Context.JumpSettings.ControllerStepOffset;

        Context.Animator.SetBool(AnimationHashUtility.Stand, false);
        Context.Animator.SetBool(AnimationHashUtility.Jumping, true);
        //Check if is running to change jump animations here
        Context.Animator.Play("Jump_Idle_Action", 0);
        Context.StartCoroutine(JumpEventSimple());
    }

    private IEnumerator JumpEventSimple()
    {
        do
        {
            jumpTime += Time.deltaTime;
            HasSomethingAboveHead = Physics.SphereCast(Context.ColliderTop + offsetTop, Context.Controller.radius, Vector3.up, out _, Context.JumpSettings.HeadHitAboveRayLenght);
            jumpForce = Context.JumpSettings.FallOff.Evaluate(jumpTime);
            jumpMovement = jumpForce * Context.JumpSettings.Force * Time.deltaTime * Vector3.up;
            strafeMovement = ((Context.ForwardMovement + Context.RightMovement) * (Context.PressingRunKey ? Context.Speed.RunSpeed : Context.Speed.WalkSpeed)) * Time.deltaTime;
            Context.Controller.Move(jumpMovement + strafeMovement);
            Context.HandleMouseLook(true, false);
            if (jumpTime >= minimumJumpTime && (HasSomethingAboveHead || Context.GroundHit))
            {
                if (Context.CanStartToHang)
                {
                    SwitchState(Factory.Climb());
                    yield break;
                }
                if (Context.GroundHit)
                {
                    SwitchState(Factory.Land());
                }
                else
                {
                    SwitchState(Factory.Falling());
                }
                if (Context.DebugSettings.DebugJump)
                {
                    string reason = "End jump, Reason: ";
                    reason += jumpTime >= minimumJumpTime ? "Time ended and:" : "";
                    reason += HasSomethingAboveHead ? "Has something above the head" : "Is on ground";
                    Debug.Log(reason);
                }
               
                yield break;
            }

            yield return null;
        }
        while (jumpTime < maximumJumpTime);
       
        if (Context.GroundHit)
        {
            SwitchState(Factory.Land());
        }
        else
        {
            SwitchState(Factory.Falling());
        }

        yield break;
    }

    public override void ExitState()
    {
        Context.CheckGround = true;
        Context.IsJumping = false;
        Context.Animator.SetBool(AnimationHashUtility.Jumping, false);
    }

    public override void InitializeSubState() { }
    public override void OnCollisionEnter() { }
    public override void UpdateState() { }
    public override void OnCollisionExit() { }
    public override void OnCollisionStay() { }
}
