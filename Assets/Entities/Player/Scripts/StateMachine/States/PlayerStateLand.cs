using UnityEngine;
public enum LandState { Roll, Hard, Medium, Soft , None};
public class PlayerStateLand : PlayerState
{
    LandState landState = LandState.Soft;
    float exitTime = 1;
    float currentTime = 0;
    Vector3 hardLandingCameraTargetRotation;

    readonly float hardLandingCameraForcedRotationSpeed = 2;
    readonly float rollLandingMouseLookMaxX = 30;
    readonly float rollLandingMouseLookMinX = -30;

    readonly string rollAnimationName = "Roll";
    readonly string hardLandingAnimationName = "Land_Hard";
    readonly string mediumLandingAnimationName = "Land_Medium";
    readonly string softLandingAnimationName = "Land_Soft";
    public PlayerStateLand(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Landing) { IsRootState = true; }
    public override void CheckSwitchStates()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= exitTime)
        {
            SwitchState(Factory.Grounded());
        }
    }

    public override void EnterState()
    {
        Context.CheckGround = true;
        Context.Falling = false;
        Context.TimeWaitingToFall = 0;
        currentTime = 0;
        Context.LastHangObject = null;

        Context.Controller.slopeLimit = Context.DefaultSettings.ControllerSlopeLimit;
        Context.Controller.center = Context.DefaultSettings.ControllerCenter;
        Context.Controller.height = Context.DefaultSettings.ControllerHeight;
        Context.Controller.stepOffset = Context.DefaultSettings.ControllerStepOffset;
        Context.Animator.SetBool(AnimationHashUtility.Stand, true);
        Context.Animator.SetBool(AnimationHashUtility.Landing, true);

        Context.GravityForce = Vector3.zero;
        float totalFallHeight = Context.LastGroundedPositionY - Context.ThisTransform.position.y;
        if (Context.PlayingFallingAnimation)
        {
            Context.Animator.SetBool(AnimationHashUtility.Falling, false);
            if (totalFallHeight >= 8)
            {
                Context.Animator.Play(rollAnimationName, 0);
                
                landState = LandState.Roll;
                Context.MouseLook.MaxX = rollLandingMouseLookMaxX;
                Context.MouseLook.MinX = -rollLandingMouseLookMinX;
                exitTime = 1;
                return;
            }
            if (totalFallHeight >= 6)
            {
                Context.Animator.Play(hardLandingAnimationName, 0);
                landState = LandState.Hard;
                exitTime =1;
                hardLandingCameraTargetRotation = new(70, 0, 0);
                return;
            }
            if (totalFallHeight >= 4)
            {
                Context.Animator.Play(mediumLandingAnimationName, 0);
                landState = LandState.Medium;
                exitTime = 0.6f;
                return;
            }

            Context.Animator.Play(softLandingAnimationName, 0);
            landState = LandState.Soft;
            exitTime = 0.3f;
        }
        else
        {
            landState = LandState.None;
            exitTime = 0.3f;
        }
    }

    public override void UpdateState()
    {
        Context.SetPlayerCameraPosition(Context.Speed.LandCameraSpeed);
        
        switch (landState)
        {
            case LandState.Roll:
                Context.MoveSimple(Context.Speed.WalkSpeed);
                Context.UpdateGroundedDirectionalAnimations(1);
                Context.HandleMouseLook(true, true);
                break;
            case LandState.Hard:
                Context.MouseLook.CameraTargetRot = Vector3.Lerp(Context.MouseLook.CameraTargetRot, hardLandingCameraTargetRotation, hardLandingCameraForcedRotationSpeed * Time.deltaTime);
                Context.HandleMouseLook(false, false);
                break;
            case LandState.Medium:
                break;
            case LandState.Soft:
                Context.MoveSimple(Context.Speed.WalkSpeed);
                Context.UpdateGroundedDirectionalAnimations(1);
                Context.HandleMouseLook(true, false);
                break;
            default:
                Context.UpdateGroundedDirectionalAnimations(1);
                Context.MoveSimple(Context.PressingRunKey ? Context.Speed.RunSpeed : Context.Speed.WalkSpeed);
                Context.HandleMouseLook(true, false);
                break;
        }
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Context.MouseLook.MaxX = 75;
        Context.MouseLook.MinX = -90;
        Context.Animator.SetBool(AnimationHashUtility.Landing, false);
    }

    public override void InitializeSubState() { }
    public override void OnCollisionEnter() { }
    public override void OnCollisionExit() { }
    public override void OnCollisionStay() { }
}
