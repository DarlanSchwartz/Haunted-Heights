//using System;
//using System.Collections;
//using UnityEngine;

//public class BackupPlayerMove : MonoBehaviour
//{
//    public enum CrouchMode { Toggle, Hold }
//    public Animator playerAnimator;
//    public Transform playerHead;
//    public float animationTransitionSmoth;
//    public PlayerLook playerMouseLook;
//    public Transform playerCamera;
//    public CharacterController characterController;
//    public float radius = 0.5f;
//    public LayerMask groundLayers;
//    public Transform animationMover;
//    [Tooltip("This value will be set on the characterController.center when you stand up.")]
//    public Vector3 defaultControllerCenter;
//    [Tooltip("This value will be set on the character controller.height when you stand up.")]
//    public float defaultHeight;
//    [Tooltip("This value will be set on your character controller when you fall back on the ground after jumping or falling.")]
//    public float defaultStepOffset = 0.88f;
//    [Tooltip("This position will be set on the camera when changing stance from crouched to stand up.")]
//    public Vector3 defaultCameraPos;
//    [Space(10)]
//    [Header("Speed values")]
//    [Space(1)]
//    [Tooltip("This controls how much speed you have while pressing WDSA keys.")]
//    [Range(0, 100)]
//    public float walkSpeed;
//    [Tooltip("This controls how much speed you have while pressing run key and WDSA keys.")]
//    [Range(0, 100)]
//    public float runSpeed;
//    [Tooltip("This controls how much speed you have while crouched and pressing WDSA keys.")]
//    [Range(0, 100)]
//    public float crouchedSpeed;
//    [Tooltip("This controls how much speed you have while crouched and pressing run key.")]
//    [Range(0, 100)]
//    public float crouchedRunSpeed;
//    [Tooltip("This controls how fast you will speed up from idle to running.")]
//    public float acceleration;
//    [Space(10)]
//    [Header("Keys")]
//    [Space(1)]
//    public KeyCode runKey = KeyCode.LeftShift;
//    public KeyCode jumpKey = KeyCode.Space;
//    public KeyCode crouchKey = KeyCode.LeftControl;
//    [Space(10)]
//    [Header("Crouch")]
//    [Space(1)]
//    [Tooltip("This controls the crouch mode, toggle you have to press the crouch key everytime you want to crouch or get out of crouch state./nHold you have to hold the crouch key when you want to be crouched.")]
//    public CrouchMode crouchMode = CrouchMode.Toggle;
//    public CrouchStamina crouchStaminaConfig;
//    [Tooltip("This controls how fast the transition of the camera will occur from crouched to stand.")]
//    [Range(0.01f, 5)]
//    public float crouchSmooth;
//    [Tooltip("This controls the lenght off the ray that goes upside your head when you try to stand up.")]
//    public float crouchedRayLenght;
//    [Tooltip("This value will be set on the characterController.center when you crouch.")]
//    public Vector3 crouchedControllerCenter;
//    [Tooltip("This value will be set on the character controller.height when you crouch.")]
//    public float crouchedHeight;
//    [Tooltip("This will be the camera position when you crouch.")]
//    public Vector3 crouchedCameraPos;
//    [Space(10)]
//    [Header("Slopes")]
//    [Space(1)]
//    [Tooltip("This value controls how much downward force is applyed when you are moving in a slope.")]
//    public float slopeForce;
//    [Tooltip("This value controls how deep slopes can be to be considered walkable.")]
//    public float slopeForceRayLenght;
//    [Space(10)]
//    [Header("Jump")]
//    [Space(1)]
//    [Tooltip("This curve controls the movement you will do when you jump and dont hit anything.")]
//    public AnimationCurve jumpFallOff;
//    [Tooltip("This controls the jump force multiplier.")]
//    public float jumpMultiplier;
//    [Tooltip("This value controls where the tip of your head is. ( Value 1 means the tip of the character controller collider) \n\nThis value is used to prevent jumping being weird when you hit something above you.")]
//    public float headHitAboveRayLenght;
//    [Tooltip("This value controls how high you'll be able to climb up when you are jumping.\n Higher values means you can jump up very high places.")]
//    public float jumpingStepOffset = 0.3f;
    
//    [Space(10)]
//    [Header("Falling")]
//    [Space(1)]
//    [Tooltip("This value controls how far from ground you need to be to start the falling animation. \n\n Setting this to 0 means you will always start the falling animation no matter how hight you start to fall. \n\n This may cause unwanted animations when you fall from very low places, like steps from a staircase.")]
//    public float minimumFallDistance = 0.6f;
//    [Tooltip("This value controls how much time you have to be out of the ground to be considered falling.")]
//    [Range(0,1)]
//    public float minimumFallTime = 0.25f;
//    [Tooltip("This value will be set on the characterController.center when you are falling.")]
//    public Vector3 fallingControllerCenter;
//    [Tooltip("This value will be set on the character controller.height when you are falling.")]
//    public float fallingControllerHeight;
//    [Tooltip("This value how much gravity force is applyed on the character when you are falling.")]
//    public float gravityForce = 10;
//    public float groundRayDistance = 0.11f;
//    public float slopeGroundRayDistance = 0.2f;
//    [Space(10)]
//    [Header("Air control")]
//    [Space(1)]
//    [Tooltip("When this is true you can control the character while in the air.")]
//    public bool airControl = false;
//    [Tooltip("This controls how much you have control of the character in the air.")]
//    public float airControlFactor = 1;
//    [Tooltip("This controls how much you can move the character in the air.")]
//    public float airResistance = 100;
//    [Space(10)]
//    [Header("Vault")]
//    [Space(1)]
//    public float vaultThickness = 0.2f;
//    [Tooltip("This value sets the minimum angle that can be between you and your obstacle to you be able to vault over it.")]
//    [Range(90,179)]public float maxVaultAngle = 130;
//    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when running.")]
//    public float minVaultDistRunning = 5;
//    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when walking.")]
//    public float minVaultDistWalking = 3;
//    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when idle.")]
//    public float minVaultDistIdle = 2;
//    [Tooltip("This value controls the minimum height needed over the obstacle that you are looking for to you be able to vault over it.\n Setting this to 0 makes you able to vault up to something.")]
//    public float minVaultHeight = 1;
//    [Tooltip("This value controls wich obstacles are vautable by tag.")]
//    public string vaultableTags = "Vaultable,Etc";
//    [Tooltip("This value controls wich layers should be seen by the vault raycast.")]
//    public LayerMask vaultableLayers;
//    [Tooltip("This value will be set on the characterController.height when you start vaulting.")]
//    public Vector3 vaultingCenter;
//    [Tooltip("This value will be set on the character controller.height when you start vaulting.")]
//    public float vaultingControllerHeight = 1.42f;
//    [Space(10)]
//    [Header("Debug")]
//    public bool debugCanJump;
//    public bool debugLand;
//    public bool debugFalling;
//    public bool debugVault;
//    [Space(10)]
//    [Header("IK")]
//    public HandIK IKPlacement;
//    #region PrivateFields
//    // Grounded
//    private RaycastHit crouchedHit;
//    private Vector3 lastPlayerPos;
//    private Vector3 actualVelocity;
//    private Vector2 playerVelocityVector;
//    private Coroutine jumpEvent;
//    // MovementAdvanced
//    private Vector3 gravityForceV = Vector3.down;
//    private Vector3 movement;
//    // Ground Check
//    private readonly Vector3 GroundCheckOffset = new Vector3(0, 0.1f, 0);
//    private Vector3 hasGroundForVaultRayStart;
//    private RaycastHit groundHitFR;
//    private RaycastHit groundHitFL;
//    private RaycastHit groundHitBR;
//    private RaycastHit groundHitBL;
//    private RaycastHit middleHit;
//    private RaycastHit FarFromGroundHit;
//    // Vaulting
//    private float posH = 1;
//    private Vector3 targetFinalVault = Vector3.zero;
//    private RaycastHit hitForwardVault;
//    private RaycastHit hitFowardVaultUp;
//    private RaycastHit hitDownVault;
//    private RaycastHit hitFinalVaultTarget;
//    private float currentMinimumVaultDistance;
//    #endregion

//    private void Start()
//    {
//        //Cursor.lockState = CursorLockMode.Locked;
//        //Cursor.visible = false;
//        playerMouseLook.Init(transform, playerMouseLook.transform);
//        lastYGroundedPosition = transform.position.y;
//        posH = (characterController.height / 2) / 2;
//    }

//    private void FixedUpdate()
//    {
//        if(!Vaulting)
//        {
//            SetMovementSpeed();

//            MoveSimple();
//            //MoveAdvanced();

//            HandleVault();
//            HandleCameraPosition();
//            CheckFalling();
//            CheckLanded();
//            HandleJump();
//            HandleCrouch();
//            UpdatePlayerVelocity();
//            HandleAnimations();
//        }

//        playerMouseLook.LookRotation(false);
//    }

//    private void OnDrawGizmos()
//    {
//        float spheresRadius = 0.05f;

//        if (Crouched && !HasSomethingAboveHead)
//        {
//            if (crouchedHit.point != Vector3.zero)
//            {
//                Gizmos.color = Color.green;
//                Gizmos.DrawLine(AboveHeadRayStart, crouchedHit.point);
//                Gizmos.color = Color.red;
//                Gizmos.DrawSphere(crouchedHit.point, spheresRadius);
//            }
//        }


