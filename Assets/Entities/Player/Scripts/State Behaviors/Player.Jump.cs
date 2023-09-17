using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool Jumping { get; private set; } = false;

    public bool CheckGround { get; private set; } = true;
    public Vector3 CheckSpherePosition
    {
        get
        {
            if (!Vaulting)
            {
                m_checkSpherePosition = ColliderBotton;

                m_checkSpherePosition.y += 0.2f;
            }
            else
            {
                m_checkSpherePosition = thisTransform.position;
            }

            return m_checkSpherePosition;
        }
    }
    public bool GroundHit { get { return Physics.CheckSphere(CheckSpherePosition, Controller.radius, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore); } }

    public bool CanJump
    {
        get
        {
            if (CanHang || CanVault || CanJumpOnto || Hanging || Climbing || HasSomethingAboveHead || PlayingFallingAnimation || Jumping || Vaulting || Falling || Sliding || Landing)
            {
                if (DebugSettings.DebugCanJump)
                {
                    string reason = "";

                    reason += Jumping ? " Jumping" : "";
                    reason += Vaulting ? " Vaulting" : "";
                    reason += Vaulting ? " Falling" : "";
                    reason += Sliding ? " Sliding" : "";
                    reason += PlayingFallingAnimation ? " playing Falling Animation" : "";
                    reason += HasSomethingAboveHead ? " has something above head" : "";
                    reason += Landing ? " is playing Landing Anim" : "";

                    Debug.Log("Cannot jump because is" + reason);

                    if (!GroundHit && !Controller.isGrounded && FarFromGround)
                    {
                        Debug.Log("Cannot jump because is not grounded");
                        Debug.Log("Ground hits = " + GroundHit);
                        Debug.Log("Far from ground = " + FarFromGround);
                    }

                }

                return false;
            }

            return true;
        }
    }
    public void Jump()
    {
        if (Crouched || SetAnimatorCrouched)
        {
            if (!Input.GetKey(crouchKey))
            {
                Crouched = false;
            }

            return;
        }

        if(InProneState)
        {
            CancelProne();
            return;
        }

        Jumping = true;
        CheckGround = false;
        animator.SetFloat(AnimationHashUtility.FallHeight, 0);
        animator.SetBool(AnimationHashUtility.Jumping, true);
        animator.SetTrigger(AnimationHashUtility.Jump);
        animator.ResetTrigger(AnimationHashUtility.Land);
        m_gravityForceV = Vector3.zero;
        StartCoroutine(JumpEventSimple());

        if (DebugSettings.DebugJump)
        {
            Debug.Log("Jump Started!");
        }
    }

    private IEnumerator JumpEventSimple()
    {
        Controller.slopeLimit = 90;
        Controller.center = FallSettings.ControllerCenter;
        Controller.height = FallSettings.ControllerHeight;
        Controller.stepOffset = JumpSettings.ControllerStepOffset;
        float minimumJumpTime = 1;
        float jumpTime = 0;
        float jumpForce;
        do
        {
            jumpForce = JumpSettings.FallOff.Evaluate(jumpTime);

            if (Hanging)
            {
                Jumping = false;

                animator.SetBool(AnimationHashUtility.Jumping, false);

                if (DebugSettings.DebugJump)
                {
                    Debug.Log("Jump ended. Reason: Started Grabbing during jump.");
                }

                yield break;
            }

            Controller.Move(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * (GetTargetSpeed / FallSettings.AirResistance) * FallSettings.AirControlFactor * Time.deltaTime);

            Controller.Move(jumpForce * JumpSettings.Force * Time.deltaTime * Vector3.up);

            jumpTime += Time.deltaTime;

            if (jumpTime >= minimumJumpTime)
            {
                CheckGround = true;
            }
            else
            {
                if (OnGround && PlayingFallingAnimation)
                {
                    Land(true);

                    if (DebugSettings.DebugLand)
                    {
                        Debug.Log("Land call from jump inter");
                    }

                    yield break;
                }
            }

            yield return null;
        }
        while (!OnGround && !HasSomethingAboveHead && jumpTime <= 1 && jumpForce > 0.2f);

        if (OnGround && PlayingFallingAnimation)
        {
            Land(true);

            if (DebugSettings.DebugLand)
            {
                Debug.Log("Land call from jump");
            }
        }
        else
        {
            Jumping = false;

            animator.SetBool(AnimationHashUtility.Jumping, false);

            CheckGround = true;

            if (DebugSettings.DebugJump)
            {
                string reason = HasSomethingAboveHead ? "Had something above head" : "Jumptime end: " + jumpTime;
                Debug.Log("Jump ended. Reason: " + reason);
            }
        }

        yield break;
    }
}
