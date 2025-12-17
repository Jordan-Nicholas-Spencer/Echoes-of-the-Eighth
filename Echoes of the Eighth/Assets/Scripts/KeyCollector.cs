using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollector : MonoBehaviour {
    public DoorOpen EightDoor;                  //Eight door that will open
    public AudioClip pickupSFX;
    public AudioSource source;

    [HideInInspector]
    public bool collectedKey = false;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { 
            source.PlayOneShot(pickupSFX);
            collectedKey = true; 
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }
    }
}
