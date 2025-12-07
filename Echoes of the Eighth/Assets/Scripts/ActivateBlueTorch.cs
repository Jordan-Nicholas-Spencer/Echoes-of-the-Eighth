using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBlueTorch : MonoBehaviour
{
    private Light pointLight;
    private ParticleSystem particles;
    [SerializeField] private GameObject targetHandler;
    private bool targetEnabled = false;
    private TargetHandler targetHandlerScript;
    
    //Toggle the particles and point light on/off
    private void Awake()
    {
        targetHandlerScript = targetHandler.GetComponent<TargetHandler>();
    }

    public void ToggleLight()
    {
        pointLight = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>();

        var pointLightEnabled = pointLight.enabled;
        pointLightEnabled = !pointLightEnabled;
        pointLight.enabled = pointLightEnabled;
        targetEnabled = pointLightEnabled;
        
        if (targetEnabled)
        {
            targetHandlerScript.IncrementTargetsEnabled();
        }
        else
        {
            targetHandlerScript.DecrementTargetsEnabled();
        }
        
        if (particles.isPlaying)
        {
            particles.Stop();
        }
        else
        {
            particles.Play();
        }
    }
}
