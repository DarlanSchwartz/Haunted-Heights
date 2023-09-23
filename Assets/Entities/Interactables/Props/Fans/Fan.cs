using System.Collections;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class Fan : MonoBehaviour
{
    public enum FanSpeed { Low, Medium, High }
    public enum BladeAxis { X, Y, Z }
    public enum FanDirection { Clockwise, CounterClockwise }
    public enum FanState { Off, On, Broken }

    [Header("Fan Sounds")]
    [Space(5)]
    [SerializeField] private AudioClip fanButtonSound;
    [SerializeField] private float fanButtonSoundVolume = 0.07f;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private float turnOnSoundVolume = 0.07f;
    [SerializeField] private AudioClip turnOffSound;
    [SerializeField] private float turnOffSoundVolume = 0.07f;
    [SerializeField] private float turnOffSoundVolumeVelocityChange = 10;
    [SerializeField] private AudioClip loopSound;
    [SerializeField] private float loopSoundVolume = 0.07f;
    [SerializeField] private float loopSoundVolumeVelocityChange = 0.1f;
    [SerializeField] private float delaySoundBeforeLoop = 4f;
    [Header("Fan Pitch")]
    [Space(5)]
    [SerializeField] private float pitchLow = 1f;
    [SerializeField] private float pitchMedium = 1.1f;
    [SerializeField] private float pitchHigh = 1.2f;
    [SerializeField] private float pitchChangeSpeed = 0.5f;
    [Header("Fan Properties")]
    [Space(5)]
    [SerializeField] private FanState fanState = FanState.Off;
    [SerializeField] private FanDirection fanDirection = FanDirection.Clockwise;
    [SerializeField] private FanSpeed fanSpeed = FanSpeed.Low;
    [SerializeField] private BladeAxis bladeRotationAxis = BladeAxis.Z;
    [SerializeField] private Transform fanBlades;
    private float currentBladesSpeed = 1f;
    [Header("Fan Speeds")]
    [Space(5)]
    [SerializeField] private float fanSpeedLow = 800;
    [SerializeField] private float fanSpeedMedium = 1200;
    [SerializeField] private float fanSpeedHigh = 1800;
    [SerializeField] private float fanSpeedTurnedOffOrBroken = 0;
    [SerializeField] private float fanSpeedIncrementSwitching = 150;
    [SerializeField] private float fanSpeedDecrementSwitching = 150;
    [SerializeField] private float fanSpeedDecrementWhenTurnedOff = 70;
    private AudioSource audioSource;
    public FanState State { get => fanState; }
    public FanDirection Direction { get => fanDirection; }
    public FanSpeed Speed { get => fanSpeed; }
    public BladeAxis Axis { get => bladeRotationAxis; }
    public Transform FanBlades { get => fanBlades; }
    public float CurrentBladesSpeed { get => currentBladesSpeed; }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(TurnOn());
    }

    private void Update()
    {
        if ((fanState == FanState.Broken || fanState == FanState.Off) && currentBladesSpeed == 0)
        {
            return;
        }
        switch (fanState)
        {
            case FanState.Off:
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, turnOffSoundVolumeVelocityChange * Time.deltaTime);
                if (currentBladesSpeed > fanSpeedTurnedOffOrBroken)
                {
                    currentBladesSpeed -= fanSpeedDecrementWhenTurnedOff * Time.deltaTime;
                    if (currentBladesSpeed < fanSpeedTurnedOffOrBroken)
                    {
                        currentBladesSpeed = fanSpeedTurnedOffOrBroken;
                    }
                }
                break;
            case FanState.On:
                audioSource.volume = Mathf.Lerp(audioSource.volume, loopSoundVolume, loopSoundVolumeVelocityChange * Time.deltaTime);
                if (currentBladesSpeed < TargetSpeed)
                {
                    currentBladesSpeed += fanSpeedIncrementSwitching * Time.deltaTime;
                }
                else if (currentBladesSpeed > TargetSpeed)
                {
                    currentBladesSpeed -= fanSpeedDecrementSwitching * Time.deltaTime;
                }
                break;
            case FanState.Broken:
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, turnOffSoundVolumeVelocityChange * Time.deltaTime);

                if (currentBladesSpeed > fanSpeedTurnedOffOrBroken)
                {
                    currentBladesSpeed -= fanSpeedDecrementSwitching * Time.deltaTime;
                    if (currentBladesSpeed < fanSpeedTurnedOffOrBroken)
                    {
                        currentBladesSpeed = fanSpeedTurnedOffOrBroken;
                    }
                }
                break;
        }

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, TargetPitch, pitchChangeSpeed * Time.deltaTime);
        fanBlades.Rotate(currentBladesSpeed * Time.deltaTime * TargetRotationAxisAndDirection);
        MapSwitchVelocityInputs();
    }

    void MapSwitchVelocityInputs()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchSpeed(FanSpeed.Low);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchSpeed(FanSpeed.Medium);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchSpeed(FanSpeed.High);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            TurnOff();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (fanState != FanState.On)
            {
                StartCoroutine(TurnOn());
            }
        }
    }

    void SwitchSpeed(FanSpeed speedToSwitchTo)
    {
        AudioSource.PlayClipAtPoint(fanButtonSound, transform.position, fanButtonSoundVolume);
        fanSpeed = speedToSwitchTo;
    }

    void TurnOff()
    {
        fanState = FanState.Off;
        AudioSource.PlayClipAtPoint(turnOffSound, transform.position, turnOffSoundVolume);
    }

    IEnumerator TurnOn()
    {
        fanState = FanState.On;
        AudioSource.PlayClipAtPoint(turnOnSound, transform.position, turnOnSoundVolume);
        yield return new WaitForSeconds(delaySoundBeforeLoop);
        audioSource.clip = loopSound;
        audioSource.loop = true;
        audioSource.Play();
        yield break;
    }

    public Vector3 TargetRotationAxisAndDirection
    {
        get
        {
            return fanDirection switch
            {
                FanDirection.Clockwise => bladeRotationAxis switch
                {
                    BladeAxis.X => Vector3.right,
                    BladeAxis.Y => Vector3.up,
                    BladeAxis.Z => Vector3.forward,
                    _ => Vector3.forward,
                },
                FanDirection.CounterClockwise => bladeRotationAxis switch
                {
                    BladeAxis.X => Vector3.left,
                    BladeAxis.Y => Vector3.down,
                    BladeAxis.Z => Vector3.back,
                    _ => Vector3.back,
                },
                _ => Vector3.forward,
            };
        }
    }

    public float TargetSpeed
    {
        get
        {
            return fanSpeed switch
            {
                FanSpeed.Low => fanSpeedLow,
                FanSpeed.Medium => fanSpeedMedium,
                FanSpeed.High => fanSpeedHigh,
                _ => fanSpeedLow,
            };
        }
    }

    public float TargetPitch
    {
        get
        {
            return fanSpeed switch
            {
                FanSpeed.Low => pitchLow,
                FanSpeed.Medium => pitchMedium,
                FanSpeed.High => pitchHigh,
                _ => pitchLow,
            };
        }
    }
}
