using UnityEngine;
using System.Collections.Generic;

public class CraftinSystem : MonoBehaviour
{
    public Slot[] craftinSlots;

    public Slot resultSlot;

    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    private CraftingRecipe currentRecipe;

    public void UpdateCraftingResult()
    {
        currentRecipe = FindMatchingRecipe();

        if (currentRecipe != null)
        {
            resultSlot.SetItem(currentRecipe.resultItem, currentRecipe.resultAmount);

        }
        else
        {
            resultSlot.ClearSlot();
        }

    }
    private CraftingRecipe FindMatchingRecipe()
    {
        foreach (CraftingRecipe recipe in recipes)
        {
            if (MatchesRecipe(recipe))
            {
                return recipe;
            }
        }
        return null;
    }

    private bool MatchesRecipe(CraftingRecipe recipe)
    {


        if (!SlotMatches(craftinSlots[0], recipe.slotItem1))
        {
            
            return false;
        }

        if (!SlotMatches(craftinSlots[1], recipe.slotItem2))
        {
           
            return false;
        }

        if (!SlotMatches(craftinSlots[2], recipe.slotItem3))
        {
          
            return false;
        }

        if (!SlotMatches(craftinSlots[3], recipe.slotItem4))
        {
           
            return false;
        }

  

        return true;
    }

    private bool SlotMatches(Slot slot, Items Itemneeded)
    {
        if (Itemneeded == null)
        {
            return !slot.Hasitem();
        }
        return slot.Hasitem() && slot.GetItem() == Itemneeded;
    }
}
