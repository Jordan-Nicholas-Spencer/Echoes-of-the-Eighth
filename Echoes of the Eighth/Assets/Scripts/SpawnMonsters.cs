using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;

public class SpawnMonsters : MonoBehaviour
{
    [SerializeField] private GameObject[] monsters;
    [SerializeField] private Vector3[] startPositions;
    [SerializeField] private GameObject dayNightSystem;
    private DayNightCycle dayNightCycleScript;
    private bool spawnTriggered = false;
    private bool isNight;

    private int posIndex = 0;

    private void Start()
    {
        dayNightCycleScript = dayNightSystem.GetComponent<DayNightCycle>();
    }

    private void Update()
    {
        float time = dayNightCycleScript.GetCurrentTime();
        isNight = (time >= 19 || time < 5);
        
        if (isNight && !spawnTriggered)
        {
            SpawnDespawn(true);
            spawnTriggered = true;
        }

        if (!isNight && spawnTriggered)
        {
            SpawnDespawn(false);
            spawnTriggered = false;
        }
    }
    
    
    
    public void SpawnDespawn(bool spawn)
    {
        foreach (var monster in monsters)
        {
            if (monster != null && spawn)
            {
                print("true");
                monster.SetActive(true);
            }
            else if (monster != null && !spawn)
            {
                print("false");
                monster.SetActive(false);
                ResetPositions();
            }
        }
    }

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
