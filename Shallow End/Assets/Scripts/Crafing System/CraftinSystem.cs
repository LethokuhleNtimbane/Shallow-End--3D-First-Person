
using UnityEngine;
using System.Collections.Generic;

public class CraftinSystem : MonoBehaviour
{
    [Header("Crafting Slots")]
    public Slot[] craftinSlots;

    public Slot resultSlot;

    [Header("Available Recipes")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    private CraftingRecipe currentRecipe;

    public CraftingRecipe CurrentRecipe => currentRecipe;

    public void UpdateCraftingResult()
    {
        currentRecipe = FindMatchingRecipe();

        if (currentRecipe != null && resultSlot != null)
        {
            resultSlot.SetItem(
                currentRecipe.resultItem,
                currentRecipe.resultAmount
            );
        }
        else if (resultSlot != null)
        {
            resultSlot.ClearSlot();
        }
    }

    public CraftingRecipe FindMatchingRecipe()
    {
        if (craftinSlots == null || craftinSlots.Length < 4)
            return null;

        foreach (CraftingRecipe recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (MatchesRecipe(recipe))
                return recipe;
        }

        return null;
    }

    private bool MatchesRecipe(CraftingRecipe recipe)
    {
        return
            SlotMatches(craftinSlots[0], recipe.slotItem1) &&
            SlotMatches(craftinSlots[1], recipe.slotItem2) &&
            SlotMatches(craftinSlots[2], recipe.slotItem3) &&
            SlotMatches(craftinSlots[3], recipe.slotItem4);
    }

    private bool SlotMatches(Slot slot, Items requiredItem)
    {
        if (slot == null)
            return false;

        if (requiredItem == null)
            return !slot.Hasitem();

        return slot.Hasitem() &&
               slot.GetItem() == requiredItem &&
               slot.GetAmount() >= 1;
    }
}
