using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class RevolvingDoor : MonoBehaviour
{
    [SerializeField] private GameObject targetHandler;
    private TargetHandler targetHandlerScript;
    [SerializeField] private float openRotateSpeed = 25f;
    [SerializeField] private float closeRotateSpeed = 12.5f;

    private AudioSource doorAudio;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    private void Awake()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.AngleAxis(90f, Vector3.up) * closedRotation;
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
        print("rotating");
        while (Quaternion.Angle(transform.rotation, openRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                openRotation,
                openRotateSpeed * Time.deltaTime
                );

            yield return null;
        }

        transform.rotation = openRotation;
        
        while (Quaternion.Angle(transform.rotation, closedRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                closedRotation,
                closeRotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = closedRotation;

        targetHandlerScript.ResetTargets();
    }

    IEnumerator PlaySounds()
    {
        doorAudio.pitch = 1f;
        doorAudio.Play();

        yield return new WaitForSeconds(doorAudio.clip.length / doorAudio.pitch);

        doorAudio.pitch = 0.5f;
        doorAudio.Play();
    }
}
