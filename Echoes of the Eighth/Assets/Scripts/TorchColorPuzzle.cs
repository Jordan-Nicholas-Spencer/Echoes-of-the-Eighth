using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchColorPuzzle : MonoBehaviour
{
    public TorchColorCycle[] torches;   // assign 3 torches in proper order

    [Header("Correct Combination")]
    public int[] solution = new int[3];      // e.g. [2, 0, 3]

    [Header("Door to Open")]
    public DoorOpen doorToOpen;

    public void RegisterTorch(TorchColorCycle t)
    {
        // Optional: You can validate torches here
    }

    public void CheckCombination()
    {
        for (int i = 0; i < torches.Length; i++)
        {
            if (torches[i].CurrentColorIndex != solution[i])
                return; // wrong combo
        }

        // All matched!
        Debug.Log("Torch Color Puzzle Solved!");
        doorToOpen.puzzleSolved = true;
    }
}
