using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class FishingSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform player;
    [SerializeField] private InputActionReference useAction;

    [Header("Fishing Items")]
    [SerializeField] private Items fishingRodItem;
    [SerializeField] private Items fishItem;

    [Header("Water Detection")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float castRange = 15f;

    [Header("Fishing Timing")]
    [SerializeField] private float minimumBiteDelay = 3f;
    [SerializeField] private float maximumBiteDelay = 7f;
    [SerializeField] private float reelWindow = 3f;

    [Header("Fish Spawn")]
    [SerializeField] private float spawnDistance = 1.2f;
    [SerializeField] private float groundCheckHeight = 3f;
    [SerializeField] private float groundCheckDistance = 10f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Fishing Line")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private Transform rodTip;
    [SerializeField] private int lineSegments = 20;
    [SerializeField] private float lineSag = 1f;
    [SerializeField] private float maxLineLength = 15f;

    [Header("Water Ripple")]
    [SerializeField] private ParticleSystem ripplePrefab;
    [SerializeField] private float rippleInterval = 1.5f;

    [Header("Fishing Tug")]
    [SerializeField] private float tugDuration = 0.35f;
    [SerializeField] private float tugInterval = 0.6f;
    [SerializeField] private float tugDistance = 0.3f;
    [SerializeField] private float tugExtraSag = 0.25f;

    [Header("Fishing Splash Sound")]
    [SerializeField] private AudioSource splashAudioSource;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private float splashVolume = 1f;

    [Header("Fishing UI")]
    [SerializeField] private bool showFishingMessages = false;

    private Coroutine rippleCoroutine;
    private readonly List<ParticleSystem> activeRipples =
    new List<ParticleSystem>();

    private Coroutine tugCoroutine;
    private bool isTugging;


    private Vector3 waterPoint;

    private void ShowFishingMessage(string message)
    {
        if (showFishingMessages && inventory != null)
        {
            inventory.ShowInteractionMessage(message);
        }
    }

    private enum FishingState
    {
        Idle,
        WaitingForBite,
        FishBiting
    }

    private FishingState state = FishingState.Idle;
    private Coroutine fishingCoroutine;

    

    private void Update()
    {

        if (state != FishingState.Idle)
        {
            if (rodTip != null)
            {
                float lineLength = Vector3.Distance(
                    rodTip.position,
                    waterPoint
                );

                if (lineLength >= maxLineLength)
                {
                    CancelFishing("You moved too far away! Cast again to fish.");
                    return;
                }
            }

            if (fishingLine != null && fishingLine.enabled)
            {
                UpdateFishingLine();
            }
        }

        if (inventory == null || useAction == null)
            return;

        // Stop fishing if the player changes to another item
        if (state != FishingState.Idle &&
            inventory.GetHotbarItem() != fishingRodItem)
        {
            CancelFishing("Fishing cancelled.");
            return;
        }

        if (!useAction.action.WasPressedThisFrame())
            return;
        Debug.Log("Fishing system detected left-click!");

        // Only the fishing rod is going to use this fishing system
        if (inventory.GetHotbarItem() != fishingRodItem)
            return;

        switch (state)
        {
            case FishingState.Idle:
                TryCast();
                break;

            case FishingState.WaitingForBite:
                CancelFishing("You stopped fishing.");
                break;

            case FishingState.FishBiting:
                CatchFish();
                break;
        }
    }

    private void Start()
    {
        Debug.Log("FishingSystem is running!");

        if (inventory == null)
            Debug.LogError("Inventory is not assigned!");

        if (useAction == null)
            Debug.LogError("Use Action is not assigned!");
        else
            Debug.Log("Use Action: " + useAction.action.name
                + " | Enabled: " + useAction.action.enabled);

        if (fishingLine != null)
        {
            fishingLine.useWorldSpace = true;
            fishingLine.positionCount = lineSegments;
            fishingLine.enabled = false;
        }
    }

    private void TryCast()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
    playerCamera.transform.position,
    playerCamera.transform.forward
);

        // Detect both water and ground
        int castLayers = waterLayer | LayerMask.GetMask("Ground");

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            castRange,
            castLayers,
            QueryTriggerInteraction.Collide))
        {
            ShowFishingMessage("Look at a body of water to fish.");
            return;
        }

        // If the ray hits anything other than water first, don't allow fishing
        if (((1 << hit.collider.gameObject.layer) & waterLayer.value) == 0)
        {
            ShowFishingMessage("You cannot fish through the ground!");
            return;
        }

        waterPoint = hit.point;

        if (fishingLine != null)
        {
            fishingLine.positionCount = lineSegments;
            fishingLine.enabled = true;
        }

        UpdateFishingLine();

        Debug.Log("Cast successful! Water point: " + waterPoint);

        if (fishingLine != null)
        {
            fishingLine.enabled = true;
            Debug.Log("Fishing line enabled: " + fishingLine.enabled);
        }
        else
        {
            Debug.LogError("Fishing Line reference is missing!");
        }

        if (fishingLine != null)
            fishingLine.enabled = true;

        UpdateFishingLine();

        state = FishingState.WaitingForBite;

        ShowFishingMessage("You cast your fishing rod...");

        fishingCoroutine = StartCoroutine(WaitForBite());
        rippleCoroutine = StartCoroutine(MakeRipples());

    }

    private void HideFishingLine()
    {
        if (fishingLine == null)
            return;

        fishingLine.enabled = false;

        // Reset the line positions to the rod tip
        if (rodTip != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.SetPosition(0, rodTip.position);
            fishingLine.SetPosition(1, rodTip.position);
        }
    }

    private void UpdateFishingLine()
    {
        if (fishingLine == null || rodTip == null)
            return;

        fishingLine.positionCount = lineSegments;

        Vector3 lineEnd = waterPoint;

        if (isTugging)
        {
            Vector3 pullDirection = player != null
                ? (player.position - waterPoint).normalized
                : Vector3.zero;

            lineEnd += pullDirection * tugDistance;
        }

        float currentSag = lineSag +
            (isTugging ? tugExtraSag : 0f);

        for (int i = 0; i < lineSegments; i++)
        {
            float t = (float)i / (lineSegments - 1);

            Vector3 point = Vector3.Lerp(
                rodTip.position,
                lineEnd,
                t
            );

            point.y -= Mathf.Sin(t * Mathf.PI) * currentSag;

            fishingLine.SetPosition(i, point);
        }
    }
    private IEnumerator TugFishingLine()
    {
        isTugging = true;

        yield return new WaitForSeconds(tugDuration);

        isTugging = false;
        tugCoroutine = null;
    }

    private IEnumerator TugAtIntervals()
    {
        while (state == FishingState.FishBiting)
        {
            // Pull the line
            isTugging = true;
            UpdateFishingLine();

            // Play the splash while the line is tugging
            if (splashAudioSource != null && splashSound != null)
            {
                splashAudioSource.clip = splashSound;
                splashAudioSource.volume = splashVolume;

                //  randomise the pitch each time
                splashAudioSource.pitch = Random.Range(0.9f, 1.1f);

                // Start at a random point in the clip
                splashAudioSource.time = Random.Range(0f, 0.5f);

                splashAudioSource.Play();
            }

            yield return new WaitForSeconds(tugDuration);

            if (splashAudioSource != null)
            {
                splashAudioSource.Stop();
            }

            // Release the line
            isTugging = false;
            UpdateFishingLine();

            yield return new WaitForSeconds(tugInterval);
        }

        isTugging = false;
        tugCoroutine = null;
    }

    private Vector3 GetFishingLineEnd()
    {
        if (!isTugging)
            return waterPoint;

        // Pull the water end slightly toward the player
        Vector3 direction = player != null
            ? (player.position - waterPoint).normalized
            : Vector3.zero;

        return waterPoint + direction * tugDistance;
    }


    private IEnumerator WaitForBite()
    {
        float delay = Random.Range(
            minimumBiteDelay,
            maximumBiteDelay
        );

        yield return new WaitForSeconds(delay);

        if (inventory.GetHotbarItem() != fishingRodItem)
        {
            CancelFishing("Fishing cancelled.");
            yield break;
        }

        state = FishingState.FishBiting;

        ShowFishingMessage(
            "Fish on! LEFT CLICK to reel it in!"
        );

        tugCoroutine = StartCoroutine(TugAtIntervals());
        fishingCoroutine = StartCoroutine(ReelTimer());
    }

    private IEnumerator ReelTimer()
    {
        yield return new WaitForSeconds(reelWindow);

        if (state == FishingState.FishBiting)
        {
            state = FishingState.Idle;

            StopFishingEffects();

            fishingCoroutine = null;
            HideFishingLine();

            ShowFishingMessage("The fish got away!");
        }
    }

    private void CatchFish()
    {
        if (fishItem == null || fishItem.ItenPrefab == null)
        {

            if (fishingLine != null)
                fishingLine.enabled = false;
            ShowFishingMessage(
                "Fish item or fish prefab is missing!"
            );

            CancelFishing("");
            return;
        }

        StopFishingCoroutine();

        Vector3 spawnPosition =
            player != null
                ? player.position + player.forward * spawnDistance
                : playerCamera.transform.position +
                  playerCamera.transform.forward * spawnDistance;

        // Try to place the fish on the ground
        Vector3 rayOrigin = spawnPosition +
                            Vector3.up * groundCheckHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit groundHit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            spawnPosition = groundHit.point;
        }

        GameObject caughtFish = Instantiate(
            fishItem.ItenPrefab,
            spawnPosition,
            Quaternion.identity
        );

        GroundItem groundItem =
            caughtFish.GetComponent<GroundItem>();

        if (groundItem != null)
        {
            groundItem.item = fishItem;
            groundItem.amount = 1;
            groundItem.currentDurability = -1;
            groundItem.sourcePrefab = fishItem.ItenPrefab;
        }

        state = FishingState.Idle;
        HideFishingLine();

        ShowFishingMessage(
            "You caught a fish!"
        );

        
    }

    private void CancelFishing(string message)
    {
        StopFishingCoroutine();

        state = FishingState.Idle;

        HideFishingLine();

        if (!string.IsNullOrEmpty(message) && inventory != null)
        {
            ShowFishingMessage(message);
        }
    }

    private void StopFishingCoroutine()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }

        StopFishingEffects();
    }

    private IEnumerator MakeRipples()
    {
        while (state != FishingState.Idle)
        {
            if (ripplePrefab != null)
            {
                Vector3 ripplePosition = waterPoint;
                ripplePosition.y += 0.02f;

                ParticleSystem ripple = Instantiate(
                    ripplePrefab,
                    ripplePosition,
                    Quaternion.Euler(90f, 0f, 0f)
                );

                activeRipples.Add(ripple);
                ripple.Play();
            }

            yield return new WaitForSeconds(rippleInterval);
        }

        rippleCoroutine = null;
    }

    private void StopRipples()
    {
        if (rippleCoroutine != null)
        {
            StopCoroutine(rippleCoroutine);
            rippleCoroutine = null;
        }

        foreach (ParticleSystem ripple in activeRipples)
        {
            if (ripple != null)
            {
                ripple.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                Destroy(ripple.gameObject);
            }
        }

        activeRipples.Clear();
    }

    private void StopFishingEffects()
    {
        StopRipples();

        if (splashAudioSource != null)
        {
            splashAudioSource.Stop();
        } 

        if (tugCoroutine != null)
        {
            StopCoroutine(tugCoroutine);
            tugCoroutine = null;
        }

        isTugging = false;
        UpdateFishingLine();
    }

    private void OnDisable()
    {
        StopFishingCoroutine();

        state = FishingState.Idle;

        HideFishingLine();
    }
}
