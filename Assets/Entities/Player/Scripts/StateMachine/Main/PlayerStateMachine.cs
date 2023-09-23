using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public bool isLocked = false;
    public Transform ThisTransform { get; private set; }

    PlayerState _currentState;
    PlayerStateFactory _states;

    public PlayerState CurrentState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }
    public bool CheckGround { get; set; } = true;
    public bool OnGround { get { return CheckGround && GroundHit; } }

    public RaycastHit CameraForwardHit;
    public Camera CameraComponent { get; private set; }
    private float fovCounter = 0;
    public int defaultCameraFov = 60;
    public int runningCameraFov = 80;
    public float fovFadeSpeed = 5;

    public Vector3 CheckSpherePosition { get { return ColliderBotton; } }
    public bool GroundHit { get { return Physics.CheckSphere(CheckSpherePosition, Controller.radius, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore); } }
    public Transform PlayerHead { get; private set; }
    public Transform PlayerCamera { get; private set; }
    public bool CanJump { get { return !Physics.SphereCast(ColliderTop, Controller.radius, Vector3.up, out _, JumpSettings.HeadHitAboveRayLenght); } }
    public Transform SlideDirection { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerLook MouseLook;
    public HandIK HandIK { get; private set; }
    public FeetIK FeetIK { get; private set; }
    public CharacterController Controller { get; private set; }
    [Space(10)]
    [Header("Keys")]
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.C;
    public KeyCode proneKey = KeyCode.Z;
    [Space(10)]
    public SpeedSettings Speed;
    public FallSettings FallSettings;
    public VaultSettings VaultSettings;
    public CrouchSettings CrouchSettings;
    public ProneSettings ProneSettings;
    public SlopeSettings SlopeSettings;
    public DebugSettings DebugSettings;
    public ClimbSettings ClimbSettings;
    public SlideSettings SlideSettings;
    public JumpSettings JumpSettings;
    public DefaultSettings DefaultSettings;
    public bool Hanging { get; set; }
   
    public bool OnSlope
    {
        get
        {
            if (Physics.Raycast(ThisTransform.position + (ThisTransform.forward * 0.1f), Vector3.down, out RaycastHit _fSlopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
            {
                if (_fSlopeHit.normal != Vector3.up)
                {
                    return true;
                }
            }

            if (Physics.Raycast(ThisTransform.position + (-ThisTransform.forward * 0.1f), Vector3.down, out RaycastHit _bSlopeHit, SlopeSettings.RayLenght, FallSettings.GroundLayers, QueryTriggerInteraction.Ignore) && GroundHit)
            {
                if (_bSlopeHit.normal != Vector3.up)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public bool Climbing { get; set; }
    public Transform LastHangObject { get; set; } 
    public bool HasSomethingHangableForward { get { return Physics.Raycast(PlayerCamera.position, PlayerCamera.forward, out CameraForwardHit, ClimbSettings.MaxDistance, ClimbSettings.Layers, QueryTriggerInteraction.Collide); } }
    public bool InHangableAngle { get { return (CurrentHangAngle >= ClimbSettings.MaxAngle && CurrentHangAngle <= 180); } }
    public float CurrentHangAngle { get { return Vector3.Angle(ThisTransform.forward, CameraForwardHit.normal); } }
    public bool HangableObjectForwardHasHangableTags { get { return ClimbSettings.HangableTags.Contains(CameraForwardHit.transform.tag); } }
    public bool CanStartToHang { get { return !MovingBackwards && HasSomethingHangableForward && HangableObjectForwardHasHangableTags && InHangableAngle && CameraForwardHit.transform != LastHangObject; } }
    public bool JumpOntoObjectForward { get { return Physics.Raycast(KneesHeightPosition, ThisTransform.forward, out _, VaultSettings.JumpOntoMaxDistance, VaultSettings.JumpableOntoLayers); } }
    public bool CanJumpOnto { get { return !JumpingOnto && JumpOntoObjectForward; } }
    public bool JumpingOnto { get; set; }
    public float CrouchStamina { get; set; }
    public float ProneStamina { get; set; }
    public bool CanProne { get { return ProneStamina >= ProneSettings.MinStaminaToProne; } }
    public bool IsCrouched { get { return Animator.GetBool(AnimationHashUtility.Crouched); } }
    public bool IsJumpPressed { get { return Input.GetKeyDown(jumpKey); } }
    public bool IsJumping { get; set; }
    public  bool IsProne { get; set; }
    public float HorizontalInput { get { return Input.GetAxis("Horizontal"); } }
    public float VerticalInput { get { return Input.GetAxis("Vertical"); } }
    public float AnimatorVertical { get { return Animator.GetFloat(AnimationHashUtility.Vertical); } }
    public float AnimatorHorizontal { get { return Animator.GetFloat(AnimationHashUtility.Horizontal); } }
    public bool MovingBackwards { get { return VerticalInput <= -0.1f; } }
    public bool IsPlayerMoving { get { return AnimatorVertical > 0.1f || AnimatorVertical < -0.1f || AnimatorHorizontal > 0.1f || AnimatorHorizontal < -0.1f; } }
    public float SpeedFactor { get; private set; }
    public float TargetHorizontal { get; private set; }
    public float TargetVertical { get; private set; }
    public bool SlidingUnder { get; set; }
    public bool WantToGetUp { get; set; }
    public bool CanSlide { get { return !SomethingOnKneesHeight; } }
    public bool SomethingOnKneesHeight { get { return Physics.Raycast(KneesHeightPosition, ThisTransform.forward, 9); } }
    public Vector3 KneesHeightPosition { get { return ThisTransform.position + Vector3.up; } }
    public bool PressingRunKey { get { return Input.GetKey(runKey); } }
    public bool IsMovementPressed { get { return HorizontalInput != 0 || VerticalInput != 0; } }
    public float RunFactor { get { return PressingRunKey ? 2 : 1; } }
    public float PlayerVelocity { get; private set; }
    public Vector3 RightMovement { get { return ThisTransform.right * HorizontalInput; } }
    public Vector3 ForwardMovement { get { return ThisTransform.forward * VerticalInput; } }
    public Vector3 PlayerMiddle { get { return ThisTransform.position + new Vector3(0, Controller.height / 2, 0); } }
    public Vector3 ColliderBotton { get { return ThisTransform.position + new Vector3(Controller.center.z, Controller.center.y, Controller.center.x) - new Vector3(0, Controller.height / 2, 0); } }
    public Vector3 ColliderTop { get { return ThisTransform.position + new Vector3(0, Controller.height, 0); } }
    public readonly Vector3 _fallCheckGroundStartOffset = new(0, 0.1f, 0);
    public Vector3 MiddleRayStart { get { return ThisTransform.position + _fallCheckGroundStartOffset; } }
    public Vector3 GravityForce { get; set; } = Vector3.zero;
    public bool Falling { get; set; }
    public bool PlayingFallingAnimation { get { return Animator.GetBool(AnimationHashUtility.Falling); } }
    public float LastGroundedPositionY { get; set; }
    public float TimeWaitingToFall { get; set; } = 0;
    public float CurrentVaultAngle { get { return Vector3.Angle(ThisTransform.forward, HitForwardVault.normal); } }
  
    public bool VaultableObjectForward
    {
        get
        {
            if (HitForwardVault.transform.CompareTag("Vaultable"))
            {
                _lastVaultableObj = HitForwardVault.transform.GetComponent<VaultableObject>();
            }

            return VaultSettings.VaultableTags.Contains(HitForwardVault.transform.tag);
        }
    }

    public bool HasGroundForVaultEnd(float vaultThickness)
    {
        return Physics.Raycast(PlayerMiddle + (ThisTransform.forward * (GetCurrentVaultCheckDistance + vaultThickness)), Vector3.down, out HitDownVault, 2.01f);
    }

    public Vector3 GetJumpOntoEndPosition()
    {
        Vector3 start = ThisTransform.position + (ThisTransform.forward * VaultSettings.JumpOntoMaxDistance) + new Vector3(0,4,0);
        Physics.Raycast(start, Vector3.down, out RaycastHit hitPoint, 10);
        return hitPoint.point;
    }

    public float GetCurrentVaultCheckDistance
    {
        get
        {
            return VerticalInput == 0 ? VaultSettings.MinVaultDistIdle : MovingBackwards ? 0 : PressingRunKey ? VaultSettings.MinVaultDistRunning : VaultSettings.MinVaultDistWalking;
        }
    }

    private readonly float _chestYPosition = 1;
    public RaycastHit HitForwardVault;
    public RaycastHit HitDownVault;
    private VaultableObject _lastVaultableObj;
    public bool CanVault { get { return ThereIsSomethingInFrontOfMeChin && VaultableObjectForward && !ThereIsSomethingInFrontOfMeChest && AnimatorVertical > -0.1f; } }
   
    public bool Vaulting { get; set; }

    public bool InVaultableAngle { get { return (CurrentVaultAngle >= VaultSettings.MaxAngle && CurrentVaultAngle <= 180); } }
    private Vector3 VaultRaycastMiddleStart { get { return ThisTransform.position + new Vector3(0, VaultSettings.MinVaultHeight, 0); } }
    private Vector3 JumpOntoRaycastStart { get { return ThisTransform.position + new Vector3(0, DefaultSettings.ControllerHeight / 4, 0); } }
    public bool ThereIsSomethingInFrontOfMeChin { get { return Physics.Raycast(VaultRaycastMiddleStart, ThisTransform.forward, out HitForwardVault, GetCurrentVaultCheckDistance, VaultSettings.Layers); } }
    public bool ThereIsSomethingInFrontOfMyKnees { get { return Physics.Raycast(JumpOntoRaycastStart, ThisTransform.forward, out _, GetCurrentVaultCheckDistance, VaultSettings.Layers); } }
    public bool ThereIsSomethingInFrontOfMeChest { get { return Physics.Raycast(PlayerMiddle + new Vector3(0, _chestYPosition, 0), ThisTransform.forward, out _, GetCurrentVaultCheckDistance * 1.4f); } }


    private Vector3 _lastFramePlayerPos;
    private Vector3 _thisFramePlayerVelocityVector;
    private Vector2 _playerVelocityVector2;

    [Header("State")]
    public PlayerStateType currentState = PlayerStateType.None;
    public PlayerStateType currentSubState = PlayerStateType.None;
    private void Awake()
    {
        _states = new PlayerStateFactory(this);
        Controller = GetComponent<CharacterController>();
        Animator = GetComponent<Animator>();
        HandIK = GetComponent<HandIK>();
        FeetIK = GetComponent<FeetIK>();
        PlayerCamera = MouseLook.transform;
        ThisTransform = transform;
        HandIK.Animator = Animator;
        FeetIK.Animator = Animator;
        _currentState = _states.Grounded();
        _currentState.EnterState();
        MouseLook.Init(ThisTransform, MouseLook.transform);
        CameraComponent = MouseLook != null ? MouseLook.transform.GetComponent<Camera>() : null;
        GravityForce = Vector3.zero;
        CrouchStamina = CrouchSettings.MaxStamina;
        ProneStamina = ProneSettings.MaxStamina;

        for (int i = 0; i < ThisTransform.childCount; i++)
        {
            if (ThisTransform.GetChild(i).gameObject.activeSelf && ThisTransform.GetChild(i).GetComponent<Animator>())
            {
                Animator.avatar = ThisTransform.GetChild(i).GetComponent<Animator>().avatar;
                Animator.runtimeAnimatorController = ThisTransform.GetChild(i).GetComponent<Animator>().runtimeAnimatorController;
                Destroy(ThisTransform.GetChild(i).GetComponent<Animator>());
                PlayerHead = ThisTransform.GetChild(i).GetComponentInChildren<Head>().transform;
                break;
            }
        }
    }

    private void Update()
    {
        if(CurrentState == null)
        {
            _states ??= new PlayerStateFactory(this);
            _currentState = _states.Grounded();
            _currentState.EnterState();
            return;
        }

        CurrentState.UpdateStates();
        currentState = CurrentState.StateIdentifier;
        currentSubState = CurrentState._currentSubState != null? CurrentState._currentSubState.StateIdentifier : PlayerStateType.None;
    }

    public void UpdateGroundedDirectionalAnimations(float target)
    {
        SpeedFactor = PressingRunKey ? GetPlayerVelocity / Speed.RunSpeed : GetPlayerVelocity / Speed.WalkSpeed;
        TargetHorizontal = (HorizontalInput * RunFactor) * SpeedFactor;
        TargetVertical = (VerticalInput * RunFactor) * SpeedFactor;
        Animator.SetFloat(AnimationHashUtility.Horizontal, Mathf.Lerp(AnimatorHorizontal, TargetHorizontal, Speed.AnimationSmooth * Time.deltaTime));
        Animator.SetFloat(AnimationHashUtility.Vertical, Mathf.Lerp(AnimatorVertical, TargetVertical, Speed.AnimationSmooth * Time.deltaTime));
        TargetVertical = Mathf.Clamp(TargetVertical, -target, target);
    }

    public void HandleFovChange()
    {
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

    public float GetPlayerVelocity
    {
        get
        {
            _thisFramePlayerVelocityVector = (ThisTransform.position - _lastFramePlayerPos) / Time.deltaTime;
            _lastFramePlayerPos = ThisTransform.position;

            _playerVelocityVector2.x = _thisFramePlayerVelocityVector.x;
            _playerVelocityVector2.y = _thisFramePlayerVelocityVector.z;

            PlayerVelocity = _playerVelocityVector2.magnitude;

            return _playerVelocityVector2.magnitude;
        }
    }

    public void MoveSimple(float speed)
    {
        Controller.SimpleMove(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * speed);

        if (IsMovementPressed && OnSlope)
        {
            Controller.Move(SlopeSettings.Force * Time.deltaTime * Vector3.down);
        }
    }

  
    public void HandleMouseLook(bool lockRotation, bool followHeadRotation)
    {
        if (lockRotation)
        {
            MouseLook.CameraTargetRot.y = Mathf.Lerp(MouseLook.CameraTargetRot.y, 0, 5 * Time.deltaTime);
            MouseLook.LookRotation(true);

            if (followHeadRotation)
            {
                MouseLook.transform.rotation = Quaternion.Slerp(MouseLook.transform.rotation, Quaternion.LookRotation(PlayerHead.transform.forward), Time.deltaTime * 500);
            }
        }
        else
        {
            MouseLook.CameraTargetRot.y = Mathf.Lerp(MouseLook.CameraTargetRot.y, 0, 5 * Time.deltaTime);
            MouseLook.ApplyCameraForcedLook();
        }
    }

    public void SetPlayerCameraPosition(float transitionSpeed)
    {
        PlayerCamera.position = Vector3.Lerp(PlayerCamera.position, PlayerHead.position, transitionSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawSphere(ColliderBotton, 0.5f);
            Gizmos.DrawWireSphere(ColliderTop + new Vector3(0,0.2f,0), 0.5f);
        }
    }
}