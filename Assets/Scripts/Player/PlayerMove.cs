using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    public KeyCode resetDebugPositionKey = KeyCode.R;
    [SerializeField] private Transform resetPosition;
    private Vector3 restartPos;
    public Transform PlayerHead { get; private set; } // Is found on awake

    public Transform PlayerCamera { get; private set; } // Is set on awake
    // Canera FOV run Effect
    private Camera CameraComponent { get; set; }
    private float fovCounter = 0;
    public int defaultCameraFov = 60;
    public int runningCameraFov = 80;
    public float fovFadeSpeed = 5;
   
    public Transform SlideDirection { get; private set; } // Is created on awake
    public Animator Animator { get; private set; } // Is found on awake

    public PlayerLook MouseLook; // Need to be set on inspector

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
    // Vaulting
    private float m_chestYPosition = 1;
    private Vector3 m_targetFinalVault = Vector3.zero;
    private RaycastHit m_hitForwardVault;
    private RaycastHit m_hitForwardJumpUp;
    private RaycastHit m_hitFowardVaultUp;
    private RaycastHit m_hitDownVault;
    private RaycastHit m_hitDownJumpUp;
    private RaycastHit m_hitFinalVaultTarget;
    private float m_currentVaultAngle;
    private Vector3 m_hasGroundForVaultRayStart;
    private Vector3 m_hasGroundForJumpUpEnd;
    private VaultableObject m_lastVaultableObj;
    // Grabbing
    public RaycastHit m_fwdHit;
    private Vector3 m_target_GrabPos = Vector3.zero;
    private Vector3 m_target_GrabRot;
    private Vector3 m_target_EndClimbPos = Vector3.zero;
    private Vector2 m_hangingInput;
    private GrabableObject m_current_GrabObject;
    private bool m_hangingon_DynamicObject = false;
    private float m_stored_maxX;
    private bool PlayingHangStartAnimation{ get
        {
            return Animator.GetBool(AnimationHashUtility.PlayingHangStartAnimation);
        }
    }
    //Sliding
    private Vector3 m_Start_Slide_Direction;
    private float m_currentSlideTime = 0;
    private bool m_Sliding_Under;
    //JumpingOnto
    private RaycastHit m_ForwardOntoHit;
    private Vector3 m_targetPosJumpOnto;
    // Inspecting
    public bool m_inspecting { get; set; }
    // Balance
    public bool InBalanceMode { get; private set; }
    private float motionTime = 0;
    private BalanceBeam currentBalanceBean;
    private Vector3 currentBalanceBeanTarget;
    private bool m_goinToBalanceStartPos = false;
    public bool inBetweenBalanceMode = false;
    private Transform m_currentTriggerStartTransform;

    //Ladder
    public bool InLadder { get; private set; }
    private Ladder currentLadder;
    private bool m_goingToLadderStartPos = false;
    private bool m_movingInLadder = false;
    private int currentLadderSegment = 0;
    private float defaultCameraMaxX = 75;
    private bool ladderClimbingEnd = false;

    #endregion

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Animator = GetComponent<Animator>();
        HandIK = GetComponent<HandIK>();
        FeetIK = GetComponent<FeetIK>();
        PlayerCamera = MouseLook.transform;

        HandIK.Animator = Animator;
        FeetIK.Animator = Animator;

        GameObject tempSlideDir = new GameObject("Slide Direction Object");
        tempSlideDir.transform.SetParent(transform);
        SlideDirection = tempSlideDir.transform;

        // Find a active player on the childs and get its animator controller , avatar and head
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf && transform.GetChild(i).GetComponent<Animator>())
            {
                Animator.avatar = transform.GetChild(i).GetComponent<Animator>().avatar;
                Animator.runtimeAnimatorController = transform.GetChild(i).GetComponent<Animator>().runtimeAnimatorController;
                Destroy(transform.GetChild(i).GetComponent<Animator>());
                PlayerHead = transform.GetChild(i).GetComponentInChildren<Head>().transform;
                break;
            }
        }

        MouseLook.Init(transform, MouseLook.transform);
        m_chestYPosition = (Controller.height / 2) / 2;
        QualitySettings.vSyncCount = 0;
        restartPos = transform.position;

        CameraComponent = MouseLook != null ? MouseLook.transform.GetComponent<Camera>() : null;
    }


    private void Update()
    {
        if (m_inspecting)
        {
            return;
        }

        if (Input.GetKeyUp(resetDebugPositionKey) && resetPosition != null)
        {
            ResetPlayer(resetPosition.position,resetPosition.rotation);
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

    private void ResetPlayer(Vector3 position,Quaternion rotation)
    {
        MouseLook.Reset();
        transform.SetPositionAndRotation(position, rotation);

    }

    public void EnterLadder(Ladder ladder)
    {
        currentLadder = ladder;
        InLadder = true;
        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 83;
        MouseLook.MinY = -83;
        defaultCameraMaxX = MouseLook.MaxX;
        MouseLook.MaxX = 33;

        HandIK.ManualWeights = true;
        HandIK.LeftWeight = 1;
        HandIK.RightWeight = 1;
        HandIK.SetTargetsParent(currentLadder.transform);
        HandIK.SetTargetPositions(currentLadder.LeftHandIK.position, currentLadder.RightHandIK.position, false);
        HandIK.SetTargetRotations(currentLadder.LeftHandIK.rotation, currentLadder.RightHandIK.rotation);
        HandIK.StartFollowingTargets(true, true);

        FeetIK.IsEnabled = false;
        FeetIK.forceFeetIk = true;

        FeetIK.SetIKHints(true, transform.position + ((-transform.right * 0.8f) + transform.forward * 2), transform.position + ((transform.right * 0.8f) + transform.forward * 2));


        Controller.detectCollisions = false;

        Animator.SetBool(AnimationHashUtility.ClimbLadderIdle, true);

        currentLadderSegment = 0;
        currentLadder.UpdateDelta = true;
        m_goingToLadderStartPos = true;
        StopAllCoroutines();
        StartCoroutine(GoToLadderPoint(ladder.startPoint.transform));
    }

    private IEnumerator GoToLadderPoint(Transform TargetReference)
    {
        bool conditionMet;
        do
        {
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, TargetReference.position,10 * Time.deltaTime), Quaternion.Lerp(transform.rotation, TargetReference.rotation, 10 * Time.deltaTime));
            m_goingToLadderStartPos = true;
            float difference = Quaternion.Angle(transform.rotation, TargetReference.rotation);
            conditionMet = Vector3.Distance(transform.position, TargetReference.position) <= 0.9f && difference < 0.2f;
            yield return null;
        } while (!conditionMet);

        m_goingToLadderStartPos = false;
        transform.SetPositionAndRotation(TargetReference.position, TargetReference.rotation);

        yield break;
    }

    private IEnumerator ClimbLadderPoint(Transform TargetReference)
    {
        Animator.SetTrigger(AnimationHashUtility.Climb);
        Animator.SetBool(AnimationHashUtility.ClimbLadderIdle,false);
        ladderClimbingEnd = true;
        currentLadder.ResetDelta(false);
        float delta = 0;
        float ikWeight = 1;
        UpdateIKPositionsInLadder(1);
        currentLadder.Climb();
        do
        {
            transform.SetPositionAndRotation(currentLadder.playerReferencePosition.position, currentLadder.playerReferencePosition.rotation);
            delta += 0.5f * Time.deltaTime;

            if(delta >= 0.5f)
            {
                ikWeight -= 2 * Time.deltaTime;
            }

            UpdateIKPositionsInLadder(ikWeight);
            if (delta >=1)
            {
                delta = 1;
            }

            currentLadder.SetTargetDelta(delta);

            yield return null;
        } while (delta != 1 || transform.rotation != TargetReference.rotation);

        yield return new WaitForEndOfFrame();

        currentLadder.UpdateDelta = false;
        currentLadder.ResetDelta(true);
        Animator.ResetTrigger(AnimationHashUtility.Climb);
        transform.position = TargetReference.position;
        ClearLadderConstraints();
        ladderClimbingEnd = false;

        yield break;
    }

    private void HandleLadderMovement()
    {
        
        MouseLook.LookRotation(false);
        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, Speed.CrouchCameraSpeed * Time.deltaTime);

        if(!ladderClimbingEnd)
        {
            UpdateIKPositionsInLadder(1);
        }

        if (m_goingToLadderStartPos || ladderClimbingEnd) 
        {
            return;
        }

        if(!m_movingInLadder)
        {
            if(Input.GetKey(KeyCode.W))
            {
                GoToLadderSegment(currentLadderSegment + 1);
                return;
            }
            else if(Input.GetKey(KeyCode.S))
            {
                GoToLadderSegment(currentLadderSegment - 1);
                return;
            }
        }
    }

    private void GoToLadderSegment(int thisLadderSegmentIndex)
    {
        if (thisLadderSegmentIndex < 0)
        {
            Exitladder(true);
            return;
        }
        
        if(thisLadderSegmentIndex >= currentLadder.points.Count)
        {
            Exitladder(false);
            return;
        }

       if(m_movingInLadder)
        {
            return;
        }

        currentLadderSegment = thisLadderSegmentIndex;
        currentLadder.SetTargetDelta(currentLadder.points[thisLadderSegmentIndex].deltaTarget);
        StartCoroutine(MoveToLadderSegment(currentLadder.points[thisLadderSegmentIndex].transform));
    }

    private void UpdateIKPositionsInLadder(float weights)
    {
        HandIK.LeftWeight = weights;
        HandIK.RightWeight = weights;
        HandIK.SetTargetPositions(currentLadder.LeftHandIK.position, currentLadder.RightHandIK.position, false);
        HandIK.SetTargetRotations(currentLadder.LeftHandIK.rotation, currentLadder.RightHandIK.rotation);

        FeetIK.leftFootIkPosition = currentLadder.LeftFootIK.position;
        FeetIK.rightFootIkPosition = currentLadder.RightFootIK.position;
        FeetIK.rightFootIkRotation = currentLadder.RightFootIK.rotation;
        FeetIK.leftFootIkRotation = currentLadder.LeftFootIK.rotation;

        FeetIK.SetIKHints(true, transform.position + ((-transform.right * 0.8f) + transform.forward * 2), transform.position + ((transform.right * 0.8f) + transform.forward * 2));
    }

    private IEnumerator MoveToLadderSegment(Transform targetRef)
    {
        do
        {
            transform.position = Vector3.LerpUnclamped(transform.position, targetRef.position, Speed.LadderSpeed* Time.deltaTime);
            m_movingInLadder = true;
            yield return null;
        } while (Vector3.Distance(transform.position, targetRef.position) > 0.01f);

        transform.position = targetRef.position;
        m_movingInLadder = false;

        yield break;
    }

    public void Exitladder(bool clearIkAndConstraints)
    {
       if(ladderClimbingEnd)
        {
            return;
        }

        if(clearIkAndConstraints)
        {
            Animator.SetTrigger(AnimationHashUtility.CancelHang);
            HandIK.DisableHandIK();
            ClearLadderConstraints();
            return;
        }
       else
        {
            StartCoroutine(ClimbLadderPoint(currentLadder.endClimbReference));
        }
    }

    private void ClearLadderConstraints()
    {
        HandIK.DisableHandIK();
        currentLadder = null;
        MouseLook.ClampHorizontalRotation = false;
        FeetIK.forceFeetIk = false;
        InLadder = false;
        FeetIK.IsEnabled = true;
        Animator.SetBool(AnimationHashUtility.ClimbLadderIdle, false);
        MouseLook.MaxX = defaultCameraMaxX;
        Controller.detectCollisions = true;
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
   
    // Methods

    #region Balance

    public void HandleEnterBalanceBeam(BalanceBeam balanceBeam, Transform EnterOrExitedTrigger, bool Entering)
    {
        if(Entering)
        {
            if (currentBalanceBean == null && !inBetweenBalanceMode)
            {
                currentBalanceBean = balanceBeam;
               if(balanceBeam.PointA == EnterOrExitedTrigger)
                {
                    EnterBalanceMode(currentBalanceBean.PointB.position, EnterOrExitedTrigger);
                }
               else
                {
                    EnterBalanceMode(currentBalanceBean.PointA.position, EnterOrExitedTrigger);
                }
                return;
            }
            else if(currentBalanceBean == balanceBeam && inBetweenBalanceMode || currentBalanceBean != balanceBeam && inBetweenBalanceMode)
            {
                ExitBalanceMode();
                return;
            }
        }
        else
        {
            if (balanceBeam == currentBalanceBean && !inBetweenBalanceMode || balanceBeam != currentBalanceBean && !inBetweenBalanceMode)
            {
                ExitBalanceMode();
                return;
            }
        }
    }
    public void EnterBalanceMode(Vector3 target, Transform startTrigger)
    {
        m_currentTriggerStartTransform = startTrigger;
        currentBalanceBeanTarget = target;
        InBalanceMode = true;
        Animator.SetBool(AnimationHashUtility.Balance, InBalanceMode);
        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 70;
        MouseLook.MinY = -70;
        motionTime = Animator.GetFloat(AnimationHashUtility.LeftFootCurve) >= 0.4f ? 0.5f: 0;
        Animator.SetBool(AnimationHashUtility.Idle,false);
        Animator.SetTrigger(AnimationHashUtility.StartBalance);
        Animator.SetFloat(AnimationHashUtility.Vertical, 0.1f);
        Animator.SetLayerWeight(2,0.5f);
        StartCoroutine(GoToBalanceStart(m_currentTriggerStartTransform));
    }
    private IEnumerator GoToBalanceStart(Transform startPos)
    {
        do
        {
            transform.SetPositionAndRotation(Vector3.Slerp(transform.position, startPos.position, 50 * Time.deltaTime), Quaternion.Slerp(transform.rotation, startPos.rotation, 10 * Time.deltaTime));
            m_goinToBalanceStartPos = true;
            yield return null;
        } while (Vector3.Distance(transform.position, startPos.position) > 0.01f && transform.rotation != startPos.rotation) ;

        m_goinToBalanceStartPos = false;

        yield break;
    }
    private void HandleBalance()
    {
        
        if(m_goinToBalanceStartPos)
        {
            return;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, currentBalanceBeanTarget, (VerticalInput * GetTargetSpeed) * Time.deltaTime);
        Animator.SetFloat(AnimationHashUtility.Vertical, VerticalInput);
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

        Animator.SetFloat(AnimationHashUtility.MotionTimeDelta, motionTime);
    }
    public void ExitBalanceMode()
    {
        InBalanceMode = false;
        Animator.SetFloat(AnimationHashUtility.MotionTimeDelta,1);
        Animator.SetBool(AnimationHashUtility.Balance, InBalanceMode);
        MouseLook.ClampHorizontalRotation = false;
        inBetweenBalanceMode = false;
        currentBalanceBean = null;
        Animator.SetLayerWeight(2, 0);
        Debug.Log("ExitBalancemode");
    }

    #endregion

    #region Move

    private void MoveSimple()
    {
        if (Climbing || HardLanding)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = restartPos;
            return;
        }

        if (Sliding)
        {
            MoveSlide();
        }
        else
        {
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
    }

    private void MoveSlide()
    {
        Controller.SimpleMove((Vector3.ClampMagnitude(ForwardMovement + RightMovement, 0.3f) * GetTargetSpeed) + (SlopeDirection * Speed.SlideSpeed) * Time.deltaTime);
        Controller.Move(SlopeSettings.SlidingForce * Time.deltaTime * Vector3.down);
    }

    #endregion

    #region Jump

    public void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && CanJump)
        {
            Jump();
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

        Jumping = true;
        CheckGround = false;
        Animator.SetFloat(AnimationHashUtility.FallHeight, 0);
        Animator.SetBool(AnimationHashUtility.Jumping, true);
        Animator.SetTrigger(AnimationHashUtility.Jump);
        Animator.ResetTrigger(AnimationHashUtility.Land);
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

        float jumpTime = 0;
        float jumpForce;
        do
        {
            jumpForce = JumpSettings.FallOff.Evaluate(jumpTime);

            if (Hanging)
            {
                Jumping = false;

                Animator.SetBool(AnimationHashUtility.Jumping, false);

                if (DebugSettings.DebugJump)
                {
                    Debug.Log("Jump ended. Reason: Started Grabbing during jump.");
                }

                yield break;
            }

            Controller.Move(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * (GetTargetSpeed / FallSettings.AirResistance) * FallSettings.AirControlFactor * Time.deltaTime);

            Controller.Move(jumpForce * JumpSettings.Force * Time.deltaTime * Vector3.up);

            jumpTime += Time.deltaTime;

            if (jumpTime > 0.1f)
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

            Animator.SetBool(AnimationHashUtility.Jumping, false);

            CheckGround = true;

            if (DebugSettings.DebugJump)
            {
                string reason = HasSomethingAboveHead ? "Had something above head" : "Jumptime end: " + jumpTime;
                Debug.Log("Jump ended. Reason: " + reason);
            }
        }

        yield break;
    }

    #endregion

    #region Climb
    private void HandleClimb()
    {
        if (VerticalInput >= -0.1f)
        {// Cant be moving backwards to start climbing
            if (!Landing)
            {// Cant be landing or was hanging in this same object
                if (!OnGround || Jumping)
                {//Need to be looking at an object within the max grab distance range and be far from the ground
                    if (HasSomethingForward && FarFromGround)
                    {//The looking object need to have a grabable tag and the angle between the player and the object cant exceed the maxgrabangle
                        if (GrabableObjectForward && InHangableAngle && m_fwdHit.transform != LastHangObject)
                        {
                            StartHang();
                        }
                    }
                }
            }
        }
    }

    private void StartHang()
    {
        LastHangObject = m_fwdHit.transform;

        m_current_GrabObject = m_fwdHit.transform.GetComponent<GrabableObject>();

        m_stored_maxX = MouseLook.MaxX;
        
        MouseLook.ClampHorizontalRotation = true;
        MouseLook.MaxY = 83;
        MouseLook.MinY = -83;

        HandIK.LeftWeight = 0;
        HandIK.RightWeight = 0;

        //MouseLook.CameraTargetRot.y = m_current_GrabObject.startTarget.eulerAngles.y;


        switch (m_current_GrabObject.Movement)
        {
            case BragableMovementType.Static:
                m_hangingon_DynamicObject = false;
                break;
            case BragableMovementType.Dynamic:
                transform.SetParent(m_current_GrabObject.transform.parent);
                m_hangingon_DynamicObject = true;
                break;
        }

        // Target to look at and go to
        m_target_GrabPos = m_current_GrabObject.startTarget.position;
        m_target_GrabRot = m_current_GrabObject.startTarget.eulerAngles;

        //Debug.Break();

        bool freeHang;
        switch (m_current_GrabObject.GrabType)
        {
            case GrabTypes.FreeHang:
                Animator.SetInteger(AnimationHashUtility.HangType, 1);
                HandIK.SetBodyLook(m_current_GrabObject.LookAtHanging.position, true, 1);
                HandIK.SetElbows(m_current_GrabObject.leftElbowTarget, m_current_GrabObject.rightElbowTarget, true);
                freeHang = true;
                break;
            case GrabTypes.BracedHang:
                Animator.SetInteger(AnimationHashUtility.HangType, 0);
                HandIK.SetBodyLook(m_current_GrabObject.startTarget.position + ClimbSettings.StartOffset, true, 1);
                freeHang = false;
                break;
            default:
                Animator.SetInteger(AnimationHashUtility.HangType, 0);
                freeHang = false;
                break;
        }


        m_target_EndClimbPos = m_current_GrabObject.endTarget.position;

        // Hand Animation
        Animator.SetLayerWeight(1, 1);

        HandIK.SetWeightSpeed(2);
        Hanging = true;
        CheckGround = false;
        Animator.SetBool(AnimationHashUtility.FarFromGround, false);
        Animator.SetBool(AnimationHashUtility.OnGround, false);
        Animator.SetBool(AnimationHashUtility.Hanging, true);

        PlaceHandOnHangTarget();

        AnimationBehaviorGrab behaviourGrab = null;

        if (freeHang)
        {
            foreach (AnimationBehaviorGrab currentAnimBehaviour in Animator.GetBehaviours<AnimationBehaviorGrab>())
            {
                if (currentAnimBehaviour.animationID == 1)
                {
                    behaviourGrab = currentAnimBehaviour;
                    break;
                }
            }
        }
        else
        {
            foreach (AnimationBehaviorGrab currentAnimBehaviour in Animator.GetBehaviours<AnimationBehaviorGrab>())
            {
                if (currentAnimBehaviour.animationID == 0)
                {
                    behaviourGrab = currentAnimBehaviour;
                    break;
                }
            }
        }

        StartCoroutine(Hang(behaviourGrab));
    }

    private IEnumerator Hang(AnimationBehaviorGrab grabAnim)
    {
        if (GoingToHangTarget)
        {
            yield break;
        }

        if (DebugSettings.DebugHang)
        {
            Debug.Log("Started Hanging.");
        }

        bool freeHang = grabAnim.animationID == 0 ? false : true;
        float maxX = freeHang ? ClimbSettings.MouseLookMaxXFreeHang : ClimbSettings.MouseLookMaxXGrab;
        GoingToHangTarget = true;
        Destroy(Controller);
        yield return new WaitForEndOfFrame();
        Controller = gameObject.AddComponent<CharacterController>();
        Controller.center = ClimbSettings.ControllerCenter;
        Controller.height = ClimbSettings.ControllerHeight;
        Controller.detectCollisions = false;
        Controller.minMoveDistance = 0;
        Controller.skinWidth = 0.001f;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;
        Controller.slopeLimit = DefaultSettings.ControllerSlopeLimit;
        Controller.SimpleMove(Vector3.zero);

        yield return null;

        // Go to climb point
        do
        {
            transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, m_target_GrabPos, Speed.HangSpeed * Time.deltaTime), Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_target_GrabRot), Speed.HangSpeed * 5 * Time.deltaTime));
            HandIK.LeftWeight = Mathf.LerpUnclamped(HandIK.LeftWeight, 1, 5 * Time.deltaTime);
            HandIK.RightWeight = Mathf.LerpUnclamped(HandIK.RightWeight, 1, 5 * Time.deltaTime);

            if (freeHang)
            {
                if (MouseLook.MaxX > maxX)
                {
                    MouseLook.MaxX -= 5 * Time.deltaTime;
                }
                else
                {
                    MouseLook.MaxX = maxX;
                }

                MouseLook.MaxX = Mathf.Clamp(MouseLook.MaxX, 0, maxX);
            }

            yield return null;
        } while (!grabAnim.Complete && Vector3.Distance(transform.position, m_target_GrabPos) > 0.1f && MouseLook.MaxX < maxX);

        transform.position = m_current_GrabObject.startTarget.position;
        GoingToHangTarget = false;

        // Check inputs to cancel grab or climb up
        do
        {
            // Check inputs and look for neighboos
            if (m_current_GrabObject.Neighboors.Count > 0)
            {
                m_hangingInput.x = HorizontalInput;
                m_hangingInput.y = VerticalInput;
            }

            SetHandsIkOnClimbTarget();

            if (Input.GetKeyDown(crouchKey) || Input.GetKey(crouchKey) || Input.GetKeyUp(crouchKey) || VerticalInput < -0.1f)
            {
                CancelHang();
                yield break;
            }
            else if (m_target_EndClimbPos != Vector3.zero && (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(KeyCode.W)) && !GoingToHangTarget && !PlayingHangStartAnimation)
            {
                StartClimb(freeHang);
                yield break;
            }

            yield return null;

        } while (true);
    }

    private void SetHandsIkOnClimbTarget()
    {
        if (ClosestNeighboor(m_hangingInput, m_current_GrabObject) != null)
        {
            if (m_hangingInput.x > 0)
            {
                HandIK.SetRightTargetPosition(ClosestNeighboor(m_hangingInput, m_current_GrabObject).rightHandIKTarget.position, true);
                HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
            }
            else if (m_hangingInput.x < 0)
            {
                HandIK.SetLeftTargetPosition(ClosestNeighboor(m_hangingInput, m_current_GrabObject).leftHandIKTarget.position, true);
                HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
            }
            else
            {
                HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
                HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
            }
        }
        else
        {
            HandIK.SetLeftTargetPosition(m_current_GrabObject.leftHandIKTarget.position, true);
            HandIK.SetRightTargetPosition(m_current_GrabObject.rightHandIKTarget.position, true);
        }
    }
    private void StartClimb(bool isFreehangObject)
    {
        if(isFreehangObject)
        {
            SetHandsIkOnClimbTarget();
        }

        HandIK.SetElbows(null, null, false);
        Animator.SetTrigger(AnimationHashUtility.Climb);
        Animator.SetBool(AnimationHashUtility.PlayingClimbAnimation, true);
        Animator.SetBool(AnimationHashUtility.PlayerFalling, false);
        Animator.SetBool(AnimationHashUtility.OnGround, true);
        Animator.SetFloat(AnimationHashUtility.Vertical, 0);
        Animator.SetFloat(AnimationHashUtility.Horizontal, 0);

        Animator.SetBool(AnimationHashUtility.Hanging, false);
        HandIK.ResetBodyLook();
        Climbing = true;
        StartCoroutine(GoToClimbEndTarget(isFreehangObject));
    }
    private IEnumerator GoToClimbEndTarget(bool fh)
    {
        float speed = fh ? Speed.ClimbFreeSpeed : Speed.ClimbSpeed;

        do
        {// TODO : Climb animation behavior to sync animation with this movement
            transform.position = Vector3.Slerp(transform.position, m_target_EndClimbPos, speed * Time.deltaTime);
            if (MouseLook.MaxX - 5 * Time.deltaTime >= ClimbSettings.MouseLookMaxXGrab)
            {
                MouseLook.MaxX -= 5 * Time.deltaTime;
            }

            if (MouseLook.MaxY > 85)
            {
                MouseLook.MaxX -= 5 * Time.deltaTime;
            }
            else if (MouseLook.MaxY  < -85)
            {
                MouseLook.MaxX += 5 * Time.deltaTime;
            }

            if (HasSomethingAboveHead)
            {
                Crouched = true;
            }

            MouseLook.LookRotation(false);
            yield return null;

        } while (Vector3.Distance(transform.position, m_target_EndClimbPos) > 0.01f && Animator.GetBool(AnimationHashUtility.PlayingClimbAnimation));

        EndClimb();

        yield break;
    }

    private void EndClimb()
    {
        if (GoingToHangTarget)
        {
            return;
        }

        if (m_hangingon_DynamicObject)
        {
            transform.SetParent(null);
            m_hangingon_DynamicObject = false;
        }

        m_gravityForceV.Set(0, 0, 0);
        HandIK.StopFollowingTargets();
        HandIK.ResetWeightSpeed();
        Animator.SetLayerWeight(1, 0);
        transform.rotation = m_current_GrabObject.endTarget.rotation;
        MouseLook.ClampHorizontalRotation = false;
        MouseLook.MaxX = m_stored_maxX;

        if(!Crouched)
        {
            Controller.enabled = true;
            Controller.center = DefaultSettings.ControllerCenter;
            Controller.height = DefaultSettings.ControllerHeight;
            Controller.detectCollisions = true;
        }

        transform.position = m_target_EndClimbPos;
       
        Climbing = false;
        Hanging = false;
        CheckGround = true;
        HandIK.DisableHandIK();
        m_current_GrabObject = null;
        LastHangObject = null;

        if (DebugSettings.DebugHang)
        {
            Debug.Log("End Climb");
        }
    }

    private void CancelHang()
    {
        m_gravityForceV.Set(0, 0, 0);
        HandIK.StopFollowingTargets();
        HandIK.ResetWeightSpeed();
        Animator.SetLayerWeight(1, 0);
        HandIK.SetElbows(null, null, false);

        MouseLook.CharacterTargetRot.Set(0, transform.rotation.eulerAngles.y, 0);
        MouseLook.ClampHorizontalRotation = false;
        MouseLook.MaxX = m_stored_maxX;
        Hanging = false;
        CheckGround = true;
        HandIK.DisableHandIK();
        Controller.detectCollisions = true;
        Animator.SetBool(AnimationHashUtility.Hanging, false);
        Animator.SetBool(AnimationHashUtility.FarFromGround, true);
        Animator.SetBool(AnimationHashUtility.OnGround, false);
        Animator.SetBool(AnimationHashUtility.PlayerFalling, true);
        Animator.SetTrigger(AnimationHashUtility.CancelHang);
        m_current_GrabObject = null;
        StartFalling();
        if (DebugSettings.DebugHang)
        {
            Debug.Log("Cancel Hang");
        }
    }

    private void PlaceHandOnHangTarget()
    {
        if (m_current_GrabObject.fixedPositions)
        {
            HandIK.SetTargetPositions(m_current_GrabObject.leftHandIKTarget.position, m_current_GrabObject.rightHandIKTarget.position, false);

            HandIK.SetTargetsParent(m_current_GrabObject.transform);

            HandIK.SetTargetRotations(m_current_GrabObject.leftHandIKTarget.rotation, m_current_GrabObject.rightHandIKTarget.rotation);

            HandIK.WeightSpeed = 1;

            HandIK.StartFollowingTargets(true, true);

        }
    }

    private GrabableObject ClosestNeighboor(Vector2 input, GrabableObject grabableObject)
    {
        foreach (GrabableObjectNeighBoor neighBoor in grabableObject.Neighboors)
        {
            if (neighBoor.Direction == input)
            {
                return neighBoor.grabableObject;
            }
        }

        return null;
    }

    #endregion

    #region Vault

    private void HandleVaultRefactored()
    {
        if(Input.GetKeyDown(jumpKey) && CanVault)
        {
            VaultMethod();
        }
    }
    private void VaultMethod()
    {
        AnimationBehaviorVault bv;

        if (AnimatorVertical > -0.1f)
        {
            // CHECK IF THERE IS SOME WALL OR THING OBSTRUCTING THE LEFT OR THE RIGHT OF THE PLAYER
            bool SomethingRight = Physics.Raycast(PlayerMiddle, transform.right, out _, 1.5f, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);
            bool SomethingLeft = Physics.Raycast(PlayerMiddle, -transform.right, out _, 1.5f, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);

            if (DebugSettings.DebugVault)
            {
                Debug.Log("Something Right" + SomethingRight);
                Debug.Log("Something Left" + SomethingLeft);
            }

            // Get the vault object info of the vaultable object that we found in front of us
            VaultableObject vaultObject = m_hitForwardVault.transform.GetComponent<VaultableObject>();

            // Set the correct animation based on the default direction of the vault and if there is something on the path of the default direction
            VaultDirection finalDirection = vaultObject.defaultDirection;
            switch (vaultObject.defaultDirection)
            {
                case VaultDirection.Forward:
                    Animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                    finalDirection = VaultDirection.Forward;
                    break;
                case VaultDirection.Left:
                    if (!SomethingRight)
                    {
                        Animator.SetBool(AnimationHashUtility.VaultRight, false);
                        finalDirection = VaultDirection.Left;
                    }
                    else if (SomethingRight)
                    {
                        Animator.SetBool(AnimationHashUtility.VaultRight, true);
                        finalDirection = VaultDirection.Right;
                    }
                    else if (SomethingRight && SomethingLeft)
                    {
                        Animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                        finalDirection = VaultDirection.Forward;
                    }
                    break;
                case VaultDirection.Right:
                    if (!SomethingLeft)
                    {
                        Animator.SetBool(AnimationHashUtility.VaultRight, true);
                        finalDirection = VaultDirection.Right;
                    }
                    else if (SomethingLeft)
                    {
                        Animator.SetBool(AnimationHashUtility.VaultRight, false);
                        finalDirection = VaultDirection.Left;
                    }
                    else if (SomethingRight && SomethingLeft)
                    {
                        Animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                        finalDirection = VaultDirection.Forward;
                    }
                    break;
                default:
                    Animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                    finalDirection = VaultDirection.Forward;
                    break;
            }

            VaultSpeed vaultMode = AnimatorVertical < 0.1f ? VaultSpeed.Idle : AnimatorVertical <= 1 && AnimatorVertical >= 0.1f ? VaultSpeed.Walking : AnimatorVertical > 1 ? VaultSpeed.Running : VaultSpeed.Idle;

            if (finalDirection == VaultDirection.Forward && AnimatorVertical >= 0.1f)
            {
                vaultMode = VaultSpeed.Monkey;
            }
            else
            {
                Animator.SetBool(AnimationHashUtility.MonkeyVault, false);
                finalDirection = vaultObject.defaultDirection;
                if (finalDirection == VaultDirection.Right)
                {
                    Animator.SetBool(AnimationHashUtility.VaultRight, true);
                    
                }
                else if(finalDirection == VaultDirection.Left)
                {
                    Animator.SetBool(AnimationHashUtility.VaultRight, false);
                }
                else
                {
                    Animator.SetBool(AnimationHashUtility.VaultRight, true);
                    finalDirection = VaultDirection.Right;
                }
            }

            VaultDirection vaultDirection = finalDirection;

            m_targetFinalVault = m_hitForwardVault.point + (transform.forward * ((1 * VaultSettings.SpeedMaxDistanceMultiplier) + vaultObject.Thickness));
            m_targetFinalVault.y = HasGroundForVaultEnd(vaultObject.Thickness) ? m_hitDownVault.point.y : transform.position.y;

            AnimationBehaviorVault[] vaultBehaviours = Animator.GetBehaviours<AnimationBehaviorVault>();


            foreach (AnimationBehaviorVault behavior in vaultBehaviours)
            {
                if (behavior.direction == vaultDirection && behavior.vaultMode == vaultMode)
                {
                    switch (vaultMode)
                    {
                        case VaultSpeed.Idle:
                            Animator.SetBool(AnimationHashUtility.VaultRun, false);
                            Animator.SetBool(AnimationHashUtility.Idle, true);
                            Animator.SetFloat(AnimationHashUtility.Vertical, 0);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Walking:
                            Animator.SetBool(AnimationHashUtility.VaultRun, false);
                            Animator.SetBool(AnimationHashUtility.Idle, false);
                            Animator.SetFloat(AnimationHashUtility.Vertical, 1);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Running:
                            Animator.SetBool(AnimationHashUtility.VaultRun, true);
                            Animator.SetBool(AnimationHashUtility.Idle, false);
                            Animator.SetFloat(AnimationHashUtility.Vertical, 1);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Monkey:
                            Animator.SetBool(AnimationHashUtility.VaultRun, false);
                            Animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                            Animator.SetBool(AnimationHashUtility.Idle, false);
                            Animator.SetFloat(AnimationHashUtility.Vertical, 1);
                            m_TargetSpeed = 0;
                            break;
                    }

                    bv = behavior;

                    behavior.Reset();
                    PlaceHandOnVaultTarget(vaultObject, vaultDirection);
                    StartCoroutine(VaultLerpDelta(behavior));
                    StartVault();


                    if (DebugSettings.DebugVault)
                    {
                        string debugResult = HasGroundForVaultEnd(vaultObject.Thickness) ? "Vault with land position" : "Vault without land position";
                        Debug.Log("Started vaulting to: " + vaultDirection.ToString() + " in a " + vaultMode.ToString() + " state.");
                        Debug.Log(debugResult);
                    }

                    break;
                }
            }
        }
    }

    private void StartVault()
    {
        Vaulting = true;
        Falling = false;
        MouseLook.UpdateRotation = false;
        Animator.SetBool(AnimationHashUtility.Vaulting, true);

        Controller.height = VaultSettings.ControllerHeight;
        Controller.center = VaultSettings.ControllerCenter;

        Animator.ResetTrigger(AnimationHashUtility.Land);
        Animator.SetTrigger(AnimationHashUtility.Vault);
    }

    private void PlaceHandOnVaultTarget(VaultableObject vaultableObject, VaultDirection directionVault)
    {
        switch (directionVault)
        {
            case VaultDirection.Left:

                RaycastHit hitL;
                bool foundHandPlacementLeft = Physics.Raycast(PlayerMiddle - transform.right, transform.forward, out hitL, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetHandPositionLeft = foundHandPlacementLeft ? hitL.point : m_hitForwardVault.point + -transform.right;

                // place hand on the correct vault height
                targetHandPositionLeft.y += vaultableObject.Height;

                //place hand on the left of the character

                HandIK.SetTargetPositions(targetHandPositionLeft, Vector3.zero, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(transform.rotation, Quaternion.identity);

                HandIK.WeightSpeed = 3;

                HandIK.StartFollowingTargets(true, false);

                break;
            case VaultDirection.Right:

                RaycastHit hitR;
                bool foundHandPlacementRight = Physics.Raycast(PlayerMiddle + transform.right, transform.forward, out hitR, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetHandPositionRight = foundHandPlacementRight ? hitR.point : m_hitForwardVault.point + transform.right;

                // place hand on the correct vault height
                targetHandPositionRight.y += vaultableObject.Height;

                //place hand on the right of the character

                HandIK.SetTargetPositions(Vector3.zero, targetHandPositionRight, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(Quaternion.identity, transform.rotation);

                HandIK.WeightSpeed = 3;

                HandIK.StartFollowingTargets(false, true);

                break;
            case VaultDirection.Forward:

                RaycastHit hitRF;
                RaycastHit hitLF;
                bool foundHandPlacementRightForward = Physics.Raycast(PlayerMiddle + transform.right, transform.forward, out hitRF, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                bool foundHandPlacementLeftForward = Physics.Raycast(PlayerMiddle - transform.right, transform.forward, out hitLF, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetForwardHandPositionRight = foundHandPlacementRightForward ? hitRF.point : m_hitForwardVault.point + transform.right;
                Vector3 targetForwardHandPositionLeft = foundHandPlacementLeftForward ? hitLF.point : m_hitForwardVault.point + -transform.right;

                targetForwardHandPositionRight.y += vaultableObject.Height;
                targetForwardHandPositionLeft.y += vaultableObject.Height;

                HandIK.SetTargetPositions(targetForwardHandPositionLeft, targetForwardHandPositionRight, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(transform.rotation, transform.rotation);
                HandIK.WeightSpeed = 3;
                HandIK.StartFollowingTargets(true, true);
                break;
        }

        if (DebugSettings.DebugVault)
        {
            Debug.Log("Placed the " + directionVault.ToString() + " hand on the vault object.");
        }
    }

    private IEnumerator VaultLerpDelta(AnimationBehaviorVault vaultAnim)
    {
        Vector3 startPos = transform.position;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("Started Vault Coroutine.");
        }

        float timePassed = 0;

        do
        {
            transform.position = Vector3.Lerp(startPos, m_targetFinalVault, vaultAnim.AnimationDelta);
            timePassed += Time.deltaTime;

            if(timePassed >= 0.5f)
            {
                HandIK.DisableHandIK();
            }

            yield return null;
        } while (!vaultAnim.Complete);
        EndVault();

        if (!OnGround)
        {
            //Force start Falling
            Animator.SetBool(AnimationHashUtility.FarFromGround, true);
            Animator.SetBool(AnimationHashUtility.OnGround, false);
            Animator.SetBool(AnimationHashUtility.PlayerFalling, true);
            StartFalling();
        }

        yield break;
    }

    private void EndVault()
    {
        Vaulting = false;
        HandIK.DisableHandIK();
        MouseLook.UpdateRotation = true;
        Animator.SetBool(AnimationHashUtility.Vaulting, false);
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.height = DefaultSettings.ControllerHeight;
    }

    #endregion

    #region JumpingOnto

    public bool JumpingOnto 
    {
        get
        {
            return Animator.GetBool(AnimationHashUtility.JumpingOnto);
        } 
    }
    private void StartJumpOnto()
    {
        if (JumpingOnto || Animator.GetBool(AnimationHashUtility.JumpingOnto))
        {
            return;
        }
        m_targetPosJumpOnto = GetJumpOntoEndPosition();
        Animator.SetTrigger(AnimationHashUtility.JumpOnto);
        StartCoroutine(JumpOntoLerpDelta(Animator.GetBehaviour<JumpingOntoBehavior>()));
    }

    private IEnumerator JumpOntoLerpDelta(JumpingOntoBehavior jumpingOntoAnim)
    {
        Vector3 startPos = transform.position;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("Started Jump onto Coroutine.");
        }

        do
        {
            transform.position = Vector3.SlerpUnclamped(startPos, m_targetPosJumpOnto, jumpingOntoAnim.AnimationDelta);
            PlayerCamera.position = PlayerHead.position;
            yield return null;
        } while (!jumpingOntoAnim.Complete);
        EndJumpOnto();
    }

    private void EndJumpOnto()
    {
        Falling = false;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("End Jump Onto");
        }
    }
  
    public bool JumpOntoObjectForward
    {
        get
        {
            return HasGroundForJumpOntoEnd() && Physics.Raycast(PlayerMiddle, transform.forward, out m_ForwardOntoHit, VaultSettings.JumpOntoMaxDistance, VaultSettings.Layers) && ThereIsSomethingInFrontOfMyKnees;
        }
    }
    public void HandleJumpingOnto()
    {
        if (!Vaulting && !Jumping && !Landing && OnGround && !PlayingFallingAnimation && !Falling && !Animator.GetBool(AnimationHashUtility.PlayingJumpAnimation) && !JumpingOnto)
        {
            if (JumpOntoObjectForward && InVaultableAngle)
            {
                CanJumpOnto = true;
                if (Input.GetKeyDown(jumpKey))
                {
                    StartJumpOnto();
                }

                return;
            }
            else
            {
                CanJumpOnto = false;
            }
        }
        else
        {
            CanJumpOnto = false;
        }
    }

    #endregion

    #region Land

    public void Land(bool triggerAnimation)
    {
        CheckGround = true;
        Jumping = false;
        Falling = false;
        Animator.SetBool(AnimationHashUtility.Jumping, false);
        Animator.SetBool(AnimationHashUtility.PlayerFalling, false);
        Animator.SetBool(AnimationHashUtility.FarFromGround, false);

        TimeWaitingToFall = 0;

        Controller.slopeLimit = DefaultSettings.ControllerSlopeLimit;
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.height = DefaultSettings.ControllerHeight;
        Controller.stepOffset = DefaultSettings.ControllerStepOffset;

        m_gravityForceV = Vector3.zero;

        if (triggerAnimation)
        {
            Animator.SetTrigger(AnimationHashUtility.Land);
        }

        if (DebugSettings.DebugLand)
        {
            Debug.Log("Landed");
        }

       // Debug.Log("Landed");

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

            LastGroundedPositionY = transform.position.y;
        }
        else
        {
            Animator.SetFloat(AnimationHashUtility.FallHeight, LastGroundedPositionY - transform.position.y);
        }
    }

    #endregion

    #region Fall

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
        else if(Crouched && FarFromGround && !OnGround)
        {
            StartFalling();
        }

        TimeWaitingToFall = 0;
    }

    public void StartFalling()
    {
        if(Animator.GetBool(AnimationHashUtility.PlayingClimbAnimation) || GoingToHangTarget)
        {
            return;
        }
        
        TimeWaitingToFall = 0;
        Falling = true;
        Animator.SetBool(AnimationHashUtility.FarFromGround, FarFromGround);
        Animator.SetBool(AnimationHashUtility.PlayerFalling, true);
        Animator.SetBool(AnimationHashUtility.PlayingLandAnimation, false);
        Controller.center = FallSettings.ControllerCenter;
        Controller.height = FallSettings.ControllerHeight;
        Controller.stepOffset = JumpSettings.ControllerStepOffset;
        Controller.radius = DefaultSettings.ControllerRadius;
        if (DebugSettings.DebugFalling)
        {
            Debug.Log("Started falling");
        }
    }

    #endregion

    #region Crouch

    private void HandleCrouch()
    {
        if(PlayingFallingAnimation || Animator.GetBool(AnimationHashUtility.PlayingLandAnimation))
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

    #endregion

    #region Animate

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

        Animator.SetFloat(AnimationHashUtility.Horizontal, Mathf.Lerp(AnimatorHorizontal, TargetHorizontal, Speed.AnimationSmooth * Time.deltaTime));
        Animator.SetFloat(AnimationHashUtility.Vertical, Mathf.Lerp(AnimatorVertical, TargetVertical, Speed.AnimationSmooth * Time.deltaTime));

        Animator.SetBool(AnimationHashUtility.OnGround, OnGround);


        if (AnimatorVertical > 0.1f || AnimatorVertical < -0.1f || AnimatorHorizontal > 0.1f || AnimatorHorizontal < -0.1f)
        {
            Animator.SetBool(AnimationHashUtility.Idle, false);
        }
        else
        {
            Animator.SetBool(AnimationHashUtility.Idle, true);
        }

        if (Animator.GetFloat(AnimationHashUtility.Vertical) >= 1.9f)
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


    #endregion

    #region Slide

    private void HandleSlideUnder()
    {
        if (Input.GetKeyDown(slideKey) && CanSlide)
        {
            StartSlide();
        }
    }
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


        Animator.SetTrigger(AnimationHashUtility.Slide);
        m_Sliding_Under = true;

        if (DebugSettings.DebugSlide)
        {
            Debug.Log("Started Sliding");
        }
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
                Animator.SetBool(AnimationHashUtility.Crouched, true);
                m_crouched = true;
                WantToGetUp = true;
                Controller.height = CrouchSettings.ControllerHeight;
                Controller.center = CrouchSettings.ControllerCenter;
                Controller.radius = DefaultSettings.ControllerRadius;
                Controller.stepOffset = CrouchSettings.ControllerStepOffset;
                m_Sliding_Under = false;
                SetAnimatorCrouched = true;
                Animator.SetTrigger(AnimationHashUtility.EndSlide);

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
                Animator.SetBool(AnimationHashUtility.Crouched, false);
                m_Sliding_Under = false;
                Animator.SetTrigger(AnimationHashUtility.EndSlide);

                if (DebugSettings.DebugSlide)
                {
                    Debug.Log("Ended Sliding to stand.");
                }
            }
        }
        else
        {
            //Slide to Fall
            Animator.SetBool(AnimationHashUtility.FarFromGround, true);
            Animator.SetBool(AnimationHashUtility.OnGround, false);
            Animator.SetBool(AnimationHashUtility.PlayerFalling, true);
            StartFalling();
        }
    }

    #endregion

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

    #region Slide Below

    public bool CanSlide { get { return AnimatorVertical > 1f && !SlidingUnder && OnGround && !SomethingOnKneesHeight; } }

    public bool SlidingUnder { get { return Animator.GetBool(AnimationHashUtility.PlayingSlideAnimation) && m_Sliding_Under; } }

    public bool SomethingOnKneesHeight { get { return Physics.Raycast(transform.position + Vector3.up, transform.forward, 9); } }
  

    #endregion

    #region Crouched

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
            return Animator.GetBool(AnimationHashUtility.Crouched);
        }
        set
        {
            Animator.SetBool(AnimationHashUtility.Crouched, value);
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


            if (Physics.Raycast(transform.position + (transform.forward * 0.1f), Vector3.down, out m_slopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
            {
                if (m_slopeHit.normal != Vector3.up)
                {
                    return true;
                }
            }

            if (Physics.Raycast(transform.position + (-transform.forward * 0.1f), Vector3.down, out m_slopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
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
            SlideDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(Vector3.Cross(m_slopeHit.normal, Vector3.up), m_slopeHit.normal));
            SlideDirection.position = m_slopeHit.point;

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
                return Vector3.Angle(m_slopeHit.normal, transform.up);
            }
            else
            {
                return 0;
            }
        }
    }

    #endregion

    #region Vaulting

    public bool CanVault 
    { 
        get
        {
            return 
                !JumpingOnto && 
                !Vaulting && 
                !Jumping && 
                !Landing && 
                OnGround && 
                !PlayingFallingAnimation && 
                !Falling && 
                !Animator.GetBool(AnimationHashUtility.PlayingJumpAnimation) &&
                ThereIsSomethingInFrontOfMeChin &&
                VaultableObjectForward &&
                !ThereIsSomethingInFrontOfMeChest;
        }
        private set { }
    } 
    public bool Vaulting { get; private set; } = false;
    public bool CanJumpOnto { get; private set; } = false;


    public float CurrentVaultAngle
    {
        get
        {
            m_currentVaultAngle = Vector3.Angle(transform.forward, m_hitForwardVault.normal);

            return m_currentVaultAngle;
        }
        private set
        {
            m_currentVaultAngle = value;
        }
    }

    public bool InVaultableAngle
    {
        get
        {
            return (CurrentVaultAngle >= VaultSettings.MaxAngle && CurrentVaultAngle <= 180);
        }
    }

    private Vector3 VaultRaycastMiddleStart
    {
        get
        {
            return transform.position + new Vector3(0, VaultSettings.MinVaultHeight, 0);
        }
    }

    private Vector3 JumpOntoRaycastStart
    {
        get
        {
            return transform.position + new Vector3(0, DefaultSettings.ControllerHeight / 4, 0);
        }
    }

    public bool ThereIsSomethingInFrontOfMeChin
    {
        get
        {
            return Physics.Raycast(VaultRaycastMiddleStart, transform.forward, out m_hitForwardVault, GetCurrentVaultCheckDistance, VaultSettings.Layers);
        }
    }

    public bool ThereIsSomethingInFrontOfMyKnees
    {
        get
        {
            return Physics.Raycast(JumpOntoRaycastStart, transform.forward, out m_hitForwardJumpUp, GetCurrentVaultCheckDistance, VaultSettings.Layers);
        }
    }

    public bool ThereIsSomethingInFrontOfMeChest
    {
        get
        {
            return Physics.Raycast(PlayerMiddle + new Vector3(0, m_chestYPosition, 0), transform.forward, out m_hitFowardVaultUp, GetCurrentVaultCheckDistance * 1.4f);
        }
    }

    public bool VaultableObjectForward
    {
        get
        {
            if (m_hitForwardVault.transform.CompareTag("Vaultable"))
            {
                m_lastVaultableObj = m_hitForwardVault.transform.GetComponent<VaultableObject>();
            }

            return VaultSettings.VaultableTags.Contains(m_hitForwardVault.transform.tag);
        }
    }

    public bool HasGroundForVaultEnd(float vaultThickness)
    {
        m_hasGroundForVaultRayStart = PlayerMiddle + (transform.forward * (GetCurrentVaultCheckDistance + vaultThickness));

        return Physics.Raycast(m_hasGroundForVaultRayStart, Vector3.down, out m_hitDownVault, 2.01f);
    }

    public bool HasGroundForJumpOntoEnd()
    {
        m_hasGroundForJumpUpEnd = (transform.position + new Vector3(0, DefaultSettings.ControllerHeight, 0) + (transform.forward * 1.3f));

        return Physics.Raycast(m_hasGroundForJumpUpEnd, Vector3.down, out m_hitDownJumpUp, 10);
    }

    public Vector3 GetJumpOntoEndPosition()
    {

        Vector3 start = transform.position + (transform.forward * 1.3f);
        start.y += 4;

        Physics.Raycast(start, Vector3.down, out RaycastHit hitPoint, 10);

        return hitPoint.point;
    }

    public float GetCurrentVaultCheckDistance
    {
        get
        {
            if (VerticalInput > 0.1f)
            {
                if (PressingRunKey)
                {
                    return VaultSettings.MinVaultDistRunning;
                }
                else
                {
                    return VaultSettings.MinVaultDistWalking;
                }
            }
            else if (VerticalInput == 0)
            {
                return VaultSettings.MinVaultDistIdle;
            }

            return 0;
        }
    }

    #endregion

    #region Climbing

    public bool CanHang { get; private set; } = false;
    public bool Hanging { get; private set; } = false;
    public bool Climbing { get; private set; } = false;
    public bool GoingToHangTarget { get; private set; } = false;
    public bool GrabableObjectForward { get { return ClimbSettings.HangableTags.Contains(m_fwdHit.transform.tag); } }
    public bool InHangableAngle { get { return (CurrentHangAngle >= ClimbSettings.MaxAngle && CurrentHangAngle <= 180); } }
    public float CurrentHangAngle
    {
        get
        {// Angle between my front and the looking point normal front
            return Vector3.Angle(transform.forward, m_fwdHit.normal);
        }
    }

    public Transform LastHangObject { get; private set; } = null;

    public bool HasSomethingForward { get { return Physics.Raycast(PlayerCamera.position, PlayerCamera.forward, out m_fwdHit, ClimbSettings.MaxDistance, ClimbSettings.Layers, QueryTriggerInteraction.Collide); } }
 

    public bool EnableHandsIK { get; set; }

    #endregion

    #region Falling

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
            return Animator.GetBool(AnimationHashUtility.PlayingFallingAnimation);
        }
        set
        {
            Animator.SetBool(AnimationHashUtility.PlayingFallingAnimation, value);
        }
    }

    public Vector3 MiddleRayStart { get { return transform.position + m_groundCheckOffset; } }

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

            if(Climbing)
            {
                return Physics.SphereCast(transform.position + CrouchSettings.ControllerCenter, Controller.radius, Vector3.up, out m_aboveHeadHit, CrouchSettings.HeadHitAboveRayLenght);
            }

            return true;
        }
    }

    public bool OnGround { get { return CheckGround && GroundHit; } }

    public bool Landing { get { return Animator.GetBool(AnimationHashUtility.PlayingLandAnimation); } }

    public bool HardLanding { get { return Animator.GetBool(AnimationHashUtility.HardLanding); } }

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
                m_checkSpherePosition = transform.position;
            }

            return m_checkSpherePosition;
        }
    }

    public float HorizontalInput { get { return Input.GetAxis("Horizontal"); } }
    public float VerticalInput { get { return Input.GetAxis("Vertical"); } }

    public float SpeedFactor { get; private set; } = 0;

    public float TargetHorizontal { get; private set; } = 0;

    public float TargetVertical { get; private set; } = 0;

    public float AnimatorVertical { get { return Animator.GetFloat(AnimationHashUtility.Vertical); } }

    public float AnimatorHorizontal { get { return Animator.GetFloat(AnimationHashUtility.Horizontal); } }

    public bool PressingRunKey { get { return Input.GetKey(runKey); } }

    public float RunFactor { get { return PressingRunKey ? 2 : 1; } }

    public float PlayerVelocity { get; private set; }

    public float GetPlayerVelocity
    {
        get
        {
            m_thisFrameVelocityVector = (transform.position - m_lastPlayerPos) / Time.deltaTime;
            m_lastPlayerPos = transform.position;

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
            if(InBalanceMode)
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

    public Vector3 RightMovement { get { return transform.right * HorizontalInput; } }
    public Vector3 ForwardMovement { get { return transform.forward * VerticalInput; } }
    public Vector3 PlayerMiddle{ get { return transform.position + new Vector3(0, Controller.height / 2, 0); } }
    public Vector3 ColliderBotton
    {
        get
        {//Prevent Collider Missing Null Reference
            if (GoingToHangTarget)
            {
                return transform.position + Vector3.up * 2;
            }

            return transform.position + new Vector3(Controller.center.z, Controller.center.y, Controller.center.x) - new Vector3(0, Controller.height / 2, 0);
        }
    }

    public Vector3 ColliderTop
    {
        get
        {
            if (GoingToHangTarget)
            {//Prevent Collider Missing Null Reference
                return transform.position + Vector3.up * 2;
            }

            if(!Crouched)
            {
                return transform.position + (Controller.center + new Vector3(0, Controller.height / 2, 0));
            }

            return transform.position + Controller.center;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        float spheresRadius = 0.03f;

        if (Controller == null) // was hanging
        {
            Controller = GetComponent<CharacterController>();
        }

        if (PlayerCamera == null)
        {
            PlayerCamera = MouseLook.transform;
        }

        if (GoingToHangTarget) // was hanging
        {
            return;
        }

        //Grounded indicator
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(CheckSpherePosition, Controller.radius);
        // Collider Tip indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(ColliderTop, spheresRadius);
        //Botton Tip
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ColliderBotton, spheresRadius);

        //Above Head Ray
        if (m_aboveHeadHit.point != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(ColliderTop, m_aboveHeadHit.point);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_aboveHeadHit.point, spheresRadius);
        }

        // Slide rays
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (transform.forward * 0.1f), spheresRadius);
        Gizmos.DrawWireSphere(transform.position + (-transform.forward * 0.1f), spheresRadius);

        //Climb Ray
        if (HasSomethingForward)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(PlayerCamera.position, m_fwdHit.point);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(m_fwdHit.point, spheresRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(PlayerCamera.position, PlayerCamera.position + (PlayerCamera.forward * ClimbSettings.MaxDistance));
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(PlayerCamera.position + (PlayerCamera.forward * ClimbSettings.MaxDistance), spheresRadius);
        }

        // Knee Height
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, spheresRadius);
        Gizmos.DrawWireSphere((transform.position + Vector3.up) + transform.forward * 7, spheresRadius);
        Gizmos.DrawLine(transform.position + Vector3.up, (transform.position + Vector3.up) + transform.forward * 7);
        //Vault forward indicator
        if (m_hitForwardVault.point != Vector3.zero)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(PlayerMiddle, m_hitForwardVault.point);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_hitForwardVault.point, spheresRadius);

            // Vault foward upside indicator
            if (m_hitFowardVaultUp.point == Vector3.zero)
            {
                Vector3 targetPos = PlayerMiddle + new Vector3(0, m_chestYPosition, 0) + (transform.forward * (GetCurrentVaultCheckDistance * 1.4f));

                Gizmos.color = Color.green;
                Gizmos.DrawLine(PlayerMiddle + new Vector3(0, m_chestYPosition, 0), targetPos);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPos, spheresRadius);

                // Vault down end indicator
                if (m_hitDownVault.point != Vector3.zero)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(PlayerMiddle + (transform.forward * (GetCurrentVaultCheckDistance + 0.4f)), m_hitDownVault.point);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(m_hitDownVault.point, spheresRadius);
                }
                else
                {
                    if (!HasGroundForVaultEnd(0.4f))
                    {
                        Gizmos.color = Color.green;
                        Vector3 posT = PlayerMiddle + (transform.forward * (GetCurrentVaultCheckDistance + 0.4f));
                        Gizmos.DrawLine(posT, posT + new Vector3(posT.x, transform.position.y, posT.z));
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(posT + new Vector3(posT.x, transform.position.y, posT.z), spheresRadius);
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                        Vector3 posT = m_hitForwardVault.point + (transform.forward * (1 + 0.4f));
                        Gizmos.DrawLine(posT, m_hitDownVault.point);
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(m_hitDownVault.point, spheresRadius);
                    }
                }
            }
        }
         if(InLadder)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + ((-transform.right * 0.8f) + transform.forward * 2), 0.2f);
            Gizmos.DrawWireSphere(transform.position + ((transform.right * 0.8f) + transform.forward * 2), 0.2f);
        }
    }


}