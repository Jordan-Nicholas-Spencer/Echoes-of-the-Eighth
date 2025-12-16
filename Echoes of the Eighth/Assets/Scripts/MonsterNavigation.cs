using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Store_Imports.Ilumisoft.Health_System.Scripts;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterNavigation : MonoBehaviour
{
    public NavMeshAgent monsterAgent;
    public Transform playerTransform;

    //Array of empty game objects that are points for the agent to walk to
    public Transform[] wanderPoints;

    private int currentIndex = -1;

    public float distanceToPlayer = 100f;

    [SerializeField] private float detectionRadius = 6f;

    [SerializeField] private float runMaxSpeed = 4.5f;

    [SerializeField] private float walkMaxSpeed = 2.5f;

    [SerializeField] private float attackDistance = 3f;

    private AudioSource monsterAudio;

    [SerializeField] private AudioClip detection;

    private Health monsterHealthComponent;
    [SerializeField] private GameObject playerHealthBar;

    private void Start()
    {
        monsterAudio = GetComponent<AudioSource>();
        monsterHealthComponent = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, playerTransform.transform.position);
        if ((distanceToPlayer < detectionRadius) && monsterHealthComponent.IsAlive)
        {
            playerHealthBar.SetActive(true);
            TargetPlayer();
        }
        else if (monsterHealthComponent.IsAlive)
        {
            WanderAround();
        }
        else
        {
            playerHealthBar.SetActive(false);
        }
    }

    //Pick a new wander point if agent has reached its current point
    void WanderAround()
    {
        if (!monsterAgent.pathPending && monsterAgent.remainingDistance <= monsterAgent.stoppingDistance)
        {
            if (!monsterAgent.hasPath || monsterAgent.velocity.sqrMagnitude < 0.1f)
            {
                PickNewPoint();
            }
        }
    }

    //Pick a random point out of the given wander points to direct the agent to
    void PickNewPoint()
    {
        int nextIndex = Random.Range(0, wanderPoints.Length);

        while (nextIndex == currentIndex)
        {
            nextIndex = Random.Range(0, wanderPoints.Length);
        }

        currentIndex = nextIndex;
        monsterAgent.stoppingDistance = 0f;
        monsterAgent.SetDestination(wanderPoints[currentIndex].position);
        monsterAgent.speed = walkMaxSpeed;
    }

    //Tracks the player and moves toward them
    void TargetPlayer()
    {
        monsterAgent.stoppingDistance = attackDistance;
        monsterAgent.SetDestination(playerTransform.position);
        monsterAgent.speed = runMaxSpeed;
    }
}
