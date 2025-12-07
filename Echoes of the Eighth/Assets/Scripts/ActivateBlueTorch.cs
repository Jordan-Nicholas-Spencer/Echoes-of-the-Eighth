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
        particles.Stop();

        targetEnabled = false;
    }
}
