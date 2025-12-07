using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
    //[SerializeField] private List<GameObject> targetList;

    [SerializeField] private GameObject revolvingDoor;
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
}
