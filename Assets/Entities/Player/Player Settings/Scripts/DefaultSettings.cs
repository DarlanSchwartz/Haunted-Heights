using UnityEngine;

[CreateAssetMenu(fileName = "New Default Settings", menuName = "Player/Default Settings", order = 0)]
public class DefaultSettings : ScriptableObject
{
    [Tooltip("Character controller center when not crouched and on ground.")]
    public Vector3 ControllerCenter = new Vector3(0,2,0);
    [Tooltip("Character controller height when not crouched and on ground.")]
    public float ControllerHeight = 4;
    public float ControllerRadius = 0.5f;
    [Tooltip("Step Offset when on ground")]
    public float ControllerStepOffset = 0.98f;
    [Tooltip("Slope limit when on ground.")]
    public float ControllerSlopeLimit = 45;
    [Tooltip("How far the raycast will go above head to check if there is something in the way while grounded.")]
    [Range(0, 2)] public float HeadHitAboveRayLenght = 0.65f;
}
