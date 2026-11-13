using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Rain Weather")]
    [SerializeField] private RainWeatherController rainWeatherController;
    [SerializeField, Min(1)] private int rainMonstersAddedPerStorm = 2;

    [Header("Night Time")]
    [SerializeField] private float firstSpawnTime = 21f;
    [SerializeField] private float lastSpawnTime = 5f;

    [Header("Night Progression")]
    [SerializeField, Min(1)] private int firstMonsterSpawnNight = 2;

    [Header("Monster Count")]
    [SerializeField] private int startingMonsterCount = 1;
    [SerializeField] private int monsterIncreasePerNight = 1;

    [Header("Spawn Delay")]
    [SerializeField] private float minimumSpawnDelay = 120f;
    [SerializeField] private float maximumSpawnDelay = 180f;

    [Header("Portal Effect")]
    [SerializeField] private GameObject portalEffects;

    [Header("Monster Emergence")]
    [SerializeField] private float monsterRiseHeight = 2f;
    [SerializeField] private float monsterRiseDuration = 2f;
    [SerializeField] private float monsterInactiveTime = 1f;

    [Header("Waterfall")]
    [SerializeField] private Renderer waterfall;
    [SerializeField] private Material waterfallAlteredMaterial;

    [Header("Pond")]
    [SerializeField] private Renderer pond;
    [SerializeField] private Material pondAlteredMaterial;

    [Header("Shader Graph Transition")]
    [SerializeField] private string transitionProperty = "_Transition";
    [SerializeField] private float waterFadeDuration = 2f;

    [Header("Objects To Hide")]
    [SerializeField] private GameObject[] objectsToHide;

    private float nextSpawnTimer;

    private bool wasNight = false;
    private bool wasRaining = false;

    private int currentNight = 0;
    private int monstersToSpawn = 0;
    private int monstersSpawned = 0;

    private int currentStormNumber = 0;
    private int rainMonstersToSpawn = 0;
    private int rainMonstersSpawned = 0;

    private List<GameObject> spawnedMonsters =
        new List<GameObject>();

    private List<GameObject> rainSpawnedMonsters =
        new List<GameObject>();

    private Material[] originalWaterfallMaterials;
    private Material[] originalPondMaterials;

    private MaterialPropertyBlock originalWaterfallBlock;
    private MaterialPropertyBlock originalPondBlock;

    private bool[] originalObjectStates;

    private bool waterEffectStarted = false;
    private Coroutine waterFadeCoroutine;

    private int transitionPropertyID;

    public bool HasActiveMonsters
    {
        get
        {
            for (int i =
                 spawnedMonsters.Count - 1;
                 i >= 0;
                 i--)
            {
                if (spawnedMonsters[i] == null)
                {
                    spawnedMonsters.RemoveAt(i);
                    continue;
                }

                if (spawnedMonsters[i].activeInHierarchy)
                    return true;
            }

            return false;
        }
    }

    private void Start()
    {
        nextSpawnTimer = 0f;

        transitionPropertyID =
            Shader.PropertyToID(
                transitionProperty
            );

        if (portalEffects != null)
            portalEffects.SetActive(false);

        if (rainWeatherController == null)
        {
            rainWeatherController =
                FindFirstObjectByType<
                    RainWeatherController
                >();
        }

        if (TimeController.instance != null)
        {
            float hour =
                GetCurrentHour();

            bool isNight =
                hour >= firstSpawnTime ||
                hour < lastSpawnTime;

            if (isNight)
            {
                StartNight();
                wasNight = true;
            }
        }

        if (rainWeatherController != null)
        {
            bool isRaining =
                rainWeatherController.IsRaining;

            if (isRaining)
            {
                StartRain();
                wasRaining = true;
            }
        }
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        float hour =
            GetCurrentHour();

        bool isNight =
            hour >= firstSpawnTime ||
            hour < lastSpawnTime;

        bool isRaining =
            rainWeatherController != null &&
            rainWeatherController.IsRaining;

        if (isNight && !wasNight)
            StartNight();

        if (!isNight && wasNight)
            EndNight();

        if (isRaining && !wasRaining)
            StartRain();

        if (!isRaining && wasRaining)
            EndRain(isNight);

        wasNight = isNight;
        wasRaining = isRaining;

        if (!isNight && !isRaining)
            return;

        HandleSpawning(
            isNight,
            isRaining
        );
    }

    private float GetCurrentHour()
    {
        System.DateTime currentTime =
            TimeController.instance.CurrentTime;

        return currentTime.Hour +
               currentTime.Minute / 60f +
               currentTime.Second / 3600f;
    }

    private int GetCurrentNightNumber()
    {
        System.DateTime currentTime =
            TimeController.instance.CurrentTime;

        int gameDay =
            (
                currentTime.Date -
                new System.DateTime(
                    2000,
                    1,
                    1
                )
            ).Days + 1;

        if (GetCurrentHour() <
            lastSpawnTime)
        {
            gameDay--;
        }

        return Mathf.Max(
            1,
            gameDay
        );
    }

    private void StartNight()
    {
        currentNight =
            GetCurrentNightNumber();

        if (currentNight <
            firstMonsterSpawnNight)
        {
            monstersToSpawn = 0;
        }
        else
        {
            monstersToSpawn =
                startingMonsterCount +
                (
                    currentNight - 1
                ) *
                monsterIncreasePerNight;
        }

        monstersSpawned = 0;

        nextSpawnTimer = 0f;

        waterEffectStarted = false;
    }

    private void EndNight()
    {
        nextSpawnTimer = 0f;

        if (portalEffects != null)
            portalEffects.SetActive(false);

        RemoveNightMonsters();

        bool stormActive =
            rainWeatherController != null &&
            rainWeatherController.IsStormActive;

        if (!stormActive)
        {
            RestoreWaterAndObjects();
        }
    }

    private void StartRain()
    {
        currentStormNumber++;

        rainMonstersToSpawn =
            currentStormNumber *
            rainMonstersAddedPerStorm;

        rainMonstersSpawned = 0;

        nextSpawnTimer = 0f;

        waterEffectStarted = false;

        Debug.Log(
            "[Monster Spawner] Storm " +
            currentStormNumber +
            " started. Rain monsters: " +
            rainMonstersToSpawn
        );
    }

    private void EndRain(bool isNight)
    {
        nextSpawnTimer = 0f;

        rainMonstersSpawned = 0;
        rainMonstersToSpawn = 0;

        if (!isNight)
        {
            RemoveRainMonsters();

            if (portalEffects != null)
                portalEffects.SetActive(false);

            RestoreWaterAndObjects();
        }
    }

    private void HandleSpawning(
        bool isNight,
        bool isRaining)
    {
        bool canSpawnNightMonster =
            isNight &&
            monstersSpawned <
            monstersToSpawn;

        bool canSpawnRainMonster =
            isRaining &&
            rainMonstersSpawned <
            rainMonstersToSpawn;

        if (!canSpawnNightMonster &&
            !canSpawnRainMonster)
        {
            return;
        }

        if (nextSpawnTimer > 0f)
        {
            nextSpawnTimer -=
                Time.deltaTime;

            return;
        }

        bool spawnRainMonster = false;

        if (canSpawnRainMonster &&
            !canSpawnNightMonster)
        {
            spawnRainMonster = true;
        }
        else if (canSpawnNightMonster &&
                 !canSpawnRainMonster)
        {
            spawnRainMonster = false;
        }
        else
        {
            spawnRainMonster =
                rainMonstersSpawned <=
                monstersSpawned;
        }

        SpawnMonster(
            spawnRainMonster
        );

        if (spawnRainMonster)
        {
            rainMonstersSpawned++;
        }
        else
        {
            monstersSpawned++;
        }

        bool moreNightMonsters =
            isNight &&
            monstersSpawned <
            monstersToSpawn;

        bool moreRainMonsters =
            isRaining &&
            rainMonstersSpawned <
            rainMonstersToSpawn;

        if (moreNightMonsters ||
            moreRainMonsters)
        {
            nextSpawnTimer =
                Random.Range(
                    minimumSpawnDelay,
                    maximumSpawnDelay
                );
        }
    }

    private void SpawnMonster(
        bool isRainMonster)
    {
        if (monsterPrefab == null)
            return;

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }

        Transform spawnPoint =
            spawnPoints[
                Random.Range(
                    0,
                    spawnPoints.Length
                )
            ];

        GameObject newMonster =
            Instantiate(
                monsterPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        if (newMonster == null)
            return;

        spawnedMonsters.Add(
            newMonster
        );

        if (isRainMonster)
        {
            rainSpawnedMonsters.Add(
                newMonster
            );
        }

        StartCoroutine(
            SpawnMonsterSequence(
                newMonster,
                spawnPoint
            )
        );
    }

    private IEnumerator SpawnMonsterSequence(
        GameObject monster,
        Transform spawnPoint)
    {
        monster.SetActive(false);

        Vector3 portalPosition =
            spawnPoint.position;

        Vector3 finalPosition =
            portalPosition +
            Vector3.up *
            monsterRiseHeight;

        monster.transform.position =
            portalPosition;

        if (!waterEffectStarted)
        {
            waterEffectStarted = true;

            BeginWaterEffect();
        }

        if (portalEffects != null)
        {
            portalEffects.transform.position =
                portalPosition;

            portalEffects.SetActive(true);
        }

        yield return new WaitForSeconds(
            monsterInactiveTime
        );

        if (monster == null)
            yield break;

        monster.SetActive(true);

        Monster monsterScript =
            monster.GetComponent<Monster>();

        if (monsterScript != null)
            monsterScript.enabled = false;

        float elapsed = 0f;

        Vector3 startPosition =
            portalPosition;

        while (elapsed <
               monsterRiseDuration)
        {
            if (monster == null)
                yield break;

            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    monsterRiseDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            monster.transform.position =
                Vector3.Lerp(
                    startPosition,
                    finalPosition,
                    smoothT
                );

            yield return null;
        }

        if (monster == null)
            yield break;

        monster.transform.position =
            finalPosition;

        if (portalEffects != null)
            portalEffects.SetActive(false);

        if (monsterScript != null)
        {
            monsterScript.SetSpawnerControlled(
                false
            );

            monsterScript.enabled = true;
        }
    }

    private void BeginWaterEffect()
    {
        if (waterfall != null &&
            waterfallAlteredMaterial != null)
        {
            originalWaterfallMaterials =
                waterfall.sharedMaterials;

            originalWaterfallBlock =
                new MaterialPropertyBlock();

            waterfall.GetPropertyBlock(
                originalWaterfallBlock
            );

            waterfall.sharedMaterial =
                waterfallAlteredMaterial;

            SetWaterTransition(
                waterfall,
                0f
            );
        }

        if (pond != null &&
            pondAlteredMaterial != null)
        {
            originalPondMaterials =
                pond.sharedMaterials;

            originalPondBlock =
                new MaterialPropertyBlock();

            pond.GetPropertyBlock(
                originalPondBlock
            );

            pond.sharedMaterial =
                pondAlteredMaterial;

            SetWaterTransition(
                pond,
                0f
            );
        }

        if (objectsToHide != null)
        {
            originalObjectStates =
                new bool[
                    objectsToHide.Length
                ];

            for (int i = 0;
                 i < objectsToHide.Length;
                 i++)
            {
                if (objectsToHide[i] == null)
                    continue;

                originalObjectStates[i] =
                    objectsToHide[i].activeSelf;

                objectsToHide[i]
                    .SetActive(false);
            }
        }

        if (waterFadeCoroutine != null)
        {
            StopCoroutine(
                waterFadeCoroutine
            );
        }

        waterFadeCoroutine =
            StartCoroutine(
                FadeWater()
            );
    }

    private IEnumerator FadeWater()
    {
        float elapsed = 0f;

        if (waterFadeDuration <= 0f)
        {
            SetWaterTransition(
                waterfall,
                1f
            );

            SetWaterTransition(
                pond,
                1f
            );

            waterFadeCoroutine =
                null;

            yield break;
        }

        while (elapsed <
               waterFadeDuration)
        {
            elapsed +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    waterFadeDuration
                );

            progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            SetWaterTransition(
                waterfall,
                progress
            );

            SetWaterTransition(
                pond,
                progress
            );

            yield return null;
        }

        SetWaterTransition(
            waterfall,
            1f
        );

        SetWaterTransition(
            pond,
            1f
        );

        waterFadeCoroutine =
            null;
    }

    private void SetWaterTransition(
        Renderer waterRenderer,
        float value)
    {
        if (waterRenderer == null)
            return;

        MaterialPropertyBlock block =
            new MaterialPropertyBlock();

        waterRenderer.GetPropertyBlock(
            block
        );

        block.SetFloat(
            transitionPropertyID,
            value
        );

        waterRenderer.SetPropertyBlock(
            block
        );
    }

    private void RestoreWaterAndObjects()
    {
        if (waterFadeCoroutine != null)
        {
            StopCoroutine(
                waterFadeCoroutine
            );

            waterFadeCoroutine =
                null;
        }

        if (waterfall != null &&
            originalWaterfallMaterials != null)
        {
            waterfall.sharedMaterials =
                originalWaterfallMaterials;

            if (originalWaterfallBlock != null)
            {
                waterfall.SetPropertyBlock(
                    originalWaterfallBlock
                );
            }
        }

        if (pond != null &&
            originalPondMaterials != null)
        {
            pond.sharedMaterials =
                originalPondMaterials;

            if (originalPondBlock != null)
            {
                pond.SetPropertyBlock(
                    originalPondBlock
                );
            }
        }

        if (objectsToHide != null &&
            originalObjectStates != null)
        {
            for (int i = 0;
                 i < objectsToHide.Length;
                 i++)
            {
                if (objectsToHide[i] == null)
                    continue;

                objectsToHide[i]
                    .SetActive(
                        originalObjectStates[i]
                    );
            }
        }

        originalWaterfallMaterials =
            null;

        originalPondMaterials =
            null;

        originalWaterfallBlock =
            null;

        originalPondBlock =
            null;

        originalObjectStates =
            null;

        waterEffectStarted =
            false;
    }

    private void RemoveNightMonsters()
    {
        for (int i =
             spawnedMonsters.Count - 1;
             i >= 0;
             i--)
        {
            GameObject monster =
                spawnedMonsters[i];

            if (monster == null)
            {
                spawnedMonsters.RemoveAt(i);
                continue;
            }

            if (rainSpawnedMonsters.Contains(
                monster))
            {
                continue;
            }

            Destroy(monster);

            spawnedMonsters.RemoveAt(i);
        }
    }

    private void RemoveRainMonsters()
    {
        for (int i =
             rainSpawnedMonsters.Count - 1;
             i >= 0;
             i--)
        {
            GameObject monster =
                rainSpawnedMonsters[i];

            if (monster != null)
            {
                spawnedMonsters.Remove(
                    monster
                );

                Destroy(monster);
            }
        }

        rainSpawnedMonsters.Clear();
    }

    public void StopSpawnedMonsters(
        bool stopped)
    {
        for (int i =
             spawnedMonsters.Count - 1;
             i >= 0;
             i--)
        {
            if (spawnedMonsters[i] == null)
            {
                spawnedMonsters.RemoveAt(i);
                continue;
            }

            Monster monster =
                spawnedMonsters[i]
                    .GetComponent<Monster>();

            if (monster != null)
            {
                monster.enabled =
                    !stopped;
            }
        }
    }
}