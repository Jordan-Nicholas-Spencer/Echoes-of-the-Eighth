using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCollector : MonoBehaviour {
    
    public static int scrollsCollected = 0;     //start at 0 scrolls collected
    public DoorOpen EightDoor;                  //Eight door that will open

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {       //walk over scroll, +1 scroll, destroy it
            scrollsCollected++;                 
            Destroy(gameObject);
            if (scrollsCollected == 7) {        //check if 7 collected, open 8th door if yes
                EightDoor.puzzleSolved = true;
            }
        }
    }
}