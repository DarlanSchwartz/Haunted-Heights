using UnityEngine;

[CreateAssetMenu(fileName = "New Speed Settings", menuName = "Player/Speed Settings", order = 0)]
public class SpeedSettings : ScriptableObject
{
    [Space(10)]
    [Header("Speed values")] //---------------------------------------SPEED---------------------------------------------------

    [Tooltip("Speed pressing WDSA keys.")][Range(0.01f, 100)] public float WalkSpeed = 5;
    [Tooltip("Speed pressing run key and WDSA keys.")][Range(0.01f, 100)] public float RunSpeed = 4;
    [Tooltip("Speed crouched and pressing WDSA keys.")][Range(0.01f, 100)] public float CrouchedSpeed = 12;
    [Tooltip("Speed proned and pressing WDSA keys.")][Range(0.01f, 100)] public float PronedSpeed = 4;
    [Tooltip("Speed when moving in balance keys.")][Range(0.01f, 100)] public float BalanceSpeed = 12;
    [Tooltip("Speed when sliding/surfing on steep slopes.")][Range(0.01f, 500)] public float SlideSpeed = 30;
    [Tooltip("Speed transition from your position to the hanging position.")][Range(0.01f, 100)] public float HangSpeed = 15;
    [Tooltip("Speed to climb up to something when you already are ledge hanged on it.")][Range(0.01f, 100)] public float ClimbSpeed = 2;
    [Tooltip("Speed to climb up to something when you already are free hanged on it.")][Range(0.01f, 100)] public float ClimbFreeSpeed = 1;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 100)] public float SlideUnderSpeed = 10;
    [Tooltip("Speed up from idle to running.")][Range(0.01f, 100)] public float Acceleration = 12;
    [Tooltip("Speed Transition on locomotion animations.")][Range(0.01f, 40)] public float AnimationSmooth = 11;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float IdleCameraSpeed = 10;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float CameraSpeed = 12.5f;
    [Tooltip("Speed Transition on camera position to head position on crouch state.")][Range(0.01f, 40)] public float CrouchCameraSpeed = 20;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float LandCameraSpeed = 15;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float VaultCameraSpeed = 4;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 1000)] public float ClimbCameraSpeed = 40;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 1000)] public float HangCameraSpeed = 40;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float FallingCameraSpeed = 5;
    [Tooltip("Speed Transition on camera position to head position on locomotion state.")][Range(0.01f, 40)] public float SlideUnderCameraSpeed = 15;
    [Tooltip("Speed to step up ladder steps in a vertical ladder")][Range(0.01f, 100)] public float LadderSpeed = 6;
}