using UnityEngine;

[CreateAssetMenu(fileName = "New Debug Settings", menuName = "Player/Debug Settings", order = 0)]
public class DebugSettings : ScriptableObject
{
    [Space(10)]
    [Header("Debug")]
    public bool DebugCanJump = false;
    public bool DebugJump = false;
    public bool DebugLand = false;
    public bool DebugFalling = false;
    public bool DebugVault = false;
    public bool DebugCrouch = false;
    public bool DebugMove = false;
    public bool DebugHang = false;
    public bool DebugSlide = false;
}