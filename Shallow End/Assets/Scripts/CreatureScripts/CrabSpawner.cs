using UnityEngine;
using System.Collections;

public class CrabSpawner : MonoBehaviour
{
  
    public GameObject crabPrefab;

    
    public int maxCrabs = 10;
    public float spawnRadius = 20f;

 
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

 
    public float raycastHeight = 20f;
    public float groundOffset = 0f;
    public LayerMask groundLayer;

   
    public float collisionCheckRadius = 0.5f;
    public LayerMask obstacleLayer;

    IEnumerator Start()
    {
        while (true)
        {
        
            int currentCrabs = GameObject.FindGameObjectsWithTag("Crab").Length;

            
            if (currentCrabs < maxCrabs)
            {
                float waitTime = Random.Range(
                    minSpawnTime,
                    maxSpawnTime
                );

                yield return new WaitForSeconds(waitTime);

           
                currentCrabs = GameObject.FindGameObjectsWithTag("Crab").Length;

                if (currentCrabs < maxCrabs)
                {
                    SpawnCrab();
                }
            }
            else
            {
               
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