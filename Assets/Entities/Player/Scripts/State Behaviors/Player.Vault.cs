using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{

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
                !animator.GetBool(AnimationHashUtility.PlayingJumpAnimation) &&
                ThereIsSomethingInFrontOfMeChin &&
                VaultableObjectForward &&
                !ThereIsSomethingInFrontOfMeChest;
        }
        private set { }
    }
    public bool Vaulting { get; private set; } = false;



    public float CurrentVaultAngle
    {
        get
        {
            m_currentVaultAngle = Vector3.Angle(thisTransform.forward, m_hitForwardVault.normal);

            return m_currentVaultAngle;
        }
        private set
        {
            m_currentVaultAngle = value;
        }
    }

    public bool InVaultableAngle { get { return (CurrentVaultAngle >= VaultSettings.MaxAngle && CurrentVaultAngle <= 180); } }
    private Vector3 VaultRaycastMiddleStart { get { return thisTransform.position + new Vector3(0, VaultSettings.MinVaultHeight, 0); } }
    private Vector3 JumpOntoRaycastStart { get { return thisTransform.position + new Vector3(0, DefaultSettings.ControllerHeight / 4, 0); } }
    public bool ThereIsSomethingInFrontOfMeChin { get { return Physics.Raycast(VaultRaycastMiddleStart, thisTransform.forward, out m_hitForwardVault, GetCurrentVaultCheckDistance, VaultSettings.Layers); } }
    public bool ThereIsSomethingInFrontOfMyKnees { get { return Physics.Raycast(JumpOntoRaycastStart, thisTransform.forward, out m_hitForwardJumpUp, GetCurrentVaultCheckDistance, VaultSettings.Layers); } }
    public bool ThereIsSomethingInFrontOfMeChest { get { return Physics.Raycast(PlayerMiddle + new Vector3(0, m_chestYPosition, 0), thisTransform.forward, out m_hitFowardVaultUp, GetCurrentVaultCheckDistance * 1.4f); } }

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
        m_hasGroundForVaultRayStart = PlayerMiddle + (thisTransform.forward * (GetCurrentVaultCheckDistance + vaultThickness));

        return Physics.Raycast(m_hasGroundForVaultRayStart, Vector3.down, out m_hitDownVault, 2.01f);
    }

    public Vector3 GetJumpOntoEndPosition()
    {

        Vector3 start = thisTransform.position + (thisTransform.forward * VaultSettings.JumpOntoMaxDistance);
        start.y += 4;

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
    private void VaultMethod()
    {
        AnimationBehaviorVault bv;

        if (AnimatorVertical > -0.1f)
        {
            // CHECK IF THERE IS SOME WALL OR THING OBSTRUCTING THE LEFT OR THE RIGHT OF THE PLAYER
            bool SomethingRight = Physics.Raycast(PlayerMiddle, thisTransform.right, out _, 1.5f, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);
            bool SomethingLeft = Physics.Raycast(PlayerMiddle, -thisTransform.right, out _, 1.5f, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);

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
                    animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                    finalDirection = VaultDirection.Forward;
                    break;
                case VaultDirection.Left:
                    if (!SomethingRight)
                    {
                        animator.SetBool(AnimationHashUtility.VaultRight, false);
                        finalDirection = VaultDirection.Left;
                    }
                    else if (SomethingRight)
                    {
                        animator.SetBool(AnimationHashUtility.VaultRight, true);
                        finalDirection = VaultDirection.Right;
                    }
                    else if (SomethingRight && SomethingLeft)
                    {
                        animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                        finalDirection = VaultDirection.Forward;
                    }
                    break;
                case VaultDirection.Right:
                    if (!SomethingLeft)
                    {
                        animator.SetBool(AnimationHashUtility.VaultRight, true);
                        finalDirection = VaultDirection.Right;
                    }
                    else if (SomethingLeft)
                    {
                        animator.SetBool(AnimationHashUtility.VaultRight, false);
                        finalDirection = VaultDirection.Left;
                    }
                    else if (SomethingRight && SomethingLeft)
                    {
                        animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                        finalDirection = VaultDirection.Forward;
                    }
                    break;
                default:
                    animator.SetBool(AnimationHashUtility.MonkeyVault, true);
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
                animator.SetBool(AnimationHashUtility.MonkeyVault, false);
                finalDirection = vaultObject.defaultDirection;
                if (finalDirection == VaultDirection.Right)
                {
                    animator.SetBool(AnimationHashUtility.VaultRight, true);

                }
                else if (finalDirection == VaultDirection.Left)
                {
                    animator.SetBool(AnimationHashUtility.VaultRight, false);
                }
                else
                {
                    animator.SetBool(AnimationHashUtility.VaultRight, true);
                    finalDirection = VaultDirection.Right;
                }
            }

            VaultDirection vaultDirection = finalDirection;

            m_targetFinalVault = m_hitForwardVault.point + (thisTransform.forward * ((1 * VaultSettings.SpeedMaxDistanceMultiplier) + vaultObject.Thickness));
            m_targetFinalVault.y = HasGroundForVaultEnd(vaultObject.Thickness) ? m_hitDownVault.point.y : thisTransform.position.y;

            AnimationBehaviorVault[] vaultBehaviours = animator.GetBehaviours<AnimationBehaviorVault>();


            foreach (AnimationBehaviorVault behavior in vaultBehaviours)
            {
                if (behavior.direction == vaultDirection && behavior.vaultMode == vaultMode)
                {
                    switch (vaultMode)
                    {
                        case VaultSpeed.Idle:
                            animator.SetBool(AnimationHashUtility.VaultRun, false);
                            animator.SetBool(AnimationHashUtility.Idle, true);
                            animator.SetFloat(AnimationHashUtility.Vertical, 0);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Walking:
                            animator.SetBool(AnimationHashUtility.VaultRun, false);
                            animator.SetBool(AnimationHashUtility.Idle, false);
                            animator.SetFloat(AnimationHashUtility.Vertical, 1);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Running:
                            animator.SetBool(AnimationHashUtility.VaultRun, true);
                            animator.SetBool(AnimationHashUtility.Idle, false);
                            animator.SetFloat(AnimationHashUtility.Vertical, 1);
                            m_TargetSpeed = 0;
                            break;
                        case VaultSpeed.Monkey:
                            animator.SetBool(AnimationHashUtility.VaultRun, false);
                            animator.SetBool(AnimationHashUtility.MonkeyVault, true);
                            animator.SetBool(AnimationHashUtility.Idle, false);
                            animator.SetFloat(AnimationHashUtility.Vertical, 1);
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
        animator.SetBool(AnimationHashUtility.Vaulting, true);

        Controller.height = VaultSettings.ControllerHeight;
        Controller.center = VaultSettings.ControllerCenter;

        animator.ResetTrigger(AnimationHashUtility.Land);
        animator.SetTrigger(AnimationHashUtility.Vault);
    }

    private void PlaceHandOnVaultTarget(VaultableObject vaultableObject, VaultDirection directionVault)
    {
        switch (directionVault)
        {
            case VaultDirection.Left:

                RaycastHit hitL;
                bool foundHandPlacementLeft = Physics.Raycast(PlayerMiddle - thisTransform.right, thisTransform.forward, out hitL, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetHandPositionLeft = foundHandPlacementLeft ? hitL.point : m_hitForwardVault.point + -thisTransform.right;

                // place hand on the correct vault height
                targetHandPositionLeft.y += vaultableObject.Height;

                //place hand on the left of the character

                HandIK.SetTargetPositions(targetHandPositionLeft, Vector3.zero, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(thisTransform.rotation, Quaternion.identity);

                HandIK.WeightSpeed = 3;

                HandIK.StartFollowingTargets(true, false);

                break;
            case VaultDirection.Right:

                RaycastHit hitR;
                bool foundHandPlacementRight = Physics.Raycast(PlayerMiddle + thisTransform.right, thisTransform.forward, out hitR, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetHandPositionRight = foundHandPlacementRight ? hitR.point : m_hitForwardVault.point + thisTransform.right;

                // place hand on the correct vault height
                targetHandPositionRight.y += vaultableObject.Height;

                //place hand on the right of the character

                HandIK.SetTargetPositions(Vector3.zero, targetHandPositionRight, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(Quaternion.identity, thisTransform.rotation);

                HandIK.WeightSpeed = 3;

                HandIK.StartFollowingTargets(false, true);

                break;
            case VaultDirection.Forward:

                RaycastHit hitRF;
                RaycastHit hitLF;
                bool foundHandPlacementRightForward = Physics.Raycast(PlayerMiddle + thisTransform.right, thisTransform.forward, out hitRF, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                bool foundHandPlacementLeftForward = Physics.Raycast(PlayerMiddle - thisTransform.right, thisTransform.forward, out hitLF, GetCurrentVaultCheckDistance + 0.5f, VaultSettings.Layers);
                Vector3 targetForwardHandPositionRight = foundHandPlacementRightForward ? hitRF.point : m_hitForwardVault.point + thisTransform.right;
                Vector3 targetForwardHandPositionLeft = foundHandPlacementLeftForward ? hitLF.point : m_hitForwardVault.point + -thisTransform.right;

                targetForwardHandPositionRight.y += vaultableObject.Height;
                targetForwardHandPositionLeft.y += vaultableObject.Height;

                HandIK.SetTargetPositions(targetForwardHandPositionLeft, targetForwardHandPositionRight, false);

                HandIK.SetTargetsParent(vaultableObject.transform);

                HandIK.SetTargetRotations(thisTransform.rotation, thisTransform.rotation);
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
        Vector3 startPos = thisTransform.position;

        if (DebugSettings.DebugVault)
        {
            Debug.Log("Started Vault Coroutine.");
        }

        float timePassed = 0;

        do
        {
            thisTransform.position = Vector3.Lerp(startPos, m_targetFinalVault, vaultAnim.AnimationDelta);
            timePassed += Time.deltaTime;

            if (timePassed >= 0.5f)
            {
                HandIK.DisableHandIK();
            }

            yield return null;
        } while (!vaultAnim.Complete);
        EndVault();

        if (!OnGround)
        {
            //Force start Falling
            animator.SetBool(AnimationHashUtility.FarFromGround, true);
            animator.SetBool(AnimationHashUtility.OnGround, false);
            animator.SetBool(AnimationHashUtility.PlayerFalling, true);
            StartFalling();
        }

        yield break;
    }

    private void EndVault()
    {
        Vaulting = false;
        HandIK.DisableHandIK();
        MouseLook.UpdateRotation = true;
        animator.SetBool(AnimationHashUtility.Vaulting, false);
        Controller.center = DefaultSettings.ControllerCenter;
        Controller.height = DefaultSettings.ControllerHeight;
    }
}
