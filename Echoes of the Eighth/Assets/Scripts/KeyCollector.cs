using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollector : MonoBehaviour {
    public DoorOpen EightDoor;                  //Eight door that will open

    [HideInInspector]
    public bool collectedKey = false;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { 
            Debug.Log("got key");
            collectedKey = true;                 
            Destroy(gameObject);
        }
    }
    
}
