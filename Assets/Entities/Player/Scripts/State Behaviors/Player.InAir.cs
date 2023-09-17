using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool OnGround { get { return CheckGround && GroundHit; } }

    public bool Landing { get { return animator.GetBool(AnimationHashUtility.PlayingLandAnimation); } }

    public bool HardLanding { get { return animator.GetBool(AnimationHashUtility.HardLanding); } }
    public bool FarFromGround
    {
        get
        {
            return !Physics.Raycast(MiddleRayStart, Vector3.down, out _, FallSettings.MinFallDistance);
        }
        set
        {
            if (value == true && !Physics.Raycast(MiddleRayStart, Vector3.down, out _, FallSettings.MinFallDistance))
            {
                if (FallSettings.MinFallTime == 0)
                {
                    StartFalling();
                }
            }
        }
    }

    public bool Falling { get; private set; } = false;

    public float LastGroundedPositionY { get; private set; } = 0;

    public float TimeWaitingToFall { get; private set; } = 0;

    public bool PlayingFallingAnimation
    {
        get
        {
            return animator.GetBool(AnimationHashUtility.PlayingFallingAnimation);
        }
        set
        {
            animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, value);
        }
    }

    public Vector3 MiddleRayStart { get { return thisTransform.position + m_groundCheckOffset; } }
    public void StartFalling()
    {
        if (animator.GetBool(AnimationHashUtility.PlayingClimbAnimation) || GoingToHangTarget)
        {
            return;
        }

        TimeWaitingToFall = 0;
        Falling = true;
        animator.SetBool(AnimationHashUtility.FarFromGround, FarFromGround);
        animator.SetBool(AnimationHashUtility.PlayerFalling, true);
        animator.SetBool(AnimationHashUtility.PlayingLandAnimation, false);
        Controller.center = FallSettings.ControllerCenter;
        Controller.height = FallSettings.ControllerHeight;
        Controller.stepOffset = JumpSettings.ControllerStepOffset;
        Controller.radius = DefaultSettings.ControllerRadius;
        if (DebugSettings.DebugFalling)
        {
            Debug.Log("Started falling");
        }
    }

    public void Land(bool triggerAnimation)
    {
        CheckGround = true;
        Jumping = false;
        Falling = false;
        animator.SetBool(AnimationHashUtility.Jumping, false);
        animator.SetBool(AnimationHashUtility.PlayerFalling, false);
        animator.SetBool(AnimationHashUtility.FarFromGround, false);

        TimeWaitingToFall = 0;

        Controller.slopeLimit = DefaultSettings.ControllerSlopeLimit;
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.height = DefaultSettings.ControllerHeight;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;

        m_gravityForceV = Vector3.zero;

        if (triggerAnimation)
        {
            animator.SetTrigger(AnimationHashUtility.Land);
        }

        if (DebugSettings.DebugLand)
        {
            Debug.Log("Landed");
        }

        LastHangObject = null;
    }

    public void CheckLanded()
    {
        if (OnGround)
        {
            if (PlayingFallingAnimation && !Climbing && !Hanging)
            {
                if (DebugSettings.DebugLand)
                {
                    Debug.Log("Land call from Check land.");
                }

                Land(true);
            }
            else if (!PlayingFallingAnimation && !Climbing && !Hanging && Falling)
            {
                Land(false);
            }

            LastGroundedPositionY = thisTransform.position.y;
        }
        else
        {
            animator.SetFloat(AnimationHashUtility.FallHeight, LastGroundedPositionY - thisTransform.position.y);
        }
    }
    private void CheckFalling()
    {
        if (!Crouched && !Jumping && !Falling && FarFromGround && !OnGround && !Sliding && !Vaulting)
        {
            if (FallSettings.MinFallTime == 0)
            {
                StartFalling();
                return;
            }

            if (DebugSettings.DebugFalling && TimeWaitingToFall == 0)
            {
                Debug.Log("Waiting Time to Start Falling");
            }

            TimeWaitingToFall += Time.deltaTime;

            if (TimeWaitingToFall >= FallSettings.MinFallTime)
            {
                StartFalling();
            }

            return;
        }
        else if (Crouched && FarFromGround && !OnGround)
        {
            StartFalling();
        }

        TimeWaitingToFall = 0;
    }
}
