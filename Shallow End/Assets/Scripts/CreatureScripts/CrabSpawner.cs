using UnityEngine;
using System.Collections;

public class CrabSpawner : MonoBehaviour
{
    [Header("Crab")]
    public GameObject crabPrefab;

    [Header("Spawning")]
    public int maxCrabs = 10;
    public float spawnRadius = 20f;

    [Header("Timing")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Ground")]
    public float raycastHeight = 20f;
    public float groundOffset = 0f;
    public LayerMask groundLayer;

    [Header("Collision")]
    public float collisionCheckRadius = 0.5f;
    public LayerMask obstacleLayer;

    IEnumerator Start()
    {
        while (true)
        {
            // Count how many crabs currently exist
            int currentCrabs = GameObject.FindGameObjectsWithTag("Crab").Length;

            // Only spawn if we have fewer than the maximum
            if (currentCrabs < maxCrabs)
            {
                float waitTime = Random.Range(
                    minSpawnTime,
                    maxSpawnTime
                );

                yield return new WaitForSeconds(waitTime);

                // Check again after waiting
                currentCrabs = GameObject.FindGameObjectsWithTag("Crab").Length;

                if (currentCrabs < maxCrabs)
                {
                    SpawnCrab();
                }
            }
            else
            {
                // Check again shortly
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void SpawnCrab()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomCircle =
                Random.insideUnitCircle * spawnRadius;

            Vector3 rayStart = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y + raycastHeight,
                transform.position.z + randomCircle.y
            );

            RaycastHit groundHit;

            if (!Physics.Raycast(
                rayStart,
                Vector3.down,
                out groundHit,
                raycastHeight * 2f,
                groundLayer))
            {
                continue;
            }

            Vector3 spawnPosition =
                groundHit.point + Vector3.up * groundOffset;

            if (Physics.CheckSphere(
                spawnPosition,
                collisionCheckRadius,
                obstacleLayer))
            {
                continue;
            }

            Instantiate(
                crabPrefab,
                spawnPosition,
                Quaternion.identity
            );

            return;
        }

        Debug.LogWarning("Can't find an open place to spawn crab.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            spawnRadius
        );
    }
}