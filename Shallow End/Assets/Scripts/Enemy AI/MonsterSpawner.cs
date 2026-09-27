using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{

    [SerializeField] private GameObject monsterPrefab;


    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private float firstSpawnTime = 21f;
    [SerializeField] private float lastSpawnTime = 5f;

    [SerializeField] private int startingMonsterCount = 2;

    [SerializeField] private int monsterIncreasePerNight = 1;

    [SerializeField] private float minimumSpawnDelay = 120f;
    [SerializeField] private float maximumSpawnDelay = 180f;

    private float nextSpawnTimer;

    private bool wasNight = false;

    private int currentNight = 0;
    private int monstersToSpawn = 0;
    private int monstersSpawned = 0;

    private List<GameObject> spawnedMonsters =
        new List<GameObject>();

    private void Start()
    {
        nextSpawnTimer = 0f;


        if (TimeController.instance != null)
        {
            System.DateTime currentTime =
                TimeController.instance.CurrentTime;

            float hour =
                currentTime.Hour +
                currentTime.Minute / 60f +
                currentTime.Second / 3600f;

            bool isNight =
                hour >= firstSpawnTime ||
                hour < lastSpawnTime;

            if (isNight)
            {
                StartNight();
                wasNight = true;
            }
        }
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        System.DateTime currentTime =
            TimeController.instance.CurrentTime;

        float hour =
            currentTime.Hour +
            currentTime.Minute / 60f +
            currentTime.Second / 3600f;

        bool isNight =
            hour >= firstSpawnTime ||
            hour < lastSpawnTime;

     
        if (isNight && !wasNight)
        {
            StartNight();
        }

    
        if (!isNight && wasNight)
        {
            EndNight();
        }

        wasNight = isNight;

        if (!isNight)
            return;

        HandleSpawning();
    }

    private void StartNight()
    {
        currentNight++;

        monstersToSpawn =
            startingMonsterCount +
            ((currentNight - 1) * monsterIncreasePerNight);

        monstersSpawned = 0;

        nextSpawnTimer = 0f;

    }

    private void EndNight()
    {
        nextSpawnTimer = 0f;

        RemoveNightMonsters();

    }

    private void HandleSpawning()
    {
      
        if (monstersSpawned >= monstersToSpawn)
            return;

        if (nextSpawnTimer > 0f)
        {
            nextSpawnTimer -= Time.deltaTime;
            return;
        }

        SpawnMonster();

        monstersSpawned++;


        if (monstersSpawned < monstersToSpawn)
        {
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            nextSpawnTimer = delay;
        }
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null)
        {
         

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
          

            return;
        }

        Transform spawnPoint =
            spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

        GameObject newMonster =
            Instantiate(
                monsterPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        spawnedMonsters.Add(newMonster);


    }

    private void RemoveNightMonsters()
    {
        for (int i = spawnedMonsters.Count - 1; i >= 0; i--)
        {
            if (spawnedMonsters[i] != null)
            {
                Destroy(spawnedMonsters[i]);
            }
        }

        spawnedMonsters.Clear();
    }
}