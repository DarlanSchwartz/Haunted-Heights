using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMove
{
    private void OnDrawGizmos()
    {
        if(thisTransform == null) return;
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
        Gizmos.DrawWireSphere(thisTransform.position + (thisTransform.forward * 0.1f), spheresRadius);
        Gizmos.DrawWireSphere(thisTransform.position + (-thisTransform.forward * 0.1f), spheresRadius);

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
        Gizmos.DrawWireSphere(thisTransform.position + Vector3.up, spheresRadius);
        Gizmos.DrawWireSphere((thisTransform.position + Vector3.up) + thisTransform.forward * 7, spheresRadius);
        Gizmos.DrawLine(thisTransform.position + Vector3.up, (thisTransform.position + Vector3.up) + thisTransform.forward * 7);
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
                Vector3 targetPos = PlayerMiddle + new Vector3(0, m_chestYPosition, 0) + (thisTransform.forward * (GetCurrentVaultCheckDistance * 1.4f));

                Gizmos.color = Color.green;
                Gizmos.DrawLine(PlayerMiddle + new Vector3(0, m_chestYPosition, 0), targetPos);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPos, spheresRadius);

                // Vault down end indicator
                if (m_hitDownVault.point != Vector3.zero)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(PlayerMiddle + (thisTransform.forward * (GetCurrentVaultCheckDistance + 0.4f)), m_hitDownVault.point);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(m_hitDownVault.point, spheresRadius);
                }
                else
                {
                    if (!HasGroundForVaultEnd(0.4f))
                    {
                        Gizmos.color = Color.green;
                        Vector3 posT = PlayerMiddle + (thisTransform.forward * (GetCurrentVaultCheckDistance + 0.4f));
                        Gizmos.DrawLine(posT, posT + new Vector3(posT.x, thisTransform.position.y, posT.z));
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(posT + new Vector3(posT.x, thisTransform.position.y, posT.z), spheresRadius);
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                        Vector3 posT = m_hitForwardVault.point + (thisTransform.forward * (1 + 0.4f));
                        Gizmos.DrawLine(posT, m_hitDownVault.point);
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(m_hitDownVault.point, spheresRadius);
                    }
                }
            }
        }
        if (InLadder)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(thisTransform.position + ((-thisTransform.right * 0.8f) + thisTransform.forward * 2), 0.2f);
            Gizmos.DrawWireSphere(thisTransform.position + ((thisTransform.right * 0.8f) + thisTransform.forward * 2), 0.2f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_ForwardOntoHit.point,0.3f);
        Gizmos.DrawLine(PlayerMiddle, m_ForwardOntoHit.point);

        Vector3 start = thisTransform.position + (thisTransform.forward * VaultSettings.JumpOntoMaxDistance );
        start.y += 4;
        Physics.Raycast(start, Vector3.down, out RaycastHit _debugJumpOntoHit, 10);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawLine(start, _debugJumpOntoHit.point);

        Gizmos.DrawWireSphere(_debugJumpOntoHit.point, 0.1f);
        Gizmos.DrawIcon(start, "Camera");
        
        
    }
   

}
