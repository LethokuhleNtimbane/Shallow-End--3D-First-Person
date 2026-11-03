
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

    [Header("Required Ingredients")]
    [SerializeField] private Items[] requiredIngredients = new Items[4];

    private const int RequiredSlotCount = 4;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (button == null)
        {
            Debug.LogError("QuickCraftButton: No Button component assigned.", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(CraftButtonPressed);
    }

    private void OnEnable()
    {
        RefreshButton();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(CraftButtonPressed);
    }

    private void Update()
    {
        RefreshButton();
    }

    private Dictionary<Items, int> GetRequiredItemCounts()
    {
        Dictionary<Items, int> requiredCounts =
            new Dictionary<Items, int>();

        for (int i = 0; i < RequiredSlotCount; i++)
        {
            Items item = requiredIngredients[i];

            if (item == null)
                continue;

            if (requiredCounts.ContainsKey(item))
            {
                requiredCounts[item]++;
            }
            else
            {
                requiredCounts.Add(item, 1);
            }
        }

        return requiredCounts;
    }

    private bool CanQuickCraft()
    {
        if (inventory == null || craftingSystem == null)
            return false;

        if (craftingSystem.craftinSlots == null ||
            requiredIngredients == null)
            return false;

        if (craftingSystem.craftinSlots.Length != RequiredSlotCount ||
            requiredIngredients.Length != RequiredSlotCount)
            return false;

        // Every ingredient slot on the crafting table must be empty.
        // Items already in the crafting table are never counted.
        foreach (Slot slot in craftingSystem.craftinSlots)
        {
            if (slot == null || slot.Hasitem())
                return false;
        }

        Dictionary<Items, int> requiredCounts =
            GetRequiredItemCounts();

        if (requiredCounts.Count == 0)
            return false;

        // Only check the hotbar for the required ingredients.
        foreach (KeyValuePair<Items, int> requirement in requiredCounts)
        {
            int amountInHotbar =
                inventory.GetTotalItemAmount(requirement.Key);

            if (amountInHotbar < requirement.Value)
                return false;
        }

        return true;
    }

    private void RefreshButton()
    {
        if (button == null)
            return;

        bool canCraft = CanQuickCraft();

        button.interactable = canCraft;

        if (canvasGroup != null)
            canvasGroup.alpha = canCraft ? 1f : 0.5f;
    }

    public void CraftButtonPressed()
    {
        // Check everything again when the button is pressed.
        if (!CanQuickCraft())
        {
            RefreshButton();
            return;
        }

        Dictionary<Items, int> requiredCounts =
            GetRequiredItemCounts();

        List<Items> removedItems = new List<Items>();
        List<int> removedAmounts = new List<int>();

        // Remove the ingredients directly from the hotbar.
        foreach (KeyValuePair<Items, int> requirement in requiredCounts)
        {
            int removedAmount = inventory.RemoveItemAmount(
                requirement.Key,
                requirement.Value
            );

            if (removedAmount > 0)
            {
                removedItems.Add(requirement.Key);
                removedAmounts.Add(removedAmount);
            }

            // Restore anything already removed if an unexpected
            // removal failure occurs.
            if (removedAmount != requirement.Value)
            {
                RestoreRemovedItems(removedItems, removedAmounts);

                Debug.LogError(
                    "Quick crafting stopped because an ingredient could not be removed."
                );

                RefreshButton();
                return;
            }
        }

        // Place the ingredients directly into their corresponding slots.
        // This does not use CraftinSystem to collect or transfer anything.
        for (int i = 0; i < RequiredSlotCount; i++)
        {
            Items item = requiredIngredients[i];

            if (item == null)
                continue;

            craftingSystem.craftinSlots[i].SetItem(item, 1);
        }

        // This only refreshes the crafting result after the transfer.
        craftingSystem.UpdateCraftingResult();

        RefreshButton();
    }

    private void RestoreRemovedItems(
        List<Items> items,
        List<int> amounts)
    {
        for (int i = 0; i < items.Count; i++)
        {
            bool restored = inventory.AddItem(items[i], amounts[i]);

            if (!restored)
            {
                Debug.LogError(
                    "Quick crafting could not fully restore an ingredient: " +
                    items[i].itemName
                );
            }
        }
    }
}
