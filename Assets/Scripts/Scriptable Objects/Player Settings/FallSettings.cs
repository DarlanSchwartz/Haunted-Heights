using UnityEngine;

[CreateAssetMenu(fileName = "New Fall Settings", menuName = "Player/Fall Settings", order = 0)]
public class FallSettings : ScriptableObject
{
    [Space(10)]
    [Header("Falling")] //----------------------------------------------------FALLING-------------------------------------------------------------------
    public LayerMask GroundLayers;
    [Tooltip("This value controls how far from ground you need to be to start the falling animation. \n\n Setting this to 0 means you will always start the falling animation no matter how hight you start to fall. \n\n This may cause unwanted animations when you fall from very low places, like steps from a staircase.")]
    public float MinFallDistance = 1.1f;
    [Tooltip("This value controls how much time you have to be out of the ground to be considered falling.")]
    [Range(0, 1)] public float MinFallTime = 0.1f;
    [Tooltip("This value how much gravity force is applyed on the character when you are falling.")]
    [Range(0, 500)] public float GravityForce = 40;
    [Tooltip("This value will be set on the character controller.height when you are falling.")]
    public float ControllerHeight= 3.19f;
    [Tooltip("This value will be set on the characterController.center when you are falling.")]
    public Vector3 ControllerCenter = new Vector3(0,2,0);
    [Space(10)]
    [Header("Air control")] //-------------------------------------------------AIR CONTROL--------------------------------------------------------------------
    [Tooltip("This controls how much you have control of the character in the air.")]
    [Range(0, 1)] public float AirControlFactor = 0.8f;
    [Tooltip("This controls how much you can move the character in the air.")]
    [Range(0,300)]public float AirResistance = 230;
}
