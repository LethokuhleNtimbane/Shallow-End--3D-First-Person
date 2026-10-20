using UnityEngine;

[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;

    [Header("Required Crafting Slots")]
    public Items slotItem1;
    public Items slotItem2;
    public Items slotItem3;
    public Items slotItem4;

    [Header("Crafting Result")]
    public Items resultItem;
    public int resultAmount = 1;
}