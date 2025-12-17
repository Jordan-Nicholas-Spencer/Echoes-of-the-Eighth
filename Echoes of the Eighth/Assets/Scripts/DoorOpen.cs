using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour {

    // used on "Barred Arch" prefab, add script to it, and in the inspector set to its child object "Bars"
    [Header("Day/Time To Open At")]
    public int unlockDay = 1;          // door opens on this day
    public float openHour = 6f;        // time of day to open at, 6 AM
    public float closeHour = 17f;      // time of day to close at, 5pm

    public Transform doorPart;         // the child object that will move up (Bars)
    private float openOffsetY = 3f;     // move Y distance
    private float openSpeed = 2f;       // move speed

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

        // true if day X and after 6am and 5pm
        bool timeWindowOpen =
            currentDay >= unlockDay &&
            currentHour >= openHour &&
            currentHour < closeHour;
        // true if puzzle solved
        bool shouldBeOpen = puzzleSolved || timeWindowOpen;

        // open if puzzle solved or during day/time window
        Vector3 targetPos = shouldBeOpen ? openPosition : closedPosition;
        if (Vector3.Distance(doorPart.position, targetPos) > 0.01f)
        {
            doorPart.position = Vector3.MoveTowards(
                doorPart.position,
                targetPos,
                openSpeed * Time.deltaTime
            );
        }
    }
}
