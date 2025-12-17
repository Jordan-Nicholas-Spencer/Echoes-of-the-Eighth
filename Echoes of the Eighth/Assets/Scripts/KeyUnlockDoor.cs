using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyUnlockDoor : MonoBehaviour {
    public KeyCollector matchingKey;
    public DoorOpen matchingDoor;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { 
            if (matchingKey.collectedKey)  {
                matchingDoor.puzzleSolved = true;
            }              
                
        }
    }
}
