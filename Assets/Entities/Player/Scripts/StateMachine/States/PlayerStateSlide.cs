using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Tilemaps.Tile;

public class PlayerStateSlide : PlayerState
{
    private Vector3 m_Start_Slide_Direction;
    private float storedMouseLookMaxX;
    private float storedMouseLookMinX;
    private float m_currentSlideTime = 0;
    public PlayerStateSlide(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Sliding) { IsRootState = true; }
    public override void CheckSwitchStates()
    {
        throw new System.NotImplementedException();
    }

    public override void EnterState()
    {
        m_Start_Slide_Direction = Vector3.ClampMagnitude(Context.ForwardMovement + Context.RightMovement, 1.0f);
        Context.Controller.height = Context.SlideSettings.ControllerHeight;
        Context.Controller.stepOffset = 0;
        Context.Controller.center = Context.SlideSettings.ControllerCenter;
        Context.Controller.radius = Context.SlideSettings.ControllerRadius;
        m_currentSlideTime = 0;

        storedMouseLookMaxX = Context.MouseLook.MaxX;
        storedMouseLookMinX = Context.MouseLook.MinX;

        Context.MouseLook.MaxY = 35;
        Context.MouseLook.MinY = -35;
        Context.MouseLook.ClampHorizontalRotation = true;

        Context.Animator.Play("Slide_Down", 0);
        Context.SlidingUnder = true;
        Context.Animator.SetBool(AnimationHashUtility.Sliding,true);
        Context.Animator.SetBool(AnimationHashUtility.Stand,false);
    }


    public override void UpdateState()
    {
        Context.SetPlayerCameraPosition(Context.Speed.SlideUnderCameraSpeed);
        Context.Controller.SimpleMove(m_Start_Slide_Direction * Context.Speed.SlideUnderSpeed);

        if (Context.OnSlope)
        {
            Context.Controller.Move(Context.SlopeSettings.Force * Time.deltaTime * Vector3.down);
        }

        m_currentSlideTime += Time.deltaTime;

        if (m_currentSlideTime >= Context.SlideSettings.AnimationLenght || !Context.OnGround)
        {
            if (Context.OnGround)
            {
                if (Input.GetKeyDown(Context.crouchKey) || Input.GetKey(Context.crouchKey) || Physics.Raycast(Context.ColliderTop, Vector3.up, out _, 1.5f) || Physics.SphereCast(Context.ColliderTop, Context.Controller.radius, Vector3.up, out _, 1.5f))
                {
                    // Slide to crouched
                    Context.WantToGetUp = true;
                    SwitchState(Factory.Crouched());
                }
                else
                {
                    //Slide to Stand up
                    SwitchState(Factory.Grounded());
                }
            }
            else
            {
                //Slide to Fall
                SwitchState(Factory.Falling());
            }
        }
    }

    public override void ExitState()
    {
        Context.Animator.SetBool(AnimationHashUtility.Sliding, false);
        Context.MouseLook.ClampHorizontalRotation = false;
        Context.MouseLook.MaxY = storedMouseLookMaxX;
        Context.MouseLook.MinY = storedMouseLookMinX;
    }


    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }

    public override void OnCollisionEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnCollisionExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnCollisionStay()
    {
        throw new System.NotImplementedException();
    }

}
