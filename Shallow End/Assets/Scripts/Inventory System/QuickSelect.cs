
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickCraftButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private CraftinSystem craftingSystem;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Manually Assigned Crafting Slots")]
    [SerializeField] private Slot slot1;
    [SerializeField] private Slot slot2;
    [SerializeField] private Slot slot3;
    [SerializeField] private Slot slot4;

    [Header("Ingredients For This Recipe")]
    [SerializeField] private Items ingredientSlot1;
    [SerializeField] private Items ingredientSlot2;
    [SerializeField] private Items ingredientSlot3;
    [SerializeField] private Items ingredientSlot4;

    [Header("Canvas Group Alpha")]
    [SerializeField, Range(0f, 1f)]
    private float availableAlpha = 1f;

    [SerializeField, Range(0f, 1f)]
    private float unavailableAlpha = 0.7f;

    private bool isPreparing;

    private Slot[] GetSlots()
    {
        return new Slot[]
        {
            slot1,
            slot2,
            slot3,
            slot4
        };
    }

    private Items[] GetIngredients()
    {
        return new Items[]
        {
            ingredientSlot1,
            ingredientSlot2,
            ingredientSlot3,
            ingredientSlot4
        };
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        RefreshButton();
    }

    private void Update()
    {
        RefreshButton();
    }

    private void OnDisable()
    {
        isPreparing = false;
    }

    private bool CanPrepareRecipe()
    {
        if (inventory == null || craftingSystem == null)
            return false;

        Slot[] slots = GetSlots();
        Items[] ingredients = GetIngredients();

        foreach (Slot slot in slots)
        {
            if (slot == null || slot.Hasitem())
                return false;
        }

        Dictionary<Items, int> required =
            new Dictionary<Items, int>();

        foreach (Items item in ingredients)
        {
            if (item == null)
                continue;

            if (!required.ContainsKey(item))
                required[item] = 0;

            required[item]++;
        }

        if (required.Count == 0)
            return false;

        foreach (KeyValuePair<Items, int> entry in required)
        {
            if (inventory.GetTotalItemAmount(entry.Key)
                < entry.Value)
            {
                return false;
            }
        }

        return true;
    }

    public void RefreshButton()
    {
        bool canCraft = !isPreparing && CanPrepareRecipe();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = canCraft
                ? availableAlpha
                : unavailableAlpha;
        }

        if (button != null)
            button.interactable = canCraft;
    }

    public void CraftButtonPressed()
    {
        Debug.Log("QUICK CRAFT: Button clicked!", this);

        if (isPreparing)
            return;

        if (!CanPrepareRecipe())
        {
            Debug.LogWarning(
                "QUICK CRAFT: Requirements are not met.",
                this
            );

            RefreshButton();
            return;
        }

        Slot[] slots = GetSlots();
        Items[] ingredients = GetIngredients();

        isPreparing = true;

        // Remove the required ingredients from the inventory.
        List<Items> removedIngredients = new List<Items>();

        foreach (Items item in ingredients)
        {
            if (item == null)
                continue;

            int removed = inventory.RemoveItemAmount(item, 1);

            if (removed != 1)
            {
                // Return any ingredients already removed.
                foreach (Items previousItem in removedIngredients)
                    inventory.AddItem(previousItem, 1);

                Debug.LogError(
                    "QUICK CRAFT: Could not remove " + item.name +
                    ". Previously removed ingredients were returned.",
                    this
                );

                isPreparing = false;
                RefreshButton();
                return;
            }

            removedIngredients.Add(item);
        }

        // Place ingredients into the assigned crafting slots.
        for (int i = 0; i < ingredients.Length; i++)
        {
            if (ingredients[i] == null)
                continue;

            slots[i].SetItem(ingredients[i], 1);

            Debug.Log(
                "QUICK CRAFT: Placed " + ingredients[i].name +
                " into " + slots[i].name,
                this
            );
        }

        craftingSystem.UpdateCraftingResult();

        Debug.Log("QUICK CRAFT: Ingredients placed successfully.", this);

        isPreparing = false;
        RefreshButton();
    }
}