//        Gizmos.color = Color.blue;
//        Vector3 origin = MiddleRayStart;
//        Gizmos.color = new Color(165f, 255, 0);
//        Gizmos.DrawLine(origin, origin + (Vector3.down * minimumFallDistance));
//        Gizmos.color = new Color(255f, 0f, 255f);
//        Gizmos.DrawSphere(origin + (Vector3.down * minimumFallDistance), spheresRadius);
//    }

//    // ------------ VAULT --------------

//    private void HandleVault()
//    {
//        if (!Vaulting && !IsJumping && !playerAnimator.GetBool(AnimationHashUtility.PlayingLandAnimation) && IsOnGround)
//        {
//            HandleVaultMinimumDistance();

//            if (ThereIsSomethingInFrontOfMeChin)
//            {
//                Debug.DrawRay(PlayerMiddle, transform.forward * currentMinimumVaultDistance);

//                if (VaultableObjectForward)
//                {
//                    if (!ThereIsSomethingInFrontOfMeChest)
//                    {
//                        Debug.DrawRay(PlayerMiddle + new Vector3(0, posH, 0), transform.forward * (currentMinimumVaultDistance * 2));

//                        if (InVaultableAngle)
//                        {
//                            Debug.DrawRay(hasGroundForVaultRayStart, Vector3.down * minVaultHeight);

//                            CanVault = true;

//                            if (Input.GetKeyDown(jumpKey))
//                            {
//                                targetFinalVault = hitForwardVault.point + (transform.forward * (1 + vaultThickness));
//                                targetFinalVault.y = HasGroundForVaultEnd ? hitDownVault.point.y : transform.position.y;
                                
//                                if(debugVault)
//                                {
//                                    string debugResult = HasGroundForVaultEnd ? "Vault with land position" : "Vault without land position";
//                                    Debug.Log(debugResult);
//                                }

//                                VaultMethod();
//                            }

//                            return;

//                        }
//                    }
//                }
//            }
//        }

//        CanVault = false;
//    }

//    private void HandleVaultMinimumDistance()
//    {
//        if (VerticalInput > 0.1f)
//        {
//            if (Input.GetKey(runKey))
//            {
//                currentMinimumVaultDistance = minVaultDistRunning;
//            }
//            else
//            {
//                currentMinimumVaultDistance = minVaultDistWalking;
//            }
//        }
//        else if(VerticalInput == 0)
//        {
//            currentMinimumVaultDistance = minVaultDistIdle;
//        }
//    }

//    private void VaultMethod()
//    {
//        float vertical = playerAnimator.GetFloat(AnimationHashUtility.Vertical);

//        if (vertical > 1)
//        {
//            playerAnimator.SetBool(AnimationHashUtility.VaultRun, true);
//            //VaultAnimationRunning vaultAnimationRunning = playerAnimator.GetBehaviour<VaultAnimationRunning>();
//            //StartCoroutine(VaultLerpDeltaRunning(vaultAnimationRunning));
//            StartVault();
//            PlaceHandOnVaultTarget();

//            if (debugVault)
//            {
//                Debug.Log("Vault from run. Current Vertical: " + vertical);
//            }

//            return;
//        }
//        else if (vertical <= 1 && vertical > -0.001f)
//        {
//            playerAnimator.SetBool(AnimationHashUtility.VaultRun, false);
//            //VaultAnimationDefault vaultAnimationDefault = playerAnimator.GetBehaviour<VaultAnimationDefault>();
//            //StartCoroutine(VaultLerpDeltaDefault(vaultAnimationDefault));
//            StartVault();
//            PlaceHandOnVaultTarget();

//            if (debugVault)
//            {
//                Debug.Log("Vault from walking. Current Vertical: " + vertical);
//            }

//            return;
//        }
//        else if (vertical > -0.001f && vertical < 0.001f)
//        {
//            playerAnimator.SetBool(AnimationHashUtility.VaultRun, false);
//            //VaultAnimationDefault vaultAnimationDefault = playerAnimator.GetBehaviour<VaultAnimationDefault>();
//            //StartCoroutine(VaultLerpDeltaDefault(vaultAnimationDefault));
//            StartVault();
//            PlaceHandOnVaultTarget();

//            if (debugVault)
//            {
//                Debug.Log("Vault from idle. Current Vertical: " + vertical);
//            }

//            return;
//        }
//    }

//    private void StartVault()
//    {
//        CanVault = false;
//        Vaulting = true;
//        IsFalling = false;
//        characterController.height = vaultingControllerHeight;
//        characterController.center = vaultingCenter;
//        characterController.stepOffset = defaultStepOffset;
//        playerAnimator.ResetTrigger(AnimationHashUtility.Land);
//        playerAnimator.SetTrigger(AnimationHashUtility.Vault);
//        playerAnimator.SetBool(AnimationHashUtility.Vaulting, true);
        
