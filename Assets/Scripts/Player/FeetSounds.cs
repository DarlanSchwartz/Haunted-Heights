using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetSounds : MonoBehaviour
{
    public GameObject weSurfaceParticle;
    public Transform leftFootpos;
    public Transform rightFootpos;
    public List<AudioClip> StepSounds;
    public float DefaultVolume = 0.11f;
    public List<AudioClip> Step2Sounds;
    public List<AudioClip> WetSurfaceSounds;
    public float WetSurfaceDefaultVolume = 0.34f;
    public List<AudioClip> WaterDeepSounds;
    public float Inside_WoodDefaultVolume = 0.11f;
    public List<AudioClip> Inside_WoodSounds;
    public float GrassDefaultVolume = 0.33f;
    public List<AudioClip> GrassSounds;
    public AudioSource AudioSource;
    public bool RandomPich = false;
    public float MaxPitch = 1;
    public float MinPitch = 0;
    public Animator Animator { get; set; }
    private float timeSinceLastStep;
    [Range(0, 1)]
    public float minimumTimeBetweenSteps = 0.15f;
    public FallSettings FallSettings;
    private RaycastHit hitMaterial;

    //Step void called at animation event on animations and the "left" paramter indicates if it is the left
    // or right foot touching the ground to spawn correct particles if needed on the correct foot
    public void Step(int left)
    {
        // if i am idle and this is called then do nothing
        if (Animator.GetBool(AnimationHashUtility.Idle))
        {
            return;
        }

        if ((Time.time - timeSinceLastStep) > minimumTimeBetweenSteps)
        {
            // Modulate audio
            if (RandomPich)
            {
                AudioSource.pitch = Random.Range(MinPitch, MaxPitch);
            }

            //Check wich material am i stepping on and set the correct sound effect
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitMaterial, 1.5f, FallSettings.GroundLayers))
            {
                if(hitMaterial.transform.CompareTag("Untagged"))
                {
                    AudioSource.volume = DefaultVolume;
                    AudioSource.PlayOneShot(Step2Sounds[Random.Range(0, StepSounds.Count)]);
                }
                else if (hitMaterial.transform.CompareTag("WaterDeep"))
                {
                    AudioSource.volume = DefaultVolume;
                    AudioSource.PlayOneShot(WaterDeepSounds[Random.Range(0, WaterDeepSounds.Count)]);
                }
                else if (hitMaterial.transform.CompareTag("WetSurface"))
                {
                    AudioSource.volume = WetSurfaceDefaultVolume;
                    AudioSource.PlayOneShot(WetSurfaceSounds[Random.Range(0, WetSurfaceSounds.Count)]);
                    if (left == 1)
                    {
                        Instantiate(weSurfaceParticle, leftFootpos.position, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(weSurfaceParticle, rightFootpos.position, Quaternion.identity);
                    }
                }
                else if (hitMaterial.transform.CompareTag("Inside_Wood"))
                {
                    AudioSource.volume = Inside_WoodDefaultVolume;
                    AudioSource.PlayOneShot(Inside_WoodSounds[Random.Range(0, Inside_WoodSounds.Count)]);
                }
                else if (hitMaterial.transform.CompareTag("Grass"))
                {
                    AudioSource.volume = GrassDefaultVolume;
                    AudioSource.PlayOneShot(GrassSounds[Random.Range(0, GrassSounds.Count)]);
                }
                else if (hitMaterial.transform.CompareTag("Metal"))
                {

                }
                else if (hitMaterial.transform.CompareTag("Dirt"))
                {

                }
                else if (hitMaterial.transform.CompareTag("Outside_Wood"))
                {

                }
                else
                {
                    AudioSource.volume = DefaultVolume;
                    AudioSource.PlayOneShot(Step2Sounds[Random.Range(0, StepSounds.Count)]);
                }
            }
        }
        // this has to be after the Time.time - timeSinceLastStep) > minimumTimeBetweenSteps calculation
        // or this will not work
        timeSinceLastStep = Time.time;
    }
}
