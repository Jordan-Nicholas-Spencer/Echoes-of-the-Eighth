using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ActivateBlueTorch : MonoBehaviour
{
    private Light pointLight;
    private ParticleSystem particles;
    [SerializeField] private GameObject targetHandler;
    private bool targetEnabled = false;
    private TargetHandler targetHandlerScript;

    private AudioSource torchAudio;
    
    
    private void Awake()
    {
        targetHandlerScript = targetHandler.GetComponent<TargetHandler>();
        torchAudio = GetComponent<AudioSource>();
    }

    public void ToggleLight()
    {
        print("hit");
        print("ToggleLight called on " + gameObject.name);
        pointLight = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>();
        
        torchAudio.Play();
        
        var pointLightEnabled = pointLight.enabled;
        pointLightEnabled = !pointLightEnabled;
        pointLight.enabled = pointLightEnabled;
        targetEnabled = pointLightEnabled;
        
        if (targetEnabled)
        {
            particles.Play();
            targetHandlerScript.IncrementTargetsEnabled();
        }
        else
        {
            particles.Stop();
            targetHandlerScript.DecrementTargetsEnabled();
        }
    }

    public void ForceOff()
    {
        pointLight = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>();

        pointLight.enabled = false;
        if (pointLight.enabled == false)
        {
            print("disabled");
        }
        else
        {
            print("enabled");
        }
        particles.Stop();

        targetEnabled = false;
    }
}
