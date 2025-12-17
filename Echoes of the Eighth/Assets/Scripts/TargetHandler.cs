using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//Used for the target puzzle where the player must hit a specified number of torches before opening a rotating door
public class TargetHandler : MonoBehaviour
{
    [Header("Door and Torches")]
    [SerializeField] private GameObject revolvingDoor; //Reference to the door to rotate
    [SerializeField] private List<GameObject> torches;
    
    private RevolvingDoor revolvingDoorScript;
    private int targetsEnabled = 0;
    public int maxTargets; //Number of targets needed to open the door

    private void Awake()
    {
        revolvingDoorScript = revolvingDoor.GetComponent<RevolvingDoor>();
    }

    //Increment the number of targets enabled and rotate if the desired number has been reached
    public void IncrementTargetsEnabled()
    {
        targetsEnabled += 1;
        if (targetsEnabled == maxTargets)
        {
            revolvingDoorScript.Rotate();
        }
    }

    //Decrement the number of targets enabled
    public void DecrementTargetsEnabled()
    {
        targetsEnabled -= 1;
    }

    //Reset the number of targets enabled for the next run, and deactivate the lit torch components
    public void ResetTargets()
    {
        targetsEnabled = 0;
        foreach (var torch in torches)
        {
            torch.GetComponent<ActivateBlueTorch>().ForceOff();
        }
    }
}
