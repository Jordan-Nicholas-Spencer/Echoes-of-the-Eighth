using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
    [SerializeField] private GameObject revolvingDoor;
    [SerializeField] private List<GameObject> torches;
    
    private RevolvingDoor revolvingDoorScript;
    private int targetsEnabled = 0;
    public int maxTargets;

    private void Awake()
    {
        revolvingDoorScript = revolvingDoor.GetComponent<RevolvingDoor>();
    }

    public void IncrementTargetsEnabled()
    {
        targetsEnabled += 1;
        print(targetsEnabled);
        if (targetsEnabled == maxTargets)
        {
            revolvingDoorScript.Rotate();
        }
    }

    public void DecrementTargetsEnabled()
    {
        targetsEnabled -= 1;
        print(targetsEnabled);
    }

    public void ResetTargets()
    {
        targetsEnabled = 0;
        foreach (var torch in torches)
        {
            torch.GetComponent<ActivateBlueTorch>().ForceOff();
        }
    }
}
