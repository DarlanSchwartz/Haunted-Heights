using UnityEngine;
[CreateAssetMenu(fileName = "New Climb Settings", menuName = "Player/Climb Settings", order = 0)]
public class ClimbSettings : ScriptableObject
{
    [Space(10)]
    [Header("Climb")] //-------------------------------------------------------------------CLIMB--------------------------------------------------------------------------------
    public string HangableTags = "Grabable,Grab";
    [Range(90, 179)] public float MaxAngle = 130;
    [Range(0, 5)] public float MaxDistance = 3;
    public float MouseLookMaxXGrab = 60;
    public float MouseLookMaxXFreeHang = 35;
    public float ControllerHeight = 3.5f;
    public Vector3 ControllerCenter = new Vector3(0, 1.22f, -0.49f);
    public Vector3 StartOffset = new Vector3(0, 2, 0);
    public LayerMask Layers;
}
