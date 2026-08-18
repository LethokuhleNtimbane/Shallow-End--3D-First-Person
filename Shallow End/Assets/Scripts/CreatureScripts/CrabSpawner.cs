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

    private int currentCrabs = 0;

    void Start()
    {
        StartCoroutine(SpawnCrabs());
    }

    IEnumerator SpawnCrabs()
    {
        while (currentCrabs < maxCrabs)
        {
            float waitTime = Random.Range(
                minSpawnTime,
                maxSpawnTime
            );

            yield return new WaitForSeconds(waitTime);

            SpawnCrab();
        }
    }

    void SpawnCrab()
    {
        
        for (int i = 0; i < 20; i++)// will spawn crabs in mutiple random places
        {
            
            Vector2 randomCircle =
                Random.insideUnitCircle * spawnRadius;// spawns only in radius tho

            Vector3 rayStart = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y + raycastHeight,
                transform.position.z + randomCircle.y
            );

            
            RaycastHit groundHit; // spawns on ground

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
                groundHit.point + Vector3.up * groundOffset; // crab still clips into ground when spawned so this makes 
            // em spawn higher up if we need

            
            if (Physics.CheckSphere(
                spawnPosition,
                collisionCheckRadius,
                obstacleLayer))
            {
                continue; // checks if anything is on the way so crab does not spawn into an object
            }

            
            Instantiate(
                crabPrefab,
                spawnPosition,
                Quaternion.identity // spawn crab
            );

            currentCrabs++;

            return;
        }

        Debug.LogWarning(
            "cant find a open place to spawn crab."
        );
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