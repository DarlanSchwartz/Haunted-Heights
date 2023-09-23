using System.Collections.Generic;
using UnityEngine;

public enum DetectionStyle { DirectlyBelow,PerFeetPosition};

public class FeetSounds : MonoBehaviour
{
    public bool DebugRays = false;
    public AudioSource AudioSource;
    
    [Range(0, 1)]
    public float minimumTimeBetweenSteps = 0.15f;
    public FallSettings FallSettings;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    [Range(0f, 10f)]
    [Tooltip("In seconds")]
    public float DestroyParticlesAfter = 2;
    [Tooltip("This controls wether the raycast will be only one and at the center of the players position or one raycast on each foot of the player.")]
    public DetectionStyle DetectionStyle = DetectionStyle.PerFeetPosition;
    public bool RandomPich = false;
    [Range(0, 1)]
    public float MaxPitch = 1;
    [Range(0, 1)]
    public float MinPitch = 0;
    public QueryTriggerInteraction TriggerInteraction;
    private Animator animator;
    private float timeSinceLastStepCall;
    private RaycastHit hitMaterial;
    private RaycastHit leftFootHitMaterial;
    private RaycastHit rightFootHitMaterial;
    public List<FeetSoundStructure> Sounds;

    // Raycast downwards to check wich material am i stepping on
    private bool PlayerIsOnGround { get { return Physics.Raycast(transform.position + FootRayStartOffset, Vector3.down, out hitMaterial, raycastDistance, FallSettings.GroundLayers,TriggerInteraction); } }
    private bool PlayerIsIdle { get { return animator.GetBool(AnimationHashUtility.Idle); } }
    public float AnimatorVertical { get { return animator.GetFloat(AnimationHashUtility.Vertical); } }
    public float AnimatorHorizontal { get { return animator.GetFloat(AnimationHashUtility.Horizontal); } }
    public bool PlayerIsLanding { get { return animator.GetBool(AnimationHashUtility.PlayingLandAnimation); } }
    public bool PLayerIsFalling { get { return animator.GetBool(AnimationHashUtility.PlayingFallingAnimation); } }
    public bool PlayerIsJumping { get { return animator.GetBool(AnimationHashUtility.PlayingJumpAnimation); } }
    public bool PlayerIsVaulting { get { return animator.GetBool(AnimationHashUtility.PlayingVaultAnimation); } }
    private bool PlayerIsRunning { get { return AnimatorVertical > 1.1f || AnimatorVertical < -1.1f || AnimatorHorizontal > 1.1f || AnimatorHorizontal < -1.1f; } }
    private readonly Vector3 FootRayStartOffset = new(0, 1, 0);
    private readonly float raycastDistance = 2;
    //Step void called at animation event on animations and the "left" paramter indicates if it is the left
    // or right foot touching the ground to spawn correct particles if needed on the correct foot

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (!leftFootTransform)
        {
            leftFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform;
        }
        if (!rightFootTransform)
        {
            rightFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform;
        }
    }

    private void Update()
    {
        if (DebugRays)
        {
            Debug.DrawRay(leftFootTransform.position + FootRayStartOffset, Vector3.down, Color.magenta);
            Debug.DrawRay(rightFootTransform.position + FootRayStartOffset, Vector3.down,Color.magenta);
        }
    }
    public void JumpSound()
    {
        string tag = hitMaterial.transform != null ? hitMaterial.transform.tag : "Untagged";
            foreach (FeetSoundStructure feetSound in Sounds)
            {
                if (feetSound.TagMaterial == tag)
                {
                    PlayFootstepSound(RandomAudioClip(feetSound.Jumping), PlayerIsRunning ? feetSound.RunningVolume : feetSound.WalkingVolume , rightFootTransform.position);
                    if (feetSound.ParticlePrefab != null)
                    {
                        InstantiateParticle(leftFootTransform.position,feetSound.ParticlePrefab, Quaternion.identity);
                        InstantiateParticle(rightFootTransform.position,feetSound.ParticlePrefab, Quaternion.identity);
                        break;
                    }
                }
            }
    }

    public void LandSound()
    {
        if (PlayerIsOnGround)
        {
            string tag = hitMaterial.transform != null ? hitMaterial.transform.tag : "Untagged";
            foreach (FeetSoundStructure feetSound in Sounds)
            {
                if (feetSound.TagMaterial == tag)
                {
                    PlayFootstepSound(RandomAudioClip(PlayerIsRunning ? feetSound.Running : feetSound.Walking), PlayerIsRunning ? feetSound.RunningVolume : feetSound.WalkingVolume, leftFootTransform.position);
                    if (feetSound.ParticlePrefab != null)
                    {
                        InstantiateParticle(leftFootTransform.position,feetSound.ParticlePrefab, Quaternion.identity);
                        InstantiateParticle(rightFootTransform.position,feetSound.ParticlePrefab, Quaternion.identity);
                        break;
                    }
                }
            }
        }
    }
    public void Step(int left)
    {
        if (!PlayerIsJumping  && (Time.time - timeSinceLastStepCall) > minimumTimeBetweenSteps)
        {
            if (PlayerIsIdle && !PlayerIsVaulting)
            {
                return;
            }

            switch (DetectionStyle)
            {
                case DetectionStyle.DirectlyBelow:
                    if (PlayerIsOnGround)
                    {
                        Footstep(hitMaterial.transform != null ? hitMaterial.transform.tag : null, left == 1);
                    }
                    break;
                case DetectionStyle.PerFeetPosition:
                    if(left == 1)
                    {
                        Physics.Raycast(leftFootTransform.position + FootRayStartOffset, Vector3.down, out leftFootHitMaterial, raycastDistance, FallSettings.GroundLayers, TriggerInteraction);
                        Footstep(leftFootHitMaterial.transform != null ? leftFootHitMaterial.transform.tag : null, left ==1);
                    }
                    else
                    {
                        Physics.Raycast(rightFootTransform.position + FootRayStartOffset, Vector3.down, out rightFootHitMaterial, raycastDistance, FallSettings.GroundLayers, TriggerInteraction);
                        Footstep(rightFootHitMaterial.transform != null ? rightFootHitMaterial.transform.tag : null, left ==1 );
                    }
                    break;
                default:
                    break;
            }
        }

        timeSinceLastStepCall = Time.time;
    }
    private void PlayFootstepSound(AudioClip sound, float volume, Vector3 position)
    {
        if (RandomPich) // Modulate audio
        {
            AudioSource.pitch = Random.Range(MinPitch, MaxPitch);
        }

        AudioSource.volume = volume;
        AudioSource.PlayClipAtPoint(sound, position);
    }

    private void Footstep(string tag,bool left)
    {
        foreach (FeetSoundStructure feetSound in Sounds)
        {
            if(feetSound.TagMaterial == tag)
            {
                PlayFootstepSound(RandomAudioClip(PlayerIsRunning ? feetSound.Running : feetSound.Walking), PlayerIsRunning ? feetSound.RunningVolume : feetSound.WalkingVolume,left ? leftFootTransform.position : rightFootTransform.position);
                if(feetSound.ParticlePrefab != null)
                {
                    InstantiateParticle(left ? leftFootTransform.position : rightFootTransform.position, feetSound.ParticlePrefab, Quaternion.identity);
                    break;
                }
            }
        }
    }

    private AudioClip RandomAudioClip(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Count)];
    }

    private void InstantiateParticle(Vector3 position, GameObject particle, Quaternion rotation)
    {
        GameObject temp = Instantiate(particle, position, rotation);
        Destroy(temp, DestroyParticlesAfter);
    }
}

[System.Serializable]
public class FeetSoundStructure
{
    public string TagMaterial;

    public List<AudioClip> Landing;
    [Range(0, 5)]
    public float LandingVolume = 0.5f;

    public List<AudioClip> Jumping;
    [Range(0,5)]
    public float JumpingVolume = 0.5f;

    public List<AudioClip> Walking;
    [Range(0, 5)]
    public float WalkingVolume = 0.5f;

    public List<AudioClip> Running;
    [Range(0, 5)]
    public float RunningVolume = 0.5f;

    public GameObject ParticlePrefab;

}