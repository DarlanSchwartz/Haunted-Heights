using UnityEngine;
[CreateAssetMenu(fileName = "New Jump Settings", menuName = "Player/Jump Settings", order = 0)]
public class JumpSettings : ScriptableObject
{
    [Space(10)]
    [Header("Jump")] //-------------------------------------------------------JUMP-----------------------------------------------------------------
    [Tooltip("This curve controls the movement you will do when you jump and dont hit anything.")]
    public AnimationCurve FallOff;
    [Tooltip("This controls the jump force multiplier.")]
    [Range(0, 20)] public float Force = 5;
    [Tooltip("Controls how far the raycast will go above head to check if there is something in the way while jumping.")]
    [Range(0, 1)] public float HeadHitAboveRayLenght = 0.2f;
    [Tooltip("This value controls how high you'll be able to climb up when you are jumping.\n Higher values means you can jump up very high places.")]
    [Range(0, 2)] public float ControllerStepOffset = 0.1f;
}

