using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour {

    // used on "Barred Arch" prefab, add script to it, and in the inspector set to its child object "Tunnel_Grate"
    [Header("Day/Time To Open At")]
    public int unlockDay = 2;          // door opens on this day, Day 2
    public float openHour = 6f;        // time of day to open at, 6 AM

    [Header("Movement Settings")]
    public Transform doorPart;         // the child object that will move up (Tunnel_Grate)
    public float openOffsetY = 3f;     // move Y distance
    public float openSpeed = 2f;       // move speed

    private bool isOpening = false;
    private bool hasOpened = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    public bool puzzleSolved = false;

    void Start() {

        if (doorPart == null)
            doorPart = transform;

        closedPosition = doorPart.position;
        openPosition = closedPosition + new Vector3(0f, openOffsetY, 0f);
    }

    void Update() {
        var cycle = DayNightCycle.Instance;

        if (cycle == null) return;

        int currentDay = cycle.GetCurrentDay();          
        float currentHour = cycle.GetCurrentTime(); 

        // check for day and time
        if (!hasOpened && currentDay >= unlockDay && currentHour >= openHour || puzzleSolved) {
            isOpening = true;
        }
        // move object if open
        if (isOpening && !hasOpened) {
            doorPart.position = Vector3.MoveTowards(
                doorPart.position,
                openPosition,
                openSpeed * Time.deltaTime
            );
            if (Vector3.Distance(doorPart.position, openPosition) < 0.01f) {
                doorPart.position = openPosition;
                hasOpened = true;
                isOpening = false;
            }
        }
    }
}
