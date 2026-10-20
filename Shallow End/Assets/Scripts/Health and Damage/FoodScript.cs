
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

    [System.Serializable]
    public class FoodItem
    {
        public Items item;
        public float hungerIncrease;
        public float thirstIncrease;
    }

    [Header("Normal Food")]
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
    [Tooltip("Movement speed multiplier for the speed boost.")]
    [SerializeField] private float speedMultiplier = 1.25f;

    [Tooltip("Duration of the speed boost in seconds.")]
    [SerializeField] private float speedBoostDuration = 60f;

    private enum MushroomEffect
    {
        Neutral,
        Poison,
        FasterMovement,
        FullRecovery
    }

    private Coroutine effectDisplayCoroutine;

    private void Awake()
    {
        if (mushroomEffectImage != null)
        {
            SetImageAlpha(0f);
        }
    }

    private void OnEnable()
    {
        if (eatAct != null && eatAct.action != null)
        {
            eatAct.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (eatAct != null && eatAct.action != null)
        {
            eatAct.action.Disable();
        }

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

    private void Update()
    {
        if (eatAct == null || eatAct.action == null)
            return;

        if (!eatAct.action.WasPressedThisFrame())
            return;

        EatFood();
    }

    private void EatFood()
    {
        if (inventory == null || healthScript == null)
            return;

        Items hotbarItem = inventory.GetHotbarItem();

        if (hotbarItem == null)
            return;

        // Handle mushrooms separately from normal food.
        if (mushroomItem != null && hotbarItem == mushroomItem)
        {
            EatMushroom();
            return;
        }

        // Existing normal food functionality.
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

        // Consume one mushroom after applying its effect.
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

        // Stop any previous display or fade timer.
        if (effectDisplayCoroutine != null)
        {
            StopCoroutine(effectDisplayCoroutine);
        }

        // Set the new symbol and show it at full opacity.
        mushroomEffectImage.sprite = effectSprite;
        SetImageAlpha(1f);

        // Start the visibility timer and fade-out.
        effectDisplayCoroutine =
            StartCoroutine(HideEffectAfterDelay());
    }

    private IEnumerator HideEffectAfterDelay()
    {
        // Keep the symbol fully visible.
        yield return new WaitForSeconds(effectDisplayDuration);

        float elapsed = 0f;

        // Gradually fade the symbol from visible to invisible.
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float fadeProgress = Mathf.Clamp01(
                elapsed / fadeOutDuration
            );

            float alpha = Mathf.Lerp(1f, 0f, fadeProgress);

            SetImageAlpha(alpha);

            yield return null;
        }

        // Ensure the image ends completely invisible.
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
