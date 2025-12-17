using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;

public class SpawnMonsters : MonoBehaviour
{
    //To add a new monster to spawn, place a new monster prefab in the scene, attach the wander points in its Monster Navigation
    //script, attach this new monster to the monsters list and attach its starting transform in startPositions.
    [Header("Lists")]
    [SerializeField] private GameObject[] monsters; //Each monster in the scene
    [SerializeField] private Vector3[] startPositions; //Their start positions for respawning
    
    [Header("Script Objects")]
    [SerializeField] private GameObject dayNightSystem;
    private DayNightCycle dayNightCycleScript;
    
    //Variables
    private bool spawnTriggered = false;
    private bool isNight;
    private int posIndex = 0;

    private void Start()
    {
        //Get dayNightCycle script to check time
        dayNightCycleScript = dayNightSystem.GetComponent<DayNightCycle>();
    }

    private void Update()
    {
        float time = dayNightCycleScript.GetCurrentTime();
        isNight = (time >= 19 || time < 5); //time is measured in hours 19 == 7 pm, 5 == 5 am
        
        //Spawn each monster in their respective locations if it is night
        if (isNight && !spawnTriggered)
        {
            SpawnDespawn(true);
            spawnTriggered = true;
        }
        
        //Despawn each monster and reset their positions if it is morning
        if (!isNight && spawnTriggered)
        {
            SpawnDespawn(false);
            spawnTriggered = false;
        }
    }
    
    //Spawn if parameter spawn == true, otherwise if spawn == false, despawn monsters
    public void SpawnDespawn(bool spawn)
    {
        //Cycle through every monster prefab and activate/deactivate them in the scene
        foreach (var monster in monsters)
        {
            if (monster != null && spawn)
            {
                monster.SetActive(true);
            }
            else if (monster != null && !spawn)
            {
                monster.SetActive(false);
                ResetPositions();
            }
        }
    }

    //Cycle through the starting positions and reset each monster
    private void ResetPositions()
    {
        foreach (var monster in monsters)
        {
            monster.transform.position = startPositions[posIndex];
            posIndex++;
        }

        posIndex = 0;
    }
}
