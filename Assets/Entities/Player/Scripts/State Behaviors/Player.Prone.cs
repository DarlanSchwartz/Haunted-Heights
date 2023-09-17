using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    private bool InProneState { get; set; }
    private bool CanProne { get { return !Jumping && !JumpingOnto && !FarFromGround && !Falling && !Climbing && !Hanging && !Landing; } }
    float m_currentProneStamina = 0;

    private void Prone()
    {
        if (InProneState)
        {
            CancelProne();
            return;
        }

        ConsumeProneStamina();
        InProneState = true;
        WantToGetUp = false;
        animator.SetBool(AnimationHashUtility.Prone, true);
        FarFromGround = false;
        Controller.height = ProneSettings.ControllerHeight;
        Controller.center = ProneSettings.ControllerCenter;
        Controller.stepOffset = ProneSettings.ControllerStepOffset;
        FeetIK.IsEnabled = false;
    }

    private void ConsumeProneStamina()
    {
        if (ProneSettings.UseStamina)
        {
            if (m_currentProneStamina < ProneSettings.MinStaminaToProne)
            {
                return;
            }

            m_currentProneStamina -= ProneSettings.StaminaDecreasePerProne;
        }
    }

    private void CancelProne()
    {
        if(HasSomethingAboveHead)
        {
            return;
        }

        InProneState = false;
        WantToGetUp = false;
        animator.SetBool(AnimationHashUtility.Prone, false);
        FarFromGround = false;
        Controller.height = DefaultSettings.ControllerHeight;
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;
        FeetIK.IsEnabled = true;
    }
}
