using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    public bool CanSlide { get { return AnimatorVertical > 1f && !SlidingUnder && OnGround && !SomethingOnKneesHeight; } }
    public bool SlidingUnder { get { return animator.GetBool(AnimationHashUtility.PlayingSlideAnimation) && m_Sliding_Under; } }
    public bool SomethingOnKneesHeight { get { return Physics.Raycast(KneesHeightPosition, thisTransform.forward, 9); } }
    public Vector3 KneesHeightPosition { get { return thisTransform.position + Vector3.up; } }

    private Vector3 m_Start_Slide_Direction;
    private float m_currentSlideTime = 0;
    private bool m_Sliding_Under;

    float startY;
    public void StartSlide()
    {
        m_Start_Slide_Direction = Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f);
        Controller.height = SlideSettings.ControllerHeight;
        Controller.stepOffset = 0;
        Controller.center = SlideSettings.ControllerCenter;
        Controller.radius = SlideSettings.ControllerRadius;

        MouseLook.MaxY = 35;
        MouseLook.MinY = -35;
        MouseLook.ClampHorizontalRotation = true;
        //Debug.Break();

        m_stored_maxX = MouseLook.MaxX;


        animator.SetTrigger(AnimationHashUtility.Slide);
        m_Sliding_Under = true;

        if (DebugSettings.DebugSlide)
        {
            Debug.Log("Started Sliding");
        }
    }

    private void MoveSlide()
    {
        Controller.SimpleMove((Vector3.ClampMagnitude(ForwardMovement + RightMovement, 0.3f) * GetTargetSpeed) + (SlopeDirection * Speed.SlideSpeed) * Time.deltaTime);
        Controller.Move(SlopeSettings.SlidingForce * Time.deltaTime * Vector3.down);
    }

    private void HandleSlideUnderMovement()
    {
        if (!SlidingUnder)
        {
            return;
        }

        Controller.SimpleMove(m_Start_Slide_Direction * GetTargetSpeed);

        if (OnSlope)
        {
            Controller.Move(SlopeSettings.Force * Time.deltaTime * Vector3.down);
        }

        m_currentSlideTime += Time.deltaTime;

        if (m_currentSlideTime >= SlideSettings.AnimationLenght || !OnGround)
        {
            EndSlide();
        }
    }

    public void EndSlide()
    {
        MouseLook.ClampHorizontalRotation = false;
        m_currentSlideTime = 0;
        if (OnGround)
        {
            if (Input.GetKeyDown(crouchKey) || Input.GetKey(crouchKey) || Physics.Raycast(ColliderTop, Vector3.up, out _, 1.5f) || Physics.SphereCast(ColliderTop, Controller.radius, Vector3.up, out _, 1.5f))
            {
                // Slide to crouched
                animator.SetBool(AnimationHashUtility.Crouched, true);
                m_crouched = true;
                WantToGetUp = true;
                Controller.height = CrouchSettings.ControllerHeight;
                Controller.center = CrouchSettings.ControllerCenter;
                Controller.radius = DefaultSettings.ControllerRadius;
                Controller.stepOffset = CrouchSettings.ControllerStepOffset;
                m_Sliding_Under = false;
                SetAnimatorCrouched = true;
                animator.SetTrigger(AnimationHashUtility.EndSlide);

                if (DebugSettings.DebugSlide)
                {
                    Debug.Log("Ended Sliding to crouched.");
                }
            }
            else
            {
                //Slide to Stand up
                Controller.height = DefaultSettings.ControllerHeight;
                Controller.center = DefaultSettings.ControllerCenter;
                Controller.radius = DefaultSettings.ControllerRadius;
                Controller.stepOffset = DefaultSettings.ControllerStepOffset;
                animator.SetBool(AnimationHashUtility.Crouched, false);
                m_Sliding_Under = false;
                animator.SetTrigger(AnimationHashUtility.EndSlide);

                if (DebugSettings.DebugSlide)
                {
                    Debug.Log("Ended Sliding to stand.");
                }
            }
        }
        else
        {
            //Slide to Fall
            animator.SetBool(AnimationHashUtility.FarFromGround, true);
            animator.SetBool(AnimationHashUtility.OnGround, false);
            animator.SetBool(AnimationHashUtility.PlayerFalling, true);
            StartFalling();
        }
    }
}