//    }

//    private void PlaceHandOnVaultTarget()
//    {
//        VaultableObject vo = hitForwardVault.transform.GetComponent<VaultableObject>();

//        //place hand on the position of the raycast
//        Vector3 targetHandPosition = hitForwardVault.point;

//        // place hand on the correct vault height
//        targetHandPosition.y += vo.Thickness;

//        //place hand on the right of the character

//        targetHandPosition += -transform.right;

//        //IKPlacement.LeftHandIKTargetPosition = targetHandPosition;
//        //IKPlacement.LeftHandIKTargetRotation = transform.rotation;
//        IKPlacement.PlaceLeft = true;
//    }

//    private IEnumerator VaultLerpDeltaDefault(AnimationBehaviorVault vaultAnim)
//    {
//        Vector3 startPos = transform.position;

//        if (debugVault)
//        {
//            Debug.Log("Vault default coroutine call, start position: " + startPos);
//        }

//        do
//        {
//            transform.position = Vector3.Lerp(startPos, targetFinalVault, vaultAnim.AnimationDelta);
//            yield return null;
//        } while (!vaultAnim.Complete);
//        EndVault();

//        if (debugVault)
//        {
//            Debug.Log("Vault default coroutine end, current position: " + transform.position);
//        }

//        yield break;
//    }

//    private void EndVault()
//    {
//        Vaulting = false;
//        IKPlacement.PlaceLeft = false;
//        characterController.center = defaultControllerCenter;
//        characterController.height = defaultHeight;
//        playerAnimator.ResetTrigger(AnimationHashUtility.Vault);
//        playerAnimator.SetBool(AnimationHashUtility.Vaulting, false);
//        characterController.stepOffset = defaultStepOffset;
//    }

//    // ------------ ANIMATIONS --------------

//    private void UpdatePlayerVelocity()
//    {
//        actualVelocity = ((transform.position - lastPlayerPos) / Time.deltaTime);

//        lastPlayerPos = transform.position;

//        playerVelocityVector.x = actualVelocity.x;
//        playerVelocityVector.y = actualVelocity.z;

//        PlayerVelocity = playerVelocityVector.magnitude;
//    }

//    private void HandleCameraPosition()
//    {
//        if(IsOnGround && !IsLanding && !IsFalling)
//        {
//            if (Crouched)
//            {
//                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, crouchedCameraPos, crouchSmooth * Time.deltaTime);
//               // playerMouseLook.MaximumX = Mathf.Lerp(playerMouseLook.MaximumX, 70, 30 * Time.deltaTime);
//            }
//            else
//            {
//                //playerMouseLook.MaximumX = Mathf.Lerp(playerMouseLook.MaximumX, 85, 30 * Time.deltaTime);
//                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, defaultCameraPos, crouchSmooth * Time.deltaTime);
//            }
//        }
//        else if(IsFalling || IsJumping)
//        { 
//            //playerMouseLook.MaximumX = Mathf.Lerp(playerMouseLook.MaximumX, 70, 30 * Time.deltaTime);
//            playerCamera.position = Vector3.Lerp(playerCamera.position, playerHead.position, 10 * Time.deltaTime);
//        }
//        else if (IsLanding)
//        {
//            playerCamera.position = Vector3.Lerp(playerCamera.position, playerHead.position, 40 * Time.deltaTime);
//        }
//    }

//    private void HandleAnimations()
//    {
//        float runFactor = PressingRunKey ? 2 : 1;

//        float speedFactor = PressingRunKey ? PlayerVelocity / runSpeed : PlayerVelocity / walkSpeed;

//        float targetHorizontal = (HorizontalInput * runFactor) * speedFactor;
//        float targetVertical = (VerticalInput * runFactor) * speedFactor ;

//        targetVertical = Mathf.Clamp(targetVertical, PressingRunKey ? - 2 : -1, PressingRunKey ? 2 : 1);

//        playerAnimator.SetFloat(AnimationHashUtility.Horizontal, Mathf.Lerp(playerAnimator.GetFloat(AnimationHashUtility.Horizontal), targetHorizontal, animationTransitionSmoth * Time.deltaTime));
//        playerAnimator.SetFloat(AnimationHashUtility.Vertical, Mathf.Lerp(playerAnimator.GetFloat(AnimationHashUtility.Vertical), targetVertical, animationTransitionSmoth * Time.deltaTime));

//        playerAnimator.SetBool(AnimationHashUtility.Crouched, Crouched);
//        playerAnimator.SetBool(AnimationHashUtility.OnGround, IsOnGround);
        

