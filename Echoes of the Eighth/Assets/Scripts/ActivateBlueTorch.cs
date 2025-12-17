using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//Used by the blue torch prefab to activate its components such as the light and particles
public class ActivateBlueTorch : MonoBehaviour
{
    [Header("Torch Visual Components")]
    private Light pointLight;
    private ParticleSystem particles;
    
    [Header("Target Handler References")]
    [SerializeField] private GameObject targetHandler;
    private bool targetEnabled = false;
    private TargetHandler targetHandlerScript;

    [Header("Audio")]
    private AudioSource torchAudio;
    
    //Assign components on awake
    private void Awake()
    {
        targetHandlerScript = targetHandler.GetComponent<TargetHandler>();
        torchAudio = GetComponent<AudioSource>();
    }

    public void ToggleLight()
    {
        //Find the pointlight and particles components in the prefab's children objects
        pointLight = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>();
        
        torchAudio.Play();
        
        //Toggle the point light and particles on/off
        var pointLightEnabled = pointLight.enabled;
        pointLightEnabled = !pointLightEnabled;
        pointLight.enabled = pointLightEnabled;
        targetEnabled = pointLightEnabled;
        
        //Increment the number of torches enabled for the target handler script
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

    //Force the current torch's components off
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
