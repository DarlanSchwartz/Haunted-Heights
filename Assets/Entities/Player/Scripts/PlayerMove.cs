using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
public partial class PlayerMove : MonoBehaviour
{
    public KeyCode resetDebugPositionKey = KeyCode.R;
    [SerializeField] private Transform resetPosition;
    private Vector3 restartPos;
    public bool isLocked = false;
    private Transform thisTransform;
    public Transform PlayerHead { get; private set; } // Is found on awake

    public Transform PlayerCamera { get; private set; } // Is set on awake
    // Canera FOV run Effect
    private Camera CameraComponent { get; set; }
    private float fovCounter = 0;
    public int defaultCameraFov = 60;
    public int runningCameraFov = 80;
    public float fovFadeSpeed = 5;

    public Transform SlideDirection { get; private set; } // Is created on awake
    public Animator animator { get; private set; } // Is found on awake

    public PlayerLook MouseLook; // Need to be set on inspector

    public bool MovingBackwards { get { return VerticalInput <= -0.1f; } }

    public HandIK HandIK { get; private set; } // Is found on awake
    public FeetIK FeetIK { get; private set; } // Is found on awake
    public CharacterController Controller { get; private set; } // Is found on awake
    [Space(10)]
    [Header("Keys")] //--------------------------------------------------KEYS----------------------------------------------------------------------
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.C;
    [Space(10)]
    public SpeedSettings Speed;
    public FallSettings FallSettings;
    public VaultSettings VaultSettings;
    public CrouchSettings CrouchSettings;
    public SlopeSettings SlopeSettings;
    public DebugSettings DebugSettings;
    public ClimbSettings ClimbSettings;
    public SlideSettings SlideSettings;
    public JumpSettings JumpSettings;
    public DefaultSettings DefaultSettings;

    #region PrivateFields
    // Grounded
    private RaycastHit m_aboveHeadHit;
    private Vector3 m_lastPlayerPos;
    private Vector3 m_thisFrameVelocityVector;
    private Vector2 m_playerVelocityVector;
    private float m_TargetSpeed;
    private Vector3 m_checkSpherePosition;
    private bool m_crouched;
    private float m_currentCrouchStamina = 1;

    // MovementAdvanced
    private Vector3 m_gravityForceV = Vector3.zero;
    private Vector3 m_movement;
    // Ground Check
    private readonly Vector3 m_groundCheckOffset = new Vector3(0, 0.1f, 0);
    private RaycastHit m_slopeHit;
    private RaycastHit m_groundHit;

    private bool PlayingHangStartAnimation
    {
        get
        {
            return animator.GetBool(AnimationHashUtility.PlayingHangStartAnimation);
        }
    }
    public bool m_inspecting { get; set; }

    #endregion

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        HandIK = GetComponent<HandIK>();
        FeetIK = GetComponent<FeetIK>();
        PlayerCamera = MouseLook.transform;
        thisTransform = transform;
        HandIK.Animator = animator;
        FeetIK.Animator = animator;

        GameObject tempSlideDir = new GameObject("Slide Direction Object");
        tempSlideDir.transform.SetParent(transform);
        SlideDirection = tempSlideDir.transform;

        // Find a active player on the childs and get its animator controller , avatar and head
        for (int i = 0; i < thisTransform.childCount; i++)
        {
            if (thisTransform.GetChild(i).gameObject.activeSelf && thisTransform.GetChild(i).GetComponent<Animator>())
            {
                animator.avatar = thisTransform.GetChild(i).GetComponent<Animator>().avatar;
                animator.runtimeAnimatorController = thisTransform.GetChild(i).GetComponent<Animator>().runtimeAnimatorController;
                Destroy(thisTransform.GetChild(i).GetComponent<Animator>());
                PlayerHead = thisTransform.GetChild(i).GetComponentInChildren<Head>().transform;
                break;
            }
        }

        MouseLook.Init(thisTransform, MouseLook.transform);
        m_chestYPosition = (Controller.height / 2) / 2;
        QualitySettings.vSyncCount = 0;
        restartPos = thisTransform.position;

        CameraComponent = MouseLook != null ? MouseLook.transform.GetComponent<Camera>() : null;
    }
    private void Update()
    {
        if (isLocked)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            thisTransform.position = restartPos;
            return;
        }

        if (m_inspecting)
        {
            return;
        }

        if (Input.GetKeyUp(resetDebugPositionKey) && resetPosition != null)
        {
            ResetPlayer(resetPosition.position, resetPosition.rotation);
            return;
        }

        if (InBalanceMode)
        {
            HandleBalance();
            return;
        }

        if (InLadder)
        {
            HandleLadderMovement();
            return;
        }


        if (!Vaulting && !Hanging && !SlidingUnder && !JumpingOnto && !GoingToHangTarget)
        {
            MoveSimple();
            HandleVaultRefactored();
            HandleJumpingOnto();
            HandleCrouch();
            HandleClimb();
            HandleSlideUnder();

            CheckFalling();

            if (!Jumping)
            {
                CheckLanded();
            }

            HandleJump();

            HandleAnimations();
        }

        HandleMouseLook();
        HandleCameraPosition();
        HandleSlideUnderMovement();
    }

    private void ResetPlayer(Vector3 position, Quaternion rotation)
    {
        MouseLook.Reset();
        thisTransform.SetPositionAndRotation(position, rotation);

    }
    private void HandleMouseLook()
    {
        if (HardLanding)
        {
            return;
        }

        if (Vaulting || Hanging || SlidingUnder || JumpingOnto)
        {
            if (!Climbing)
            {
                MouseLook.LookRotation(false);
            }
            else
            {
                MouseLook.CameraTargetRot.y = Mathf.Lerp(MouseLook.CameraTargetRot.y, 0, 5 * Time.deltaTime);
                MouseLook.ApplyCameraForcedLook();
            }
        }
        else
        {
            MouseLook.CameraTargetRot.y = Mathf.Lerp(MouseLook.CameraTargetRot.y, 0, 5 * Time.deltaTime);
            MouseLook.LookRotation(true);
        }
    }
    private void HandleBalance()
    {

        if (m_goinToBalanceStartPos)
        {
            return;
        }

        thisTransform.position = Vector3.MoveTowards(thisTransform.position, currentBalanceBeanTarget, (VerticalInput * GetTargetSpeed) * Time.deltaTime);
        animator.SetFloat(AnimationHashUtility.Vertical, VerticalInput);
        MouseLook.LookRotation(false);

        if (VerticalInput > 0)
        {
            motionTime += 0.5f * Time.deltaTime;
        }
        else if (VerticalInput < 0)
        {
            motionTime += 0.25f * Time.deltaTime;
        }

        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CrouchCameraSpeed * Time.deltaTime);

        animator.SetFloat(AnimationHashUtility.MotionTimeDelta, motionTime);
    }
    private void MoveSimple()
    {
        if (Climbing || HardLanding)
        {
            return;
        }
        Controller.SimpleMove(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * GetTargetSpeed);

        if ((VerticalInput != 0 || HorizontalInput != 0) && OnSlope)
        {
            Controller.Move(SlopeSettings.Force * Time.deltaTime * Vector3.down);
        }

        if (Falling && !Jumping)
        {
            m_gravityForceV += FallSettings.GravityForce * Time.deltaTime * Vector3.down;
            Controller.Move(m_gravityForceV * Time.deltaTime);
        }
    }
    public void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && CanJump)
        {
            Jump();
        }
    }
    private void HandleClimb()
    {
        if (CanClimb)
        {
            StartHang();
        }
    }
    private void HandleVaultRefactored()
    {
        if (Input.GetKeyDown(jumpKey) && CanVault)
        {
            VaultMethod();
        }
    }
    public void HandleJumpingOnto()
    {
        if (CanJumpOnto && Input.GetKeyDown(jumpKey))
        {
            StartJumpOnto();
        }
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
    private void HandleCrouch()
    {
        if (PlayingFallingAnimation || animator.GetBool(AnimationHashUtility.PlayingLandAnimation))
        {
            return;
        }

        switch (CrouchSettings.Mode)
        {
            case CrouchMode.Toggle:
                if (Input.GetKeyDown(crouchKey))
                {
                    if (DebugSettings.DebugCrouch)
                    {
                        Debug.Log("Crouch key pressed");
                    }

                    Crouched = !Crouched;
                }
                break;
            case CrouchMode.Hold:
                Crouched = Input.GetKey(crouchKey);
                break;
        }

        if (CrouchSettings.UseStamina && m_currentCrouchStamina < CrouchSettings.MaxStamina)
        {
            m_currentCrouchStamina += CrouchSettings.StaminaRegenerationRate * Time.deltaTime;
        }
    }
    private void HandleCameraPosition()
    {
        if (!Vaulting && !Hanging && !Climbing && !SlidingUnder && !JumpingOnto)
        {
            if (OnGround && !Landing && !Falling)
            {
                if (Crouched)
                {
                    PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CrouchCameraSpeed * Time.deltaTime);
                }
                else
                {
                    if (PlayerVelocity > 1)
                    {
                        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CameraSpeed * Time.deltaTime);
                    }
                    else
                    {
                        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.IdleCameraSpeed * Time.deltaTime);
                    }
                }
            }
            else if (Falling || Jumping)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.FallingCameraSpeed * Time.deltaTime);
            }
            else if (Landing)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.LandCameraSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (Vaulting || JumpingOnto)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.VaultCameraSpeed * Time.deltaTime);
            }
            else if (Climbing)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.ClimbCameraSpeed * Time.deltaTime);
            }
            else if (Hanging)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.HangCameraSpeed * Time.deltaTime);
            }
            else if (SlidingUnder)
            {
                PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.SlideUnderCameraSpeed * Time.deltaTime);
            }
        }
    }
    private void HandleAnimations()
    {
        SpeedFactor = PressingRunKey ? GetPlayerVelocity / Speed.RunSpeed : GetPlayerVelocity / Speed.WalkSpeed;
        TargetHorizontal = (HorizontalInput * RunFactor) * SpeedFactor;
        TargetVertical = (VerticalInput * RunFactor) * SpeedFactor;

        TargetVertical = Mathf.Clamp(TargetVertical, PressingRunKey ? -2 : -1, PressingRunKey ? 2 : 1);

        animator.SetFloat(AnimationHashUtility.Horizontal, Mathf.Lerp(AnimatorHorizontal, TargetHorizontal, Speed.AnimationSmooth * Time.deltaTime));
        animator.SetFloat(AnimationHashUtility.Vertical, Mathf.Lerp(AnimatorVertical, TargetVertical, Speed.AnimationSmooth * Time.deltaTime));

        animator.SetBool(AnimationHashUtility.OnGround, OnGround);


        if (AnimatorVertical > 0.1f || AnimatorVertical < -0.1f || AnimatorHorizontal > 0.1f || AnimatorHorizontal < -0.1f)
        {
            animator.SetBool(AnimationHashUtility.Idle, false);
        }
        else
        {
            animator.SetBool(AnimationHashUtility.Idle, true);
        }

        if (animator.GetFloat(AnimationHashUtility.Vertical) >= 1.9f)
        {
            fovCounter += Time.deltaTime;

            if (fovCounter >= 1)
            {
                CameraComponent.fieldOfView = Mathf.Lerp(CameraComponent.fieldOfView, runningCameraFov, fovFadeSpeed * Time.deltaTime);
            }
        }
        else
        {
            fovCounter = 0;

            if (CameraComponent.fieldOfView != defaultCameraFov)
            {
                CameraComponent.fieldOfView = Mathf.Lerp(CameraComponent.fieldOfView, defaultCameraFov, fovFadeSpeed * Time.deltaTime);
            }
        }
    }
    private void HandleSlideUnder()
    {
        if (Input.GetKeyDown(slideKey) && CanSlide)
        {
            StartSlide();
        }
    }



    // Properties

    #region Jumping

    public bool Jumping { get; private set; } = false;

    public bool CheckGround { get; private set; } = true;

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

    #endregion

    #region On Slopes

    public bool OnSlope
    {
        get
        {
            if (Jumping)
            {
                return false;
            }


            if (Physics.Raycast(thisTransform.position + (thisTransform.forward * 0.1f), Vector3.down, out m_slopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
            {
                if (m_slopeHit.normal != Vector3.up)
                {
                    return true;
                }
            }

            if (Physics.Raycast(thisTransform.position + (-thisTransform.forward * 0.1f), Vector3.down, out m_slopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
            {
                if (m_slopeHit.normal != Vector3.up)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public Vector3 SlopeDirection
    {
        get
        {
            SlideDirection.SetPositionAndRotation(m_slopeHit.point, Quaternion.FromToRotation(thisTransform.up, Vector3.Cross(Vector3.Cross(m_slopeHit.normal, Vector3.up), m_slopeHit.normal)));

            return -SlideDirection.up;
        }
    }

    public bool Sliding { get { return SlopeAngle > DefaultSettings.ControllerSlopeLimit; } }

    public float SlopeAngle
    {
        get
        {
            if (OnSlope)
            {
                return Vector3.Angle(m_slopeHit.normal, thisTransform.up);
            }
            else
            {
                return 0;
            }
        }
    }

    #endregion


    #region Defaults

    public float CurrentHeadHitAboveRayLenght
    {
        get
        {
            if (Jumping)
            {
                return JumpSettings.HeadHitAboveRayLenght;
            }

            if (Crouched)
            {
                return CrouchSettings.HeadHitAboveRayLenght;
            }

            if (SlidingUnder)
            {
                return SlideSettings.HeadHitAboveRayLenght;
            }

            return DefaultSettings.HeadHitAboveRayLenght;

        }
    }

    public bool GroundHit { get { return Physics.CheckSphere(CheckSpherePosition, Controller.radius, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore); } }


    public bool HasSomethingAboveHead
    {
        get
        {
            if (GoingToHangTarget || !Physics.SphereCast(ColliderTop, Controller.radius, Vector3.up, out m_aboveHeadHit, CurrentHeadHitAboveRayLenght))
            {//Prevent Collider Missing Null Reference
                return false;
            }

            if (Climbing)
            {
                return Physics.SphereCast(thisTransform.position + CrouchSettings.ControllerCenter, Controller.radius, Vector3.up, out m_aboveHeadHit, CrouchSettings.HeadHitAboveRayLenght);
            }

            return true;
        }
    }

    public bool OnGround { get { return CheckGround && GroundHit; } }

    public bool Landing { get { return animator.GetBool(AnimationHashUtility.PlayingLandAnimation); } }

    public bool HardLanding { get { return animator.GetBool(AnimationHashUtility.HardLanding); } }

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

    public float HorizontalInput { get { return Input.GetAxis("Horizontal"); } }
    public float VerticalInput { get { return Input.GetAxis("Vertical"); } }

    public float SpeedFactor { get; private set; } = 0;

    public float TargetHorizontal { get; private set; } = 0;

    public float TargetVertical { get; private set; } = 0;

    public float AnimatorVertical { get { return animator.GetFloat(AnimationHashUtility.Vertical); } }

    public float AnimatorHorizontal { get { return animator.GetFloat(AnimationHashUtility.Horizontal); } }

    public bool PressingRunKey { get { return Input.GetKey(runKey); } }

    public float RunFactor { get { return PressingRunKey ? 2 : 1; } }

    public float PlayerVelocity { get; private set; }

    public float GetPlayerVelocity
    {
        get
        {
            m_thisFrameVelocityVector = (thisTransform.position - m_lastPlayerPos) / Time.deltaTime;
            m_lastPlayerPos = thisTransform.position;

            m_playerVelocityVector.x = m_thisFrameVelocityVector.x;
            m_playerVelocityVector.y = m_thisFrameVelocityVector.z;

            PlayerVelocity = m_playerVelocityVector.magnitude;

            return m_playerVelocityVector.magnitude;
        }
    }

    public float GetTargetSpeed
    {
        get
        {
            if (InBalanceMode)
            {
                m_TargetSpeed = Mathf.Lerp(m_TargetSpeed, Speed.BalanceSpeed, Time.deltaTime * Speed.Acceleration);
                return m_TargetSpeed;
            }

            if (Crouched)
            {
                m_TargetSpeed = Mathf.Lerp(m_TargetSpeed, Speed.CrouchedSpeed, Time.deltaTime * Speed.Acceleration);
                return m_TargetSpeed;
            }

            if (SlidingUnder)
            {
                m_TargetSpeed = Speed.SlideUnderSpeed;
                return m_TargetSpeed;
            }

            if (VerticalInput == 0 && HorizontalInput == 0)
            {
                m_TargetSpeed = Mathf.Lerp(m_TargetSpeed, 0, Time.deltaTime * Speed.Acceleration);
                return m_TargetSpeed;
            }

            if (PressingRunKey)
            {
                m_TargetSpeed = Mathf.Lerp(m_TargetSpeed, Speed.RunSpeed, Time.deltaTime * Speed.Acceleration);
                return m_TargetSpeed;
            }

            /// Walk Speed
            m_TargetSpeed = Mathf.Lerp(m_TargetSpeed, Speed.WalkSpeed, Time.deltaTime * Speed.Acceleration);
            return m_TargetSpeed;
        }
    }

    public Vector3 RightMovement { get { return thisTransform.right * HorizontalInput; } }
    public Vector3 ForwardMovement { get { return thisTransform.forward * VerticalInput; } }
    public Vector3 PlayerMiddle { get { return thisTransform.position + new Vector3(0, Controller.height / 2, 0); } }
    public Vector3 ColliderBotton
    {
        get
        {//Prevent Collider Missing Null Reference
            if (GoingToHangTarget)
            {
                return thisTransform.position + Vector3.up * 2;
            }

            return thisTransform.position + new Vector3(Controller.center.z, Controller.center.y, Controller.center.x) - new Vector3(0, Controller.height / 2, 0);
        }
    }

    public Vector3 ColliderTop
    {
        get
        {
            if (GoingToHangTarget)
            {//Prevent Collider Missing Null Reference
                return thisTransform.position + Vector3.up * 2;
            }

            if (!Crouched)
            {
                return thisTransform.position + (Controller.center + new Vector3(0, Controller.height / 2, 0));
            }

            return thisTransform.position + Controller.center;
        }
    }

    #endregion

}