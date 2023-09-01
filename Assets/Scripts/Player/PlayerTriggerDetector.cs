using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerDetector : MonoBehaviour
{
    public bool insideTrigger = false;
    public bool insideBuilding;
    public string insideBuildingTTag = "InsideBuilding";
    public string balanceBeamTTag = "BalanceBeam";
    public string balanceBeamBetweenTTag = "BalanceBeamBetween";
    private PlayerMove pmove;

    private void Awake()
    {
        pmove = GetComponent<PlayerMove>();
    }

    private void OnTriggerEnter(Collider other)
    {
        insideTrigger = true;

        if (other.CompareTag(balanceBeamBetweenTTag))
        {
            pmove.inBetweenBalanceMode = true;
        }

        if (other.CompareTag(balanceBeamTTag))
        {
            pmove.HandleEnterBalanceBeam(other.GetComponentInParent<BalanceBeam>(),other.transform, true);
        } 
    }

    private void OnTriggerStay(Collider other)
    {
        insideTrigger = true;

        if(other.CompareTag(insideBuildingTTag))
        {
            insideBuilding = true;
        }

        if (other.CompareTag(balanceBeamBetweenTTag))
        {
            pmove.inBetweenBalanceMode = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        insideTrigger = false;
        insideBuilding = false;

        if (other.CompareTag(balanceBeamTTag))
        {
            pmove.HandleEnterBalanceBeam(other.GetComponentInParent<BalanceBeam>(), other.transform, false);
        }

        if (other.CompareTag(balanceBeamBetweenTTag))
        {
            pmove.inBetweenBalanceMode = false;
        }
    }
}