//        if (playerAnimator.GetFloat(AnimationHashUtility.Vertical) > 0.1f || playerAnimator.GetFloat(AnimationHashUtility.Vertical) < -0.1f)
//        {
//            playerAnimator.SetBool(AnimationHashUtility.Idle, false);
//        }
//        else
//        {
//            playerAnimator.SetBool(AnimationHashUtility.Idle, true);
//        }
//    }

//    // ------------ CROUCH --------------

//    public void Crouch()
//    {
//        if (!Crouched)
//        {
//            if(crouchStaminaConfig.useCrouchStamina)
//            {
//                if(crouchStaminaConfig.currentStamina < crouchStaminaConfig.minimumStaminaToCrouch)
//                {
//                    return;
//                }
//                else
//                {
//                    crouchStaminaConfig.currentStamina -= crouchStaminaConfig.staminaDecreasePerCrouch;
//                }
//            }

//            Crouched = true;
//            WantToGetUp = false;
//            characterController.height = crouchedHeight;
//            characterController.center = crouchedControllerCenter;
//        }
//    }

//    public void CancelCrouch()
//    {
//        if (!HasSomethingAboveHead && Crouched)
//        {
//            Crouched = false;
//            WantToGetUp = false;
//            characterController.height = defaultHeight;
//            characterController.center = defaultControllerCenter;
//        }
//    }

//    private void HandleCrouch()
//    {
//        if (!IsJumping && !IsFalling && !playerAnimator.GetBool(AnimationHashUtility.PlayingLandAnimation) && !Vaulting)
//        {
//            switch (crouchMode)
//            {
//                case CrouchMode.Toggle:
//                    if (Input.GetKeyDown(crouchKey))
//                    {
//                        if (Crouched)
//                        {
//                            CancelCrouch();
//                        }
//                        else
//                        {
//                            Crouch();
//                        }
//                    }
//                    break;
//                case CrouchMode.Hold:
//                    if (Input.GetKey(crouchKey))
//                    {
//                        if(!Crouched)
//                        {
//                            Crouch();
//                        }

//                        WantToGetUp = false;
//                    }
//                    else 
//                    {
//                        if(Crouched && !HasSomethingAboveHead)
//                        {
//                            CancelCrouch();
//                        }
//                        else if(Crouched && HasSomethingAboveHead)
//                        {
//                            WantToGetUp = true;
//                        }
//                    }

//                    break;
//            }
//        }
//        else
//        {
//            CancelCrouch();
//        }

//        if(crouchStaminaConfig.useCrouchStamina && crouchStaminaConfig.currentStamina < crouchStaminaConfig.maxStamina)
//        {
//            crouchStaminaConfig.currentStamina += crouchStaminaConfig.staminaRegenerationRate * Time.deltaTime;
//        }
//    }

//    // --------- MOVEMENT ------------

//    private void MoveSimple()
//    {
//        characterController.SimpleMove(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * targetSpeed);

//        if ((VerticalInput != 0 || HorizontalInput != 0) && OnSlope)
//        {
//            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
//        }

//        if(IsFalling && !IsJumping)
//        {
//            characterController.Move(Vector3.down * characterController.height / 2 * gravityForce * Time.deltaTime);
//        }
//    }

//    private void MoveAdvanced()
//    {
//        if (!IsOnGround && !IsJumping)
//        {
//            gravityForceV += Vector3.down * gravityForce * Time.deltaTime;
//        }
//        else if(!IsOnGround && !IsJumping)
//        {
//            gravityForceV = Vector3.down * slopeForce * Time.deltaTime;
//        }
//        else
//        {
//            gravityForceV = Vector3.zero;
//        }

//        movement = Vector3.ClampMagnitude((ForwardMovement + RightMovement) * (targetSpeed * airControlFactor), targetSpeed) * Time.deltaTime;

//        movement += gravityForceV;

//        characterController.Move(movement);
//    }

//    public void SetMovementSpeed()
//    {
//        if (Input.GetKey(runKey))
//        {
//            if (!Crouched)
//            {
//                targetSpeed = Mathf.Lerp(targetSpeed, runSpeed, Time.deltaTime * acceleration);
//            }
//            else
//            {
//                targetSpeed = Mathf.Lerp(targetSpeed, crouchedRunSpeed, Time.deltaTime * acceleration);
//            }
//        }
//        else if ((VerticalInput != 0 || HorizontalInput != 0) && !Input.GetKey(runKey))
//        {
//            if (!Crouched)
//            {
//                targetSpeed = Mathf.Lerp(targetSpeed, walkSpeed, Time.deltaTime * acceleration);
//            }
//            else
//            {
//                targetSpeed = Mathf.Lerp(targetSpeed, crouchedSpeed, Time.deltaTime * acceleration);
//            }
//        }
//        else if (VerticalInput == 0 && HorizontalInput == 0)
//        {
//            targetSpeed = Mathf.Lerp(targetSpeed, 0, Time.deltaTime * acceleration);
//        }
//    }

