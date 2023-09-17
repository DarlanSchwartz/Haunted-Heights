using UnityEngine;

public static class AnimationHashUtility
{
    public readonly static int Vertical = Animator.StringToHash("Vertical");
    public readonly static int Horizontal = Animator.StringToHash("Horizontal");
    public readonly static int FallHeight = Animator.StringToHash("FallHeight");

    public readonly static int Jump = Animator.StringToHash("Jump");
    public readonly static int Land = Animator.StringToHash("Land");
    public readonly static int Vault = Animator.StringToHash("Vault");
    public readonly static int JumpOnto = Animator.StringToHash("JumpOnto");
    public readonly static int JumpingOnto = Animator.StringToHash("JumpingOnto");
    public readonly static int Hanging = Animator.StringToHash("Hanging");
    public readonly static int HangType = Animator.StringToHash("HangType");
    public readonly static int CancelHang = Animator.StringToHash("CancelHang");
    public readonly static int Climb = Animator.StringToHash("Climb"); 
    public readonly static int FreeHangStage = Animator.StringToHash("FreeHangStage");
    public readonly static int Slide = Animator.StringToHash("Slide");
    public readonly static int EndSlide = Animator.StringToHash("EndSlide");
    public readonly static int HardLanding = Animator.StringToHash("HardLanding");
    public readonly static int RollLanding = Animator.StringToHash("RollLanding");

    public readonly static int MotionTimeDelta = Animator.StringToHash("MotionTimeDelta");
    public readonly static int Balance = Animator.StringToHash("InBalanceMode");
    public readonly static int StartBalance = Animator.StringToHash("StartBalance");

    public readonly static int ClimbLadderIdle = Animator.StringToHash("ClimbLadderIdle");

    public readonly static int RightHandIKWeight = Animator.StringToHash("RightHandIKWeight");
    public readonly static int LeftHandIKWeight = Animator.StringToHash("LeftHandIKWeight");


    public readonly static int PlayerFalling = Animator.StringToHash("PlayerFalling");
    public readonly static int Jumping = Animator.StringToHash("Jumping");
    public readonly static int OnGround = Animator.StringToHash("OnGround");
    public readonly static int Crouched = Animator.StringToHash("Crouched");
    public readonly static int Prone = Animator.StringToHash("Prone");
    public readonly static int Idle = Animator.StringToHash("Idle");
    public readonly static int VaultRun = Animator.StringToHash("VaultRun");
    public readonly static int VaultRight = Animator.StringToHash("VaultRight");
    public readonly static int MonkeyVault = Animator.StringToHash("MonkeyVault");
    public readonly static int FarFromGround = Animator.StringToHash("FarFromGround");

    
    public readonly static int Vaulting = Animator.StringToHash("Vaulting");

    public readonly static int PlayingFallingAnimation = Animator.StringToHash("PlayingFallingAnimation");
    public readonly static int PlayingLandAnimation = Animator.StringToHash("PlayingLandAnimation");
    public readonly static int PlayingVaultAnimation = Animator.StringToHash("PlayingVaultAnimation");
    public readonly static int PlayingClimbAnimation = Animator.StringToHash("PlayingClimbAnimation");
    public readonly static int PlayingJumpAnimation = Animator.StringToHash("PlayingJumpAnimation");
    public readonly static int PlayingSlideAnimation = Animator.StringToHash("PlayingSlideAnimation");
    public readonly static int PlayingHangStartAnimation = Animator.StringToHash("PlayingHangStartAnimation");

    public readonly static int LeftFootCurve = Animator.StringToHash("LeftFootCurve");
    public readonly static int RightFootCurve = Animator.StringToHash("RightFootCurve");

}
