using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceBeam : MonoBehaviour
{
    public Transform PointA;    
    public Transform PointB;

    public Vector3 MoveAxis = Vector3.forward;

    public BalanceBeamInvisibleWallTriggerChecker pointA;
    public BalanceBeamInvisibleWallTriggerChecker pointB;

}
