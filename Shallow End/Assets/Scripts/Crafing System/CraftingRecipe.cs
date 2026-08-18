using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftingRecipe 
{
    public string recipeName;

    public Items slotItem1;
    public Items slotItem2;
    public Items slotItem3;
    public Items slotItem4;

    public Items resultItem;
    public int resultAmount = 1;
}
