using System.Collections;
using UnityEngine;

public class PlayerStateVault : PlayerState
{
    public PlayerStateVault(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory, PlayerStateType.Vaulting)
    {
        IsRootState = true;
       
    }

    Vector3 targetVaultEndPosition;
    Vector3 startVaultPosition;
    BehaviourVault currentVaultAnimationBehaviour;
    readonly float sideWallsCheckDistance = 1.5f;

    public override void EnterState()
    {
        Context.Vaulting = true;
        Context.Falling = false;
        Context.MouseLook.UpdateRotation = false;
        Context.Animator.SetBool(AnimationHashUtility.Vaulting, true);
        Context.Animator.SetBool(AnimationHashUtility.Stand, false);

        bool isThereSomethingRightOfThePlayer = Physics.Raycast(Context.PlayerMiddle, Context.ThisTransform.right, out _, sideWallsCheckDistance, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);
        bool isThereSomethingLeftOfThePlayer = Physics.Raycast(Context.PlayerMiddle, -Context.ThisTransform.right, out _, sideWallsCheckDistance, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);
        VaultableObject vaultObject = Context.HitForwardVault.transform.GetComponent<VaultableObject>();

        VaultDirection finalVaultDirection = GetVaultDirection(vaultObject, isThereSomethingRightOfThePlayer, isThereSomethingLeftOfThePlayer);
        VaultSpeed vaultSpeedMode = finalVaultDirection == VaultDirection.Forward ? VaultSpeed.Monkey : Context.AnimatorVertical < 0.1f ? VaultSpeed.Idle : Context.AnimatorVertical <= 1 && Context.AnimatorVertical >= 0.1f ? VaultSpeed.Walking : Context.AnimatorVertical > 1 ? VaultSpeed.Running : VaultSpeed.Idle;
        currentVaultAnimationBehaviour = GetCorrectVaultAnimBehaviour(finalVaultDirection, vaultSpeedMode);
        PlayCorrectVaultAnimation(finalVaultDirection, vaultSpeedMode);
        PlaceHandOnVaultTarget(vaultObject, finalVaultDirection);

        Context.Controller.height = Context.VaultSettings.ControllerHeight;
        Context.Controller.center = Context.VaultSettings.ControllerCenter;

        targetVaultEndPosition = Context.HitForwardVault.point + (Context.ThisTransform.forward * ((1 * Context.VaultSettings.SpeedMaxDistanceMultiplier) + vaultObject.Thickness));
        targetVaultEndPosition.y = Context.HasGroundForVaultEnd(vaultObject.Thickness) ? Context.HitDownVault.point.y : Context.ThisTransform.position.y;
        startVaultPosition = Context.ThisTransform.position;

        Context.StartCoroutine(VaultCoroutine());
    }
    public VaultDirection GetVaultDirection(VaultableObject vaultObject, bool SomethingRight, bool SomethingLeft)
    {
        VaultDirection finalDirection = vaultObject.defaultDirection;

        switch (vaultObject.defaultDirection)
        {
            case VaultDirection.Forward:
                finalDirection = VaultDirection.Forward;
                break;
            case VaultDirection.Left:
                if (!SomethingRight)
                {
                    finalDirection = VaultDirection.Left;
                }
                else if (SomethingRight)
                {
                    finalDirection = VaultDirection.Right;
                }
                else if (SomethingRight && SomethingLeft)
                {
                    finalDirection = VaultDirection.Forward;
                }
                break;
            case VaultDirection.Right:
                if (!SomethingLeft)
                {
                    finalDirection = VaultDirection.Right;
                }
                else if (SomethingLeft)
                {
                    finalDirection = VaultDirection.Left;
                }
                else if (SomethingRight && SomethingLeft)
                {
                    finalDirection = VaultDirection.Forward;
                }
                break;
            default:
                finalDirection = VaultDirection.Forward;
                break;
        }

        return finalDirection;
    }
    private BehaviourVault GetCorrectVaultAnimBehaviour(VaultDirection dir, VaultSpeed mode)
    {
        BehaviourVault[] vaultBehaviours = Context.Animator.GetBehaviours<BehaviourVault>();
        foreach (BehaviourVault behavior in vaultBehaviours)
        {
            if (behavior.direction == dir && behavior.vaultMode == mode)
            {
                behavior.Reset();
                return behavior;
            }
        }
        Debug.LogError("Something went wrong while trying to find the correct vault behaviour state on" + this);
        Debug.Break();
        return null;
    }
    private void PlayCorrectVaultAnimation(VaultDirection finalDirection, VaultSpeed vaultMode)
    {
        switch (vaultMode)
        {
            case VaultSpeed.Idle:
                switch (finalDirection)
                {
                    case VaultDirection.Left:
                        Context.Animator.Play("Vault_Idle_L", 0);
                        break;
                    case VaultDirection.Right:
                        Context.Animator.Play("Vault_Idle_R", 0);
                        break;
                    case VaultDirection.Forward:
                        Context.Animator.Play("MonkeyVault", 0);
                        break;
                }
                break;
            case VaultSpeed.Walking:
                switch (finalDirection)
                {
                    case VaultDirection.Left:
                        Context.Animator.Play("Vault_Walking_L", 0);
                        break;
                    case VaultDirection.Right:
                        Context.Animator.Play("Vault_Walking_R", 0);
                        break;
                    case VaultDirection.Forward:
                        Context.Animator.Play("MonkeyVault", 0);
                        break;
                }
                break;
            case VaultSpeed.Running:
                switch (finalDirection)
                {
                    case VaultDirection.Left:
                        Context.Animator.Play("Vault_Run_L", 0);
                        break;
                    case VaultDirection.Right:
                        Context.Animator.Play("Vault_Run_R", 0);
                        break;
                    case VaultDirection.Forward:
                        Context.Animator.Play("MonkeyVault", 0);
                        break;
                }
                break;
            case VaultSpeed.Monkey:
                Context.Animator.Play("MonkeyVault", 0);
                break;
        }
    }
    private void PlaceHandOnVaultTarget(VaultableObject vaultableObject, VaultDirection directionVault)
    {
        Context.HandIK.SetTargetsParent(vaultableObject.transform);
        Context.HandIK.WeightSpeed = 3;
        switch (directionVault)
        {
            case VaultDirection.Left:
                bool foundHandPlacementLeft = Physics.Raycast(Context.PlayerMiddle - Context.ThisTransform.right, Context.ThisTransform.forward, out RaycastHit hitL, Context.GetCurrentVaultCheckDistance + 0.5f, Context.VaultSettings.Layers);
                Vector3 targetHandPositionLeft = foundHandPlacementLeft ? hitL.point : Context.HitForwardVault.point + -Context.ThisTransform.right;

                targetHandPositionLeft.y += vaultableObject.Height;

                Context.HandIK.SetTargetPositions(targetHandPositionLeft, Vector3.zero, false);
                Context.HandIK.SetTargetRotations(Context.ThisTransform.rotation, Quaternion.identity);
                Context.HandIK.StartFollowingTargets(true, false);

                break;
            case VaultDirection.Right:
                bool foundHandPlacementRight = Physics.Raycast(Context.PlayerMiddle + Context.ThisTransform.right, Context.ThisTransform.forward, out RaycastHit hitR, Context.GetCurrentVaultCheckDistance + 0.5f, Context.VaultSettings.Layers);
                Vector3 targetHandPositionRight = foundHandPlacementRight ? hitR.point : Context.HitForwardVault.point + Context.ThisTransform.right;
                Debug.DrawLine(Context.ThisTransform.position, targetHandPositionRight);

                targetHandPositionRight.y += vaultableObject.Height;

                Context.HandIK.SetTargetPositions(Vector3.zero, targetHandPositionRight, false);
                Context.HandIK.SetTargetRotations(Quaternion.identity, Context.ThisTransform.rotation);
                Context.HandIK.StartFollowingTargets(false, true);

                break;
            case VaultDirection.Forward:
                bool foundHandPlacementRightForward = Physics.Raycast(Context.PlayerMiddle + Context.ThisTransform.right, Context.ThisTransform.forward, out RaycastHit hitRF, Context.GetCurrentVaultCheckDistance + 0.5f, Context.VaultSettings.Layers);
                bool foundHandPlacementLeftForward = Physics.Raycast(Context.PlayerMiddle - Context.ThisTransform.right, Context.ThisTransform.forward, out RaycastHit hitLF, Context.GetCurrentVaultCheckDistance + 0.5f, Context.VaultSettings.Layers);
                Vector3 targetForwardHandPositionRight = foundHandPlacementRightForward ? hitRF.point : Context.HitForwardVault.point + Context.ThisTransform.right;
                Vector3 targetForwardHandPositionLeft = foundHandPlacementLeftForward ? hitLF.point : Context.HitForwardVault.point + -Context.ThisTransform.right;

                targetForwardHandPositionRight.y += vaultableObject.Height;
                targetForwardHandPositionLeft.y += vaultableObject.Height;

                Context.HandIK.SetTargetPositions(targetForwardHandPositionLeft, targetForwardHandPositionRight, false);
                Context.HandIK.SetTargetRotations(Context.ThisTransform.rotation, Context.ThisTransform.rotation);
                Context.HandIK.StartFollowingTargets(true, true);
                break;
        }
    }
    private IEnumerator VaultCoroutine()
    {
        float timePassed = 0;

        do
        {
            Context.ThisTransform.position = Vector3.Lerp(startVaultPosition, targetVaultEndPosition, currentVaultAnimationBehaviour.AnimationDelta);
            Context.SetPlayerCameraPosition(Context.Speed.VaultCameraSpeed);
            timePassed += Time.deltaTime;

            if (timePassed >= 0.5f)
            {
                Context.HandIK.DisableHandIK();
            }

            yield return null;
        } while (!currentVaultAnimationBehaviour.Complete);

        if (Context.OnGround)
        {
            SwitchState(Factory.Grounded());
        }
        else
        {
            SwitchState(Factory.Falling());
        }

        yield break;
    }
    public override void ExitState()
    {
        Context.Vaulting = false;
        Context.HandIK.DisableHandIK();
        Context.MouseLook.UpdateRotation = true;
        Context.Animator.SetBool(AnimationHashUtility.Vaulting, false);
        //Context.Animator.SetBool(AnimationHashUtility.Stand, true);
        //Context.Controller.height = Context.DefaultSettings.ControllerHeight;
        //Context.Controller.center = Context.DefaultSettings.ControllerCenter;
        //Context.Controller.stepOffset = Context.DefaultSettings.ControllerStepOffset;
    }
    public override void UpdateState() { }
    public override void InitializeSubState() { }
    public override void OnCollisionEnter() { }
    public override void OnCollisionExit() { }
    public override void OnCollisionStay() { }
    public override void CheckSwitchStates() { }
}
