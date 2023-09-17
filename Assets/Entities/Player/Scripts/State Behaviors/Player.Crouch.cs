using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool WantToGetUp { get; private set; } = false;

    public bool Crouched
    {
        get
        {
            return m_crouched;
        }
        set
        {
            if (Jumping || Falling)
            {
                return;
            }

            if (value == true && !m_crouched)
            {
                Crouch();

                return;
            }
            else if (value == false && m_crouched && !HasSomethingAboveHead)
            {
                CancelCrouch();
            }
        }
    }

    private void Crouch()
    {
        if (CrouchSettings.UseStamina)
        {
            if (m_currentCrouchStamina < CrouchSettings.MinStaminaToCrouch)
            {
                return;
            }
            else
            {
                m_currentCrouchStamina -= CrouchSettings.StaminaDecreasePerCrouch;
            }
        }

        m_crouched = true;

        WantToGetUp = false;
        SetAnimatorCrouched = true;
        FarFromGround = false;
        Controller.height = CrouchSettings.ControllerHeight;
        Controller.center = CrouchSettings.ControllerCenter;
        Controller.stepOffset = CrouchSettings.ControllerStepOffset;
    }

    private void CancelCrouch()
    {
        m_crouched = false;

        WantToGetUp = false;
        SetAnimatorCrouched = false;
        FarFromGround = false;
        Controller.height = DefaultSettings.ControllerHeight;
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;
    }

    public bool SetAnimatorCrouched
    {
        get
        {
            return animator.GetBool(AnimationHashUtility.Crouched);
        }
        set
        {
            animator.SetBool(AnimationHashUtility.Crouched, value);
        }
    }
}
