using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Displays the current game time on the screen as a digital clock.
 * It automatically finds the DayNightCycle object and displays its time.
 */
public class Clock : MonoBehaviour
{
    [Header("References")]
    // DayNightSystem.
    public DayNightCycle dayNightCycle;

    // Text Component.
    public TMPro.TextMeshProUGUI timeText;

        // Start is called before the first frame update
    void Start()
    {
        // If the DayNightCycle wasn't assigend in the Inspector
        if (dayNightCycle == null)
        {
            // Finds the object using the Instance on the DayNightCycle script.
            dayNightCycle = DayNightCycle.Instance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get current time.
        float currentTime = dayNightCycle.GetCurrentTime();

        // Extract hours.
        int hours = Mathf.FloorToInt(currentTime);

        // Extract minutes.
        int minutes = Mathf.FloorToInt((currentTime % 1) * 60);

        // AM or PM.
        string period;
        if (hours >= 12)
        {
            period = "PM";
        }
        else
        {
            period = "AM";
        }

        // Convert 24-hour to 12-hour format.
        int displayHours = hours % 12;

        // Special case for midnight and noon.
        if (displayHours == 0)
        {
            displayHours = 12;
        }

        int currentDay = dayNightCycle.GetCurrentDay();

        // Create the time string.
        string time = $"Day {currentDay} {displayHours}:{minutes:00} {period}";

        // Display time on the screen.
        UpdateTime(time);        
    }

    // Update time function.
    void UpdateTime(string time)
    {
        if (timeText != null)
        {
            timeText.text = time;
        }
    }
}
