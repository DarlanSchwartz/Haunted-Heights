using UnityEngine;
public enum CrouchMode { Toggle, Hold }
[CreateAssetMenu(fileName = "New Crouch Settings", menuName = "Player/Crouch Settings", order = 0)]
public class CrouchSettings : ScriptableObject
{
    [Space(10)]
    [Header("Crouch")] //------------------------------------------------CROUCH-------------------------------------------------------------------------------
    [Tooltip("This controls the crouch mode, toggle you have to press the crouch key everytime you want to crouch or get out of crouch state./nHold you have to hold the crouch key when you want to be crouched.")]
    public CrouchMode Mode = CrouchMode.Hold;
    [Range(0, 5)] public float HeadHitAboveRayLenght = 1.5f;
    [Tooltip("This will be the camera position when you crouch.")]
    public float ControllerStepOffset = 0.1f;
    [Tooltip("This value will be set on the character controller.height when you crouch.")]
    public float ControllerHeight = 2.2f;
    [Tooltip("This value will be set on the characterController.center when you crouch.")]
    public Vector3 ControllerCenter = new Vector3(0,1.1f,0);
    [Space(10)]
    [Header("Stamina")]
    public bool UseStamina = false;
    [Range(0, 10)] public float MaxStamina = 1;
    [Range(0, 1)] public float StaminaDecreasePerCrouch = 0.2f;
    [Range(0, 1)] public float StaminaRegenerationRate = 0.05f;
    [Range(0, 1)] public float MinStaminaToCrouch = 0.2f;
}
