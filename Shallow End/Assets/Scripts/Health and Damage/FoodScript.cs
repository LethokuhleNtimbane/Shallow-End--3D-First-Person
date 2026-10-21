
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FoodScript : MonoBehaviour
{
    [Header("Input and References")]
    [SerializeField] private InputActionReference eatAct;
    [SerializeField] private Inventory inventory;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private PlayerController playerController;

    [Header("Water Bucket Input")]
    [Tooltip("Left-click to fill an empty bucket when aiming at water.")]
    [SerializeField] private InputActionReference fillWaterAction;

    [Tooltip("Right-click to drink from a filled bucket.")]
    [SerializeField] private InputActionReference drinkWaterAction;

    [Tooltip("Layer name used by water objects.")]
    [SerializeField] private string waterLayerName = "Water";

    [Header("Water Bucket Items")]
    [SerializeField] private Items emptyWaterBucketItem;
    [SerializeField] private Items filledWaterBucketItem;

    [Tooltip("Maximum distance for the water raycast.")]
    [SerializeField] private float waterRaycastDistance = 5f;

    [Tooltip("Amount of thirst restored when drinking.")]
    [SerializeField] private float waterThirstIncrease = 65f;

    [Tooltip("Percentage of current health removed when drinking.")]
    [Range(0f, 100f)]
    [SerializeField] private float waterHealthLossPercent = 65f;

    [Tooltip("Percentage of current hunger removed when drinking.")]
    [Range(0f, 100f)]
    [SerializeField] private float waterHungerLossPercent = 65f;

    [System.Serializable]
    public class FoodItem
    {
        public Items item;
        public float hungerIncrease;
        public float thirstIncrease;
    }

    [SerializeField] private FoodItem[] foodItems;

    [Header("Mushroom")]
    [SerializeField] private Items mushroomItem;

    [Header("Mushroom Effect UI")]
    [SerializeField] private Image mushroomEffectImage;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite poisonSprite;
    [SerializeField] private Sprite fasterMovementSprite;
    [SerializeField] private Sprite fullRecoverySprite;

    [Tooltip("How long the symbol stays fully visible.")]
    [SerializeField] private float effectDisplayDuration = 3f;

    [Tooltip("How long the symbol takes to fade out.")]
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Mushroom Speed Boost")]
    [SerializeField] private float speedMultiplier = 1.25f;
    [SerializeField] private float speedBoostDuration = 60f;

    private enum MushroomEffect
    {
        Neutral,
        Poison,
        FasterMovement,
        FullRecovery
    }

    private Coroutine effectDisplayCoroutine;
    private Camera playerCamera;
    private int waterLayer = -1;

    private void Awake()
    {
        playerCamera = Camera.main;
        waterLayer = LayerMask.NameToLayer(waterLayerName);

        if (mushroomEffectImage != null)
        {
            SetImageAlpha(0f);
        }

        if (waterLayer == -1)
        {
            Debug.LogWarning(
                "FoodScript: No layer named '" + waterLayerName +
                "' exists. Create the layer or change Water Layer Name."
            );
        }
    }

    private void OnEnable()
    {
        EnableAction(eatAct);
        EnableAction(fillWaterAction);
        EnableAction(drinkWaterAction);
    }

    private void OnDisable()
    {
        DisableAction(eatAct);
        DisableAction(fillWaterAction);
        DisableAction(drinkWaterAction);

        if (effectDisplayCoroutine != null)
        {
            StopCoroutine(effectDisplayCoroutine);
            effectDisplayCoroutine = null;
        }

        if (mushroomEffectImage != null)
        {
            SetImageAlpha(0f);
        }
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Enable();
        }
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Disable();
        }
    }

    private void Update()
    {
        Items selectedItem = inventory != null
            ? inventory.GetHotbarItem()
            : null;

        if (selectedItem == emptyWaterBucketItem &&
            emptyWaterBucketItem != null &&
            WasPressed(fillWaterAction))
        {
            TryFillBucket();
        }

        if (selectedItem == filledWaterBucketItem &&
            filledWaterBucketItem != null &&
            WasPressed(drinkWaterAction))
        {
            DrinkWater();
        }

        if (WasPressed(eatAct))
        {
            EatFood();
        }
    }

    private bool WasPressed(InputActionReference actionReference)
    {
        return actionReference != null &&
               actionReference.action != null &&
               actionReference.action.WasPressedThisFrame();
    }

    private void TryFillBucket()
    {
        if (inventory == null || playerCamera == null)
        {
            Debug.LogWarning(
                "FoodScript: Inventory or player camera is missing."
            );
            return;
        }

        if (waterLayer == -1)
            return;

        // Make sure the selected empty bucket has durability.
        int currentDurability = inventory.GetEquippedDurability();

        if (currentDurability < 0)
        {
            Debug.LogWarning(
                "Water bucket has no durability configured. " +
                "Enable durability on the empty bucket Items asset."
            );
            return;
        }

        // Check whether the player is looking at water.
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        int layerMask = 1 << waterLayer;

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                waterRaycastDistance,
                layerMask,
                QueryTriggerInteraction.Collide))
        {
            return;
        }

        // Fetching water costs one durability.
        int remainingDurability = currentDurability - 1;

        // If the bucket uses its final durability point,
        // it breaks instead of producing a filled bucket.
        inventory.RemoveHotbarItem(1);

        if (remainingDurability <= 0)
        {
            Debug.Log("The empty water bucket broke while fetching water!");
            return;
        }

        // Create the filled bucket with the remaining durability.
        inventory.AddItem(
            filledWaterBucketItem,
            1,
            remainingDurability
        );

        Debug.Log(
            "Water bucket filled! Remaining durability: " +
            remainingDurability
        );
    }

    private void DrinkWater()
    {
        if (inventory == null || healthScript == null)
            return;

        // Read the filled bucket's current durability.
        int currentDurability = inventory.GetEquippedDurability();

        if (currentDurability < 0)
        {
            Debug.LogWarning(
                "Filled water bucket has no durability configured. " +
                "Enable durability on the filled bucket Items asset."
            );
            return;
        }

        // Restore thirst.
        healthScript.AddThirst(waterThirstIncrease);

        // Reduce current health and hunger by the configured percentages.
        float newHealth = healthScript.health *
            (1f - waterHealthLossPercent / 100f);

        float hungerToRemove = healthScript.hunger *
            (waterHungerLossPercent / 100f);

        healthScript.SetHealth(newHealth);
        healthScript.RemoveHunger(hungerToRemove);

        // Remove the filled bucket without consuming durability.
        inventory.RemoveHotbarItem(1);

        // Return the same bucket as empty, keeping its durability.
        inventory.AddItem(
            emptyWaterBucketItem,
            1,
            currentDurability
        );

        Debug.Log(
            "Drank water. Bucket durability preserved at: " +
            currentDurability
        );
    }

    private void EatFood()
    {
        if (inventory == null || healthScript == null)
            return;

        Items hotbarItem = inventory.GetHotbarItem();

        if (hotbarItem == null)
            return;

        if (mushroomItem != null && hotbarItem == mushroomItem)
        {
            EatMushroom();
            return;
        }

        FoodItem food = GetFoodData(hotbarItem);

        if (food == null)
            return;

        if (food.hungerIncrease > 0f &&
            healthScript.PlayerIsFullHunger())
        {
            return;
        }

        if (food.thirstIncrease > 0f &&
            healthScript.PlayerIsFullThirst())
        {
            return;
        }

        healthScript.AddHunger(food.hungerIncrease);
        healthScript.AddThirst(food.thirstIncrease);

        inventory.RemoveHotbarItem(1);
    }

    private void EatMushroom()
    {
        MushroomEffect effect =
            (MushroomEffect)Random.Range(0, 4);

        switch (effect)
        {
            case MushroomEffect.Neutral:
                Debug.Log("Mushroom Effect: Nothing");
                ShowMushroomEffect(neutralSprite);
                break;

            case MushroomEffect.Poison:
                Debug.Log("Mushroom Effect: Poison");

                healthScript.SetHealth(
                    healthScript.health * 0.5f
                );

                ShowMushroomEffect(poisonSprite);
                break;

            case MushroomEffect.FasterMovement:
                Debug.Log(
                    "Mushroom Effect: Faster Movement (+25% for 60 seconds)"
                );

                if (playerController != null)
                {
                    playerController.ApplyTemporarySpeedBoost(
                        speedMultiplier,
                        speedBoostDuration
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Mushroom: PlayerController is not assigned!"
                    );
                }

                ShowMushroomEffect(fasterMovementSprite);
                break;

            case MushroomEffect.FullRecovery:
                Debug.Log("Mushroom Effect: Full Recovery");

                healthScript.SetHealth(healthScript.maxHealth);
                healthScript.AddHunger(healthScript.maxHunger);
                healthScript.AddThirst(healthScript.maxThirst);
                healthScript.SetHealthRegenerationEnabled(true);

                ShowMushroomEffect(fullRecoverySprite);
                break;
        }

        inventory.RemoveHotbarItem(1);
    }

    private void ShowMushroomEffect(Sprite effectSprite)
    {
        if (mushroomEffectImage == null)
        {
            Debug.LogWarning(
                "FoodScript: Mushroom Effect Image is not assigned!"
            );
            return;
        }

        if (effectSprite == null)
        {
            Debug.LogWarning(
                "FoodScript: The sprite for this mushroom effect is not assigned!"
            );
            return;
        }

        if (effectDisplayCoroutine != null)
        {
            StopCoroutine(effectDisplayCoroutine);
        }

        mushroomEffectImage.sprite = effectSprite;
        SetImageAlpha(1f);

        effectDisplayCoroutine =
            StartCoroutine(HideEffectAfterDelay());
    }

    private IEnumerator HideEffectAfterDelay()
    {
        yield return new WaitForSeconds(effectDisplayDuration);

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float fadeProgress = fadeOutDuration > 0f
                ? Mathf.Clamp01(elapsed / fadeOutDuration)
                : 1f;

            SetImageAlpha(Mathf.Lerp(1f, 0f, fadeProgress));

            yield return null;
        }

        SetImageAlpha(0f);
        effectDisplayCoroutine = null;
    }

    private void SetImageAlpha(float alpha)
    {
        if (mushroomEffectImage == null)
            return;

        Color imageColor = mushroomEffectImage.color;
        imageColor.a = alpha;
        mushroomEffectImage.color = imageColor;
    }

    private FoodItem GetFoodData(Items item)
    {
        if (foodItems == null)
            return null;

        foreach (FoodItem food in foodItems)
        {
            if (food != null && food.item == item)
            {
                return food;
            }
        }

        return null;
    }
}
