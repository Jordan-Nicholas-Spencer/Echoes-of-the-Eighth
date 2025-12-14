using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls time and rotates the sun and moon to create a day and night transition.
 */

public class DayNightCycle : MonoBehaviour
{
    /* 
     * TIME SETTINGS.
     */

    [Header("Time Settings")]

    // What time the day starts on a 24-Hour format.
    [Range(0, 24)]
    public float startTime = 6f;        // The day starts at 6:00 AM.

    // How many real minutes is equal 1 full day in game.
    public float dayDuration = 15f;     // One day is equal 15 real time minutes.

    /*
     * SUN SETTINGS.
     */

    [Header("Sun Lightings")]

    // Object for the Sun Directional Light.
    public Light sunLight;

    // Color gradient for the sun at diferent times of the day.
    public Gradient sunColorGradient;

    // Sun brightness at different times of the day.
    public AnimationCurve sunBrightnessCurve;

    /*
     * MOON SETTINGS.
     */

    [Header("Moon Lightings")]

    // Object for the Moon Directional Light.
    public Light moonLight;

    // Color gradient for the moon at diferent times of the night.
    public Gradient moonColorGradient;

    // Moon brightness at different times of the night.
    public AnimationCurve moonBrightnessCurve;

    /*
     * OTHER VARIABLES.
     */
    public static DayNightCycle Instance { get; private set; }      // Allows other scripts to access this.
    private float currentTime;                                      // Current time in game hours. Eg: 14.5 = 2:30;  

    public int CurrentDay { get; private set; } = 1;                // Start at day 1
    private float previousTime;                                     // Track when its a new day                        
    
    /*
     * Converts real time seconds into game hours, because deltaTime is given in seconds.
     * Example calculation:
     *  - 15 real minutes = 900 real seconds = 24 game hours.
     *  - 1 real seconds = 24/900 = 0.0267 game hours.
     */
    private float timeMultiplier;

    private void Awake()
    {
        // Ensure there is only one DayNightCycle.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set starting time.
        currentTime = startTime;

        // Calculate the time multipler.
        timeMultiplier = 24f / (dayDuration * 60f);

        // Update lighting to match start time.
        UpdateLighting();

        previousTime = currentTime;   // remember initial time
    }

    // Update is called once per frame
    void Update()
    {
        // Move time forward.
        currentTime += Time.deltaTime * timeMultiplier;

        // Reset clock after 24 hours.
        if (currentTime >= 24f)
        {
            currentTime = 0f;
        }

        //detect when its a new day
        if (currentTime < previousTime) {
        CurrentDay++;
        }
        previousTime = currentTime;

        // Update the sun and moon directional lighting.
        UpdateLighting();
    }

    /*
     * UPDATE LIGHTING
     * Rotates and adjust color for the sun and moon.
     */
    void UpdateLighting()
    {
        // Converts time to a percentage used to set the color gradients.
        float timePercent = currentTime / 24f;

        // UPDATE SUN.
        if (sunLight != null)
        {
            /* Calculate the angle of the sun.
             *  - timePercent * 360f, one full rotation over 24 hours.
             *  - Substract 90f so that 0� is at the horizon.
             * Examples:
             *  - At sunrise (6:00AM) -> (timePercent = 0.25) -> 0.25 * 360 - 90 = 0� (Horizon)
             *  - At noon (12:00PM) -> (timePercent = 0.5) -> 0.5 * 360 - 90 = 90� (Overhead)
             *  - At sunset (6:00PM) -> (timePercent = 0.75) -> 0.75 * 360 - 90 = 180� (Horizon)
             */
            float sunAngle = timePercent * 360f - 90f;

            /* Rotate the sun light.
             * X-Axis (sunAngle): Controls sunrise and sunset movement.
             * Y-Axis (90f): Controls direction of travel east to west.
             * Z-Axis (0f): No roll.
             */
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 90f, 0f);

            // Change the sun color depending on the time of day.
            sunLight.color = sunColorGradient.Evaluate(timePercent);

            // Change the sun brightness depending on the time of the day.
            sunLight.intensity = sunBrightnessCurve.Evaluate(timePercent);
        }

        // UPDATE MOON.
        if (moonLight != null)
        {
            /* Calculate the angle of the sun.
             * Moon is opposite to the sun, so we add 90� offset.
             */
            float moonAngle = timePercent * 360f + 90f;

            /* Rotate the moon light.
             * X-Axis (moonAngle): Controls sunrise and sunset movement.
             * Y-Axis (90f): Controls direction of travel east to west.
             * Z-Axis (0f): No roll.
             */
            moonLight.transform.rotation = Quaternion.Euler(moonAngle, 90f, 0f);

            // Change the moon color depending on the time of day.
            moonLight.color = moonColorGradient.Evaluate(timePercent);

            // Change the moon brightness depending on the time of the day.
            moonLight.intensity = moonBrightnessCurve.Evaluate(timePercent);
        }
    }

    /* 
     * PUBLIC METHODS.
     */
    public float GetCurrentTime()
    {
        return currentTime;             
    }

    public int GetCurrentDay() {
    return CurrentDay;
    }
}
