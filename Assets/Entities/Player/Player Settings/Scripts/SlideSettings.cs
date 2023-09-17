using UnityEngine;
[CreateAssetMenu(fileName = "New Slide Settings", menuName = "Player/Slide Settings", order = 0)]
public class SlideSettings : ScriptableObject
{
    [Space(10)]
    [Header("Sliding")] //-------------------------------------------------------------------SLIDE--------------------------------------------------------------------------------
    public float ControllerHeight = 2.4f;
    public Vector3 ControllerCenter = new Vector3(0, 1.2f, 0);
    [Range(0,2)]public float AnimationLenght = 1.033f;
    public float HeadHitAboveRayLenght = 2;
    [Range(0,2)]public float ControllerRadius = 0.8f;
}
