using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class RevolvingDoor : MonoBehaviour
{
    [Header("Target Handler References")]
    [SerializeField] private GameObject targetHandler;
    private TargetHandler targetHandlerScript;
    
    [Header("Open/Close Speeds")]
    [SerializeField] private float openRotateSpeed = 25f;
    [SerializeField] private float closeRotateSpeed = 12.5f;

    private AudioSource doorAudio;
    
    //Used for rotating at a constant rate
    private Quaternion closedRotation; 
    private Quaternion openRotation;
    
    private void Awake()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.AngleAxis(90f, Vector3.up) * closedRotation; //set the target rotation
        targetHandlerScript = targetHandler.GetComponent<TargetHandler>();
        doorAudio = GetComponent<AudioSource>();
    }
    
    public void Rotate()
    {
        StartCoroutine(PlaySounds());
        StartCoroutine(RotateDoor());
    }

    private IEnumerator RotateDoor()
    {
        //Rotate towards the open rotation value 
        while (Quaternion.Angle(transform.rotation, openRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                openRotation,
                openRotateSpeed * Time.deltaTime
                );

            yield return null;
        }

        transform.rotation = openRotation; //Current rotation is now open rotation
        
        //Rotate towards the closed rotation or starting rotation
        while (Quaternion.Angle(transform.rotation, closedRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                closedRotation,
                closeRotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = closedRotation; //Set rotation back to the start rotation

        targetHandlerScript.ResetTargets(); //Reset the torches and their associated values
    }

    //Play the door sounds
    IEnumerator PlaySounds()
    {
        doorAudio.pitch = 1f;
        doorAudio.Play();

        yield return new WaitForSeconds(doorAudio.clip.length / doorAudio.pitch); //Wait until first audio plays

        doorAudio.pitch = 0.5f; //Play the same audio clip at a lower pitch since it is closing at half speed
        doorAudio.Play(); 
    }
}
