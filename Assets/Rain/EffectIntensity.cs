using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIntensity : MonoBehaviour
{
    public PlayerTriggerDetector detector;
    public bool applyEffect;

    public bool fade = true;
    [Range(0.0f, 1.0f)]
    public float fadeSpeed = 1;

    public ParticleSystem waterDrop1;
    public ParticleSystem waterDrop2;
    public ParticleSystem waterDrop3;
    public ParticleSystem Trail;

    private ParticleSystem.MainModule waterDrop1Main;
    private ParticleSystem.MainModule waterDrop2Main;
    private ParticleSystem.MainModule waterDrop3Main;
    private ParticleSystem.MainModule trailMain;
    [SerializeField]
    private float waterDrop1LT;
    [SerializeField]
    private float waterDrop2LT;
    [SerializeField]
    private float waterDrop3LT;
    [SerializeField]
    private float trailLT;
    private void Awake()
    {
        trailMain = Trail.main;
        waterDrop1Main = waterDrop1.main;
        waterDrop2Main = waterDrop2.main;
        waterDrop3Main = waterDrop3.main;

        ResetLifetimes();
        ToggleRain(applyEffect);
    }

    private void LateUpdate()
    {
        if (applyEffect)
        {
            if (detector.insideBuilding)
            {
                if (fade)
                {
                    FadeOutRain();
                }
                else
                {
                    ToggleRain(false);
                }
            }
            else
            {
                if(fade)
                {
                    FadeInRain();
                }
                else
                {
                    ToggleRain(true);
                }
            }
        }
        else
        {
            if (fade)
            {
                FadeOutRain();
            }
            else
            {
                ToggleRain(false);
            }
        }
    }


    private void ToggleRain(bool value)
    {
        waterDrop1.gameObject.SetActive(value);
        waterDrop2.gameObject.SetActive(value);
        waterDrop3.gameObject.SetActive(value);
        Trail.gameObject.SetActive(value);
        ResetLifetimes();
    }

    private void ResetLifetimes()
    {
        trailMain.startLifetime = trailLT;
        waterDrop1Main.startLifetime = waterDrop1LT;
        waterDrop2Main.startLifetime = waterDrop2LT;
        waterDrop3Main.startLifetime = waterDrop3LT;
    }

    private void FadeOutRain()
    {
        trailMain.startLifetime = Mathf.Lerp(trailMain.startLifetime.constant, 0, fadeSpeed * Time.deltaTime);
        waterDrop1Main.startLifetime = Mathf.Lerp(waterDrop1Main.startLifetime.constant, 0, fadeSpeed * Time.deltaTime);
        waterDrop2Main.startLifetime = Mathf.Lerp(waterDrop2Main.startLifetime.constant, 0, fadeSpeed * Time.deltaTime);
        waterDrop3Main.startLifetime = Mathf.Lerp(waterDrop3Main.startLifetime.constant, 0, fadeSpeed * Time.deltaTime);

        if (trailMain.startLifetime.constant <= 0.1f)
        {
            ToggleRain(false);
        }
    }

    private void FadeInRain()
    {
        trailMain.startLifetime = Mathf.Lerp(trailMain.startLifetime.constant,trailLT,  fadeSpeed * 20 * Time.deltaTime);
        waterDrop1Main.startLifetime = Mathf.Lerp(waterDrop1Main.startLifetime.constant, waterDrop1LT, fadeSpeed * 20 * Time.deltaTime);
        waterDrop2Main.startLifetime = Mathf.Lerp(waterDrop2Main.startLifetime.constant, waterDrop2LT, fadeSpeed * 20 * Time.deltaTime);
        waterDrop3Main.startLifetime = Mathf.Lerp(waterDrop3Main.startLifetime.constant, waterDrop3LT, fadeSpeed * 20 * Time.deltaTime);
    }

}