//    // ------------ JUMP --------------

//    public void HandleJump()
//    {
//        if (Input.GetKeyDown(jumpKey))
//        {
//            if (!CanVault && CanJump)
//            {
//                Jump();
//            }
//        }
//    }

//    public void Jump()
//    {
//        if(Crouched)
//        {
//            CancelCrouch();
//            return;
//        }

//        IsJumping = true;
//        playerAnimator.SetFloat(AnimationHashUtility.FallHeight, 0);
//        playerAnimator.SetBool(AnimationHashUtility.Jumping, true);
//        playerAnimator.SetTrigger(AnimationHashUtility.Jump);
//        playerAnimator.ResetTrigger(AnimationHashUtility.Land);
//        jumpEvent = StartCoroutine(JumpEventSimple());
//    }

//    private IEnumerator JumpEventSimple()
//    {
//        characterController.slopeLimit = 90;
//        characterController.center = fallingControllerCenter;
//        characterController.height = fallingControllerHeight;
//        characterController.stepOffset = jumpingStepOffset;
//        float jumpTime = 0;
//        do
//        {
//            float jumpForce = jumpFallOff.Evaluate(jumpTime);

//            characterController.Move(Vector3.ClampMagnitude(ForwardMovement + RightMovement, 1.0f) * (targetSpeed / airResistance) * airControlFactor);

//            characterController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);

//            jumpTime += Time.deltaTime;

//            yield return null;
//        }
//        while (!IsOnGround && !HasSomethingAboveHead);

//        if (debugLand)
//        {
//            Debug.Log("Land call from jump");
//        }

//        if(IsOnGround)
//        {
//            Land();
//        }
//    }

//    private IEnumerator JumpEventAdvanced()
//    {
//        characterController.slopeLimit = 90;
//        characterController.center = fallingControllerCenter;
//        characterController.height = fallingControllerHeight;
//        characterController.stepOffset = jumpingStepOffset;

//        float jumpTime = 0;
//        do
//        {
//            float jumpForce = jumpFallOff.Evaluate(jumpTime);

//            characterController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);

//            jumpTime += Time.deltaTime;

//            yield return null;
//        }
//        while (!IsOnGround && !HasSomethingAboveHead && jumpTime < 0.5f);

//        if(IsOnGround)
//        {
//            Land();
//        }

//        IsJumping = false;
//    }
   
//    // ------------ LAND --------------

//    public void Land()
//    {
//        if(IsJumping)
//        {
//            StopCoroutine(jumpEvent);
//        }

//        playerAnimator.SetTrigger(AnimationHashUtility.Land);
//        characterController.slopeLimit = 45;
//        IsJumping = false;
//        playerAnimator.SetBool(AnimationHashUtility.Jumping, false);
//        playerAnimator.SetBool(AnimationHashUtility.PlayerFalling, false);
//        characterController.center = defaultControllerCenter;
//        characterController.height = defaultHeight;
//        IsFalling = false;
//        timeFalling = 0;
//        characterController.stepOffset = defaultStepOffset;

//        if (debugLand)
//        {
//            print("Landed");
//        }
//    }

//    private void CheckLanded()
//    {
//        if (IsOnGround && !Vaulting)
//        {
//            if (PlayingFallingAnimation && !IsLanding)
//            {
//                Land();
//            }

//            IsFalling = false;
//            characterController.stepOffset = defaultStepOffset;
//            lastYGroundedPosition = transform.position.y;
//        }
//        else
//        {
//            playerAnimator.SetFloat(AnimationHashUtility.FallHeight, lastYGroundedPosition - transform.position.y);
//        }
//    }

//    // ------------ FALL --------------

//    private void CheckFalling()
//    {
//        playerAnimator.SetBool(AnimationHashUtility.FarFromGround, FarFromGround);

//        if (!watingToStartFalling && !Vaulting && !Crouched && !IsJumping && !IsFalling && FarFromGround && !IsOnGround)
//        {
//            timeFalling += Time.deltaTime;

//            if(debugFalling)
//            {
//                Debug.Log("Waiting Time to Start Falling");
//            }

//            if (timeFalling >= minimumFallTime && StartFallingNextFrameCoroutine == null)
//            {
//                timeFalling = 0;
//                StartFallingNextFrameCoroutine = StartCoroutine(StartFallingNextFrame());
//            }

//            return;
//        }

//        timeFalling = 0;
//    }
   
//    public void StartFalling()
//    {
//        StartFallingNextFrameCoroutine = null;
//        playerAnimator.SetBool(AnimationHashUtility.PlayerFalling, true);
//        playerAnimator.SetBool(AnimationHashUtility.PlayingLandAnimation, false);
//        IsFalling = true;
//        characterController.center = fallingControllerCenter;
//        characterController.height = fallingControllerHeight;
//        characterController.stepOffset = jumpingStepOffset;
//        timeFalling = 0;
        
//        if(debugFalling)
//        {
//            Debug.Log("Started falling");
//        }
//    }

//    private IEnumerator StartFallingNextFrame()
//    {
//        watingToStartFalling = true;
//        yield return new WaitForEndOfFrame();
//        yield return new WaitForEndOfFrame();
//        yield return new WaitForEndOfFrame();
//        yield return new WaitForEndOfFrame();
//        watingToStartFalling = false;

//        if (!IsOnGround)
//        {
//            StartFalling();
//        }

//        yield break;
//    }
    
//    public bool HasSomethingAboveHead
//    {
//        get
//        {
//            if (!Physics.Raycast(AboveHeadRayStart, Vector3.up, out crouchedHit, crouchedRayLenght))
//            {
//                return false;
//            }

//            return true;
//        }
//    }

//    public bool OnSlope
//    {
//        get
//        {
//            if (IsJumping)
//            {
//                return false;
//            }

            
//            if (Physics.Raycast(PlayerMiddle, Vector3.down, out middleHit, characterController.height / 2 * slopeForceRayLenght))
//            {
//                if (middleHit.normal != Vector3.up)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }
//    }

//    public bool IsLanding
//    {
//        get
//        {
//            return playerAnimator.GetBool(AnimationHashUtility.PlayingLandAnimation);
//        }
//    }

//    public bool IsOnGround
//    {
//        get
//        {
//            if(GroundHit || characterController.isGrounded)
//            {
//                return true;
//            }

//            return false;
//        }
//    }

//    // Jumping

//    public bool IsJumping { get; private set; } = false;

//    public bool CanJump
//    {
//        get
//        {
//            if (IsJumping || Vaulting || IsFalling)
//            {
//                if (debugCanJump)
//                {
//                    string reason = "";

//                    reason += IsJumping ? " Jumping" : "";
//                    reason += Vaulting ? " Vaulting" : "";
//                    reason += Vaulting ? " IsFalling" : "";

//                    Debug.Log("Cannot jump because is" + reason);
//                }

//                return false;
//            }

//            if (HasSomethingAboveHead)
//            {
//                if (debugCanJump)
//                {
//                    Debug.Log("Cannot jump because has something above head");
//                }

//                return false;
//            }

//            if (playerAnimator.GetBool(AnimationHashUtility.PlayingFallingAnimation))
//            {
//                if (debugCanJump)
//                {
//                    Debug.Log("Cannot jump because is playing Falling Animation");
//                }

//                return false;
//            }

//            if (playerAnimator.GetBool(AnimationHashUtility.PlayingLandAnimation))
//            {
//                if (debugCanJump)
//                {
//                    Debug.Log("Cannot jump because is playing Landing Anim");
//                }

//                return false;
//            }

//            if (!GroundHit && !characterController.isGrounded && FarFromGround)
//            {
//                if (debugCanJump)
//                {
//                    Debug.Log("Cannot jump because is not grounded");

//                    Debug.Log("Ground hits = " + GroundHit);
//                    Debug.Log("Far from ground = " + FarFromGround);
//                }

//                return false;
//            }

//            return true;
//        }
//    }

//    // Crouched

//    public bool WantToGetUp { get; private set; } = false;

//    public bool Crouched { get; private set; } = false;

//    // Vaulting

//    public bool CanVault { get; private set; } = false;

//    public bool Vaulting { get; private set; } = false;

//    public float CurrentVaultAngle { get; private set; } = 0;

//    public bool InVaultableAngle
//    {
//        get
//        {
//            CurrentVaultAngle = Vector3.Angle(transform.forward, hitForwardVault.normal);

//            return (CurrentVaultAngle >= maxVaultAngle && CurrentVaultAngle <= 180);
//        }
//    }

//    public bool ThereIsSomethingInFrontOfMeChin
//    {
//        get
//        {
//            return Physics.Raycast(PlayerMiddle, transform.forward, out hitForwardVault, currentMinimumVaultDistance, vaultableLayers);
//        }
//    }

//    public bool ThereIsSomethingInFrontOfMeChest
//    {
//        get
//        {
//            return Physics.Raycast(PlayerMiddle + new Vector3(0, posH, 0), transform.forward, out hitFowardVaultUp, currentMinimumVaultDistance * 2);
//        }
//    }

//    public bool VaultableObjectForward
//    {
//        get
//        {
//            return vaultableTags.Contains(hitForwardVault.transform.tag);
//        }
//    }

//    public bool HasGroundForVaultEnd
//    {
//        get
//        {
//            hasGroundForVaultRayStart = PlayerMiddle + (transform.forward * (currentMinimumVaultDistance + vaultThickness));

//            return Physics.Raycast(hasGroundForVaultRayStart, Vector3.down, out hitDownVault, minVaultHeight);
//        }
//    }

//    // Falling

//    public bool FarFromGround
//    {
//        get
//        {
//            if (!Physics.Raycast(MiddleRayStart, Vector3.down, out FarFromGroundHit, minimumFallDistance))
//            {
//                return true;
//            }

//            return false;
//        }
//    }

//    public bool IsFalling { get; private set; } = false;

//    public float lastYGroundedPosition { get; private set; } = 0;

//    public float targetSpeed { get; private set; } = 0;

//    public float timeFalling { get; private set; } = 0;

//    public bool watingToStartFalling { get; private set; }

//    public Coroutine StartFallingNextFrameCoroutine { get; private set; }

//    public bool PlayingFallingAnimation
//    {
//        get
//        {
//            return playerAnimator.GetBool(AnimationHashUtility.PlayingFallingAnimation);
//        }
//    }

//    public float PlayerRadius { get; private set; } = 0.35f;

//    // Ray Starts
    
//    public Vector3 AboveHeadRayStart
//    {
//        get
//        {
//            return transform.position + new Vector3(0, characterController.height, 0);
//        }
//    }

//    private Vector3 FrontRightRayStart
//    {
//        get
//        {
//            return transform.position + new Vector3(PlayerRadius, GroundCheckOffset.y, PlayerRadius);
//        }
//    }
//    private Vector3 FrontLeftRayStart
//    {
//        get
//        {
//            return transform.position + new Vector3(-PlayerRadius, GroundCheckOffset.y, PlayerRadius);
//        }
//    }
//    private Vector3 BackRightRayStart
//    {
//        get
//        {
//            return transform.position + new Vector3(PlayerRadius, GroundCheckOffset.y, -PlayerRadius);
//        }
//    }
//    private Vector3 BackLeftRayStart
//    {
//        get
//        {
//            return transform.position + new Vector3(-PlayerRadius, GroundCheckOffset.y, -PlayerRadius);
//        }
//    }
//    private Vector3 MiddleRayStart
//    {
//        get
//        {
//            return transform.position + GroundCheckOffset;
//        }
//    }

//    // Constant

//    private RaycastHit sphereHit;



//    public bool GroundSphereHit
//    {
//        get
//        {
//            if (Physics.CheckSphere(transform.position, characterController.radius, groundLayers))
//            {
//                return true;
//            }

//            return false;
//        }
//    }

//    public bool GroundHit
//    {
//        get
//        {
//            float rayDistance = (OnSlope ? slopeGroundRayDistance : groundRayDistance);

//            if (Physics.Raycast(MiddleRayStart, Vector3.down, out middleHit, rayDistance, groundLayers))
//            {
//                return true;
//            }

//            if (Physics.Raycast(BackLeftRayStart, Vector3.down, out groundHitBL, rayDistance, groundLayers))
//            {
//                return true;
//            }

//            if (Physics.Raycast(BackRightRayStart, Vector3.down, out groundHitBR, rayDistance, groundLayers))
//            {
//                return true;
//            }

//            if (Physics.Raycast(FrontLeftRayStart, Vector3.down, out groundHitFL, rayDistance, groundLayers))
//            {
//                return true;
//            }

//            if (Physics.Raycast(FrontRightRayStart, Vector3.down, out groundHitFR, rayDistance, groundLayers))
//            {
//                return true;
//            }

//            return false;
//        }
//    }

//    public float HorizontalInput
//    {
//        get
//        {
//            return Input.GetAxis("Horizontal");
//        }
//    }

//    public float VerticalInput
//    {
//        get
//        {
//            return Input.GetAxis("Vertical");
//        }
//    }

//    public float PlayerVelocity { get; private set; } = 0;

//    public bool PressingRunKey
//    {
//        get
//        {
//            return Input.GetKey(runKey);
//        }
//    }

//    public Vector3 RightMovement
//    {
//        get
//        {
//            return transform.right * HorizontalInput;
//        }
//    }

//    public Vector3 ForwardMovement
//    {
//        get
//        {
//            return transform.forward * VerticalInput;
//        }
//    }

//    public Vector3 PlayerMiddle
//    {
//        get
//        {
//            return transform.position + new Vector3(0, characterController.height / 2, 0);
//        }
//    }
//}