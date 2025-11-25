using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterNavigation : MonoBehaviour
{
    public NavMeshAgent monsterAgent;

    //Array of empty game objects that are points for the agent to walk to
    public Transform[] wanderPoints;

    private int currentIndex = -1;
    // Update is called once per frame
    void Update()
    {
        WanderAround();
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
        monsterAgent.SetDestination(wanderPoints[currentIndex].position);
    }
}
