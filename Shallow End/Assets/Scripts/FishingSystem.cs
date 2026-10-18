using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (inventory == null || useAction == null)
            return;

        // Stop fishing if the player changes to another item.
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
                // The fish has not bitten yet
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
    }

    private void TryCast()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            castRange,
            waterLayer,
            QueryTriggerInteraction.Collide))
        {
            inventory.ShowInteractionMessage(
                "Look at a body of water to fish."
            );
            return;
        }

        state = FishingState.WaitingForBite;

        inventory.ShowInteractionMessage(
            "You cast your fishing rod..."
        );

        fishingCoroutine = StartCoroutine(WaitForBite());
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

        inventory.ShowInteractionMessage(
            "Fish on! LEFT CLICK to reel it in!"
        );

        fishingCoroutine = StartCoroutine(ReelTimer());
    }

    private IEnumerator ReelTimer()
    {
        yield return new WaitForSeconds(reelWindow);

        if (state == FishingState.FishBiting)
        {
            state = FishingState.Idle;
            fishingCoroutine = null;

            inventory.ShowInteractionMessage(
                "The fish got away!"
            );
        }
    }

    private void CatchFish()
    {
        if (fishItem == null || fishItem.ItenPrefab == null)
        {
            inventory.ShowInteractionMessage(
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

        // Try to place the fish on the ground.
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

        inventory.ShowInteractionMessage(
            "You caught a fish!"
        );
    }

    private void CancelFishing(string message)
    {
        StopFishingCoroutine();
        state = FishingState.Idle;

        if (!string.IsNullOrEmpty(message) && inventory != null)
        {
            inventory.ShowInteractionMessage(message);
        }
    }

    private void StopFishingCoroutine()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopFishingCoroutine();
        state = FishingState.Idle;
    }
}
