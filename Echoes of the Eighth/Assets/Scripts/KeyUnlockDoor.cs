using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyUnlockDoor : MonoBehaviour {
    public KeyCollector matchingKey;
    public DoorOpen matchingDoor;
    public AudioClip openSFX;
    public AudioSource audioSource;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { 
            if (matchingKey.collectedKey)  {
                audioSource.PlayOneShot(openSFX);
                matchingDoor.puzzleSolved = true;
            }                
        }
    }
}
