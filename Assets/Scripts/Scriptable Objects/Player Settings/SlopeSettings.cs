using UnityEngine;
[CreateAssetMenu(fileName = "New Slope Settings", menuName = "Player/Slope Settings", order = 0)]
public class SlopeSettings : ScriptableObject
{
    [Range(0, 1)]public float RayLenght = 1;
    [Range(0, 1000)] public float SlidingForce = 500;
    [Range(0, 1000)] public float Force = 500;
}