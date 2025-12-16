using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchColorPuzzle : MonoBehaviour {
    public TorchColorCycle[] torches = new TorchColorCycle[3];                // the three torches; 0,1,2

    [Header("Correct Combination")]
    public int[] solution = new int[3];              // combination that unlocks door

    [Header("Door to Open")]                         // door instance that will open(needs to have DoorOpen script attached)
    public DoorOpen doorToOpen;

    public void RegisterTorch(TorchColorCycle t) {
    }

    public void CheckCombination() {
        for (int i = 0; i < torches.Length; i++) {
            if (torches[i].CurrentColorIndex != solution[i])
                return; 
        }

        Debug.Log("Solved");
        doorToOpen.puzzleSolved = true;
    }
}
