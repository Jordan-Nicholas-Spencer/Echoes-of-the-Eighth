using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Store_Imports.Ilumisoft.Health_System.Scripts;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterNavigation : MonoBehaviour
{
    [Header("Navigation Components")]
    //Array of empty game objects that are points for the agent to walk to
    public Transform[] wanderPoints;
    public NavMeshAgent monsterAgent; 
    public Transform playerTransform; //Used to track distance to player

    [Header("Agent Values")]
    public float distanceToPlayer = 100f;
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float runMaxSpeed = 4.5f;
    [SerializeField] private float walkMaxSpeed = 2.5f;
    [SerializeField] private float attackDistance = 3f;
    
    private int currentIndex = -1; //used for random wandering

    [Header("Monster/Player Health")]
    private Health monsterHealthComponent;
    [SerializeField] private GameObject playerHealthBar;

    private void Start()
    {
        monsterHealthComponent = GetComponent<Health>();
    }
    
    void Update()
    {
        //Check the distance from the player to the monster
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, playerTransform.transform.position);
        if ((distanceToPlayer < detectionRadius) && monsterHealthComponent.IsAlive)
        {
            //Go towards the player if within the detection radius
            playerHealthBar.SetActive(true);
            TargetPlayer();
        }
        //Otherwise, wander randomly
        else if (monsterHealthComponent.IsAlive)
        {
            WanderAround();
        }
        else
        {
            //Hide the player health bar when the monster is dead or too far away
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
