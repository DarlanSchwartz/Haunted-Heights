using UnityEngine;

[CreateAssetMenu(fileName = "New Vault Settings", menuName = "Player/Vault Settings", order = 0)]
public class VaultSettings : ScriptableObject
{
    [Space(10)]
    [Header("Vault")] //----------------------------------------------------------VAULT--------------------------------------------------------
    [Tooltip("This value controls wich layers should be seen by the vault raycast.")]
    public LayerMask Layers;
    [Tooltip("This value controls wich obstacles are vautable by tag.")]
    public string VaultableTags = "Vaultable,Etc";
    [Tooltip("This value sets the minimum angle that can be between you and your obstacle to you be able to vault over it.")]
    [Range(90, 179)] public float MaxAngle = 130;
    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when running.")]
    public float MinVaultDistRunning = 5.5f;
    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when walking.")]
    public float MinVaultDistWalking = 3.7f;
    [Tooltip("This value controls the minimum distance needed between you and your obstacle to you be able to vault over it when idle.")]
    public float MinVaultDistIdle = 2.5f;
    [Tooltip("This value controls the minimum height needed over the obstacle that you are looking for to you be able to vault over it.\n Setting this to 0 makes you able to vault up to something.")]
    public float MinVaultHeight = 1;
    [Tooltip("This value will be set on the character controller.height when you start vaulting.")]
    public float SpeedMaxDistanceMultiplier = 3f;
    [Tooltip("This value will be set on the character controller.height when you start vaulting.")]
    public float ControllerHeight = 1.42f;
    [Tooltip("This value will be set on the characterController.height when you start vaulting.")]
    public Vector3 ControllerCenter = Vector3.zero;
    [Range(0.1f,6f)]
    public float JumpOntoMaxDistance = 3;
    public AnimationCurve VaultCameraAnimRot;

}