using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBlueTorch : MonoBehaviour
{
    private Light pointLight;
    private ParticleSystem particles;

    private void Update()
    {
        //Test for toggle behavior
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleLight();
        }
    }
    
    //Toggle the particles and point light on/off
    void ToggleLight()
    {
        pointLight = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>();

        pointLight.enabled = !pointLight.enabled;
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
