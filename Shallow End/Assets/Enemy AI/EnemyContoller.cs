using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyContoller : MonoBehaviour
{
    [Header("Patrol points")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;

    private void Patrol()
    {
        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance <= stopAtDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }
    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(patrolWaitTime);

        agent.isStopped = false;
        goToNext();
        isWaiting = false;
    }

    private NavMeshAgent agent;
    private int currentPatrolInex;
    private bool isWaiting;

    private void goToNext()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolInex].position);
        currentPatrolInex = (currentPatrolInex + 1) % patrolPoints.Length;
    }

    private void Update()
    {
        
    }
}
