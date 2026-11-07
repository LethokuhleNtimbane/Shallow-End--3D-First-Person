
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Night Time")]
    [SerializeField] private float firstSpawnTime = 21f;
    [SerializeField] private float lastSpawnTime = 5f;

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

    private int currentNight = 0;
    private int monstersToSpawn = 0;
    private int monstersSpawned = 0;

    private List<GameObject> spawnedMonsters =
        new List<GameObject>();

    private Material[] originalWaterfallMaterials;
    private Material[] originalPondMaterials;

    private MaterialPropertyBlock originalWaterfallBlock;
    private MaterialPropertyBlock originalPondBlock;

    private bool[] originalObjectStates;

    private bool waterEffectStarted = false;
    private Coroutine waterFadeCoroutine;

    private int transitionPropertyID;

    private void Start()
    {
        nextSpawnTimer = 0f;

        transitionPropertyID =
            Shader.PropertyToID(transitionProperty);

        if (portalEffects != null)
            portalEffects.SetActive(false);

        if (TimeController.instance != null)
        {
            float hour = GetCurrentHour();

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

        float hour = GetCurrentHour();

        bool isNight =
            hour >= firstSpawnTime ||
            hour < lastSpawnTime;

        if (isNight && !wasNight)
            StartNight();

        if (!isNight && wasNight)
            EndNight();

        wasNight = isNight;

        if (!isNight)
            return;

        HandleSpawning();
    }

    private float GetCurrentHour()
    {
        System.DateTime currentTime =
            TimeController.instance.CurrentTime;

        return currentTime.Hour +
               currentTime.Minute / 60f +
               currentTime.Second / 3600f;
    }

    private void StartNight()
    {
        currentNight++;

        /*
         * Night 1 = 0 monsters
         * Night 2 = 2 monsters
         * Night 3 = 3 monsters
         * Night 4 = 4 monsters
         */

        if (currentNight == 1)
        {
            monstersToSpawn = 0;
        }
        else
        {
            monstersToSpawn =
                startingMonsterCount +
                (currentNight - 1) *
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

        // Restore the water and hidden objects at 05:00.
        RestoreWaterAndObjects();

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
            nextSpawnTimer =
                Random.Range(
                    minimumSpawnDelay,
                    maximumSpawnDelay
                );
        }
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null)
            return;

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
            return;

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

        spawnedMonsters.Add(newMonster);

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

        // Start the water effect only once per night.
        if (!waterEffectStarted)
        {
            waterEffectStarted = true;
            BeginWaterEffect();
        }

        // Move and activate the entire portal parent.
        if (portalEffects != null)
        {
            portalEffects.transform.position =
                portalPosition;

            portalEffects.SetActive(true);
        }

        yield return new WaitForSeconds(
            monsterInactiveTime
        );

        monster.SetActive(true);

        Monster monsterScript =
            monster.GetComponent<Monster>();

        if (monsterScript != null)
            monsterScript.enabled = false;

        float elapsed = 0f;

        Vector3 startPosition =
            portalPosition;

        while (elapsed < monsterRiseDuration)
        {
            elapsed += Time.deltaTime;

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

        monster.transform.position =
            finalPosition;

        if (portalEffects != null)
            portalEffects.SetActive(false);

        if (monsterScript != null)
        {
            monsterScript.SetSpawnerControlled(false);
            monsterScript.enabled = true;
        }
    }

    private void BeginWaterEffect()
    {
        // Save the original materials and property blocks.
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

            // Assign the material you created in Shader Graph.
            waterfall.sharedMaterial =
                waterfallAlteredMaterial;

            SetWaterTransition(waterfall, 0f);
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

            SetWaterTransition(pond, 0f);
        }

        // Save the active states before hiding objects.
        if (objectsToHide != null)
        {
            originalObjectStates =
                new bool[objectsToHide.Length];

            for (int i = 0; i < objectsToHide.Length; i++)
            {
                if (objectsToHide[i] == null)
                    continue;

                originalObjectStates[i] =
                    objectsToHide[i].activeSelf;

                objectsToHide[i].SetActive(false);
            }
        }

        // Begin the Shader Graph transition.
        if (waterFadeCoroutine != null)
            StopCoroutine(waterFadeCoroutine);

        waterFadeCoroutine =
            StartCoroutine(FadeWater());
    }

    private IEnumerator FadeWater()
    {
        float elapsed = 0f;

        if (waterFadeDuration <= 0f)
        {
            SetWaterTransition(waterfall, 1f);
            SetWaterTransition(pond, 1f);
            yield break;
        }

        while (elapsed < waterFadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / waterFadeDuration
                );

            progress =
                Mathf.SmoothStep(0f, 1f, progress);

            SetWaterTransition(waterfall, progress);
            SetWaterTransition(pond, progress);

            yield return null;
        }

        SetWaterTransition(waterfall, 1f);
        SetWaterTransition(pond, 1f);

        waterFadeCoroutine = null;
    }

    private void SetWaterTransition(
        Renderer waterRenderer,
        float value)
    {
        if (waterRenderer == null)
            return;

        MaterialPropertyBlock block =
            new MaterialPropertyBlock();

        waterRenderer.GetPropertyBlock(block);

        block.SetFloat(
            transitionPropertyID,
            value
        );

        waterRenderer.SetPropertyBlock(block);
    }

    private void RestoreWaterAndObjects()
    {
        // Stop any transition that is still running.
        if (waterFadeCoroutine != null)
        {
            StopCoroutine(waterFadeCoroutine);
            waterFadeCoroutine = null;
        }

        // Restore the waterfall's original materials.
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

        // Restore the pond's original materials.
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

        // Restore each object to its previous active state.
        if (objectsToHide != null &&
            originalObjectStates != null)
        {
            for (int i = 0; i < objectsToHide.Length; i++)
            {
                if (objectsToHide[i] == null)
                    continue;

                objectsToHide[i].SetActive(
                    originalObjectStates[i]
                );
            }
        }

        originalWaterfallMaterials = null;
        originalPondMaterials = null;
        originalWaterfallBlock = null;
        originalPondBlock = null;
        originalObjectStates = null;

        waterEffectStarted = false;
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

    public void StopSpawnedMonsters(bool stopped)
    {
        for (int i = spawnedMonsters.Count - 1; i >= 0; i--)
        {
            if (spawnedMonsters[i] == null)
            {
                spawnedMonsters.RemoveAt(i);
                continue;
            }

            Monster monster =
                spawnedMonsters[i].GetComponent<Monster>();

            if (monster != null)
            {
                monster.enabled = !stopped;
            }
        }
    }
}
