
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickCraftButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private CraftinSystem craftingSystem;
    [SerializeField] private int recipeIndex = 0;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Button button;

    [Header("Colours")]
    [SerializeField]
    private Color availableColor =
        new Color(0.25f, 0.75f, 0.35f, 1f);

    [SerializeField]
    private Color unavailableColor =
        new Color(0.35f, 0.35f, 0.35f, 1f);

    private CraftingRecipe Recipe
    {
        get
        {
            if (craftingSystem == null ||
                craftingSystem.recipes == null ||
                recipeIndex < 0 ||
                recipeIndex >= craftingSystem.recipes.Count)
                return null;

            return craftingSystem.recipes[recipeIndex];
        }
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonImage == null && button != null)
            buttonImage = button.targetGraphic as Image;

        // Prevent Unity's normal button tint from overriding our colours.
        if (button != null)
            button.transition = Selectable.Transition.None;
    }

    private void OnEnable()
    {
        RefreshButton();
    }

    private void Update()
    {
        RefreshButton();
    }

    private Items[] GetIngredients()
    {
        CraftingRecipe recipe = Recipe;

        if (recipe == null)
            return new Items[0];

        return new Items[]
        {
            recipe.slotItem1,
            recipe.slotItem2,
            recipe.slotItem3,
            recipe.slotItem4
        };
    }

    private bool CanPrepareRecipe(bool logDetails = false)
    {
        CraftingRecipe recipe = Recipe;

        if (inventory == null || craftingSystem == null ||
            recipe == null || recipe.resultItem == null ||
            recipe.resultAmount <= 0 ||
            craftingSystem.craftinSlots == null ||
            craftingSystem.craftinSlots.Length < 4)
        {
            if (logDetails)
                Debug.LogWarning(
                    name + ": Missing reference, invalid recipe index, " +
                    "or invalid recipe result.", this);

            return false;
        }

        foreach (Slot slot in craftingSystem.craftinSlots)
        {
            if (slot == null || slot.Hasitem())
            {
                if (logDetails)
                    Debug.Log(name +
                        ": Cannot quick craft because the crafting grid " +
                        "is occupied or a slot is unassigned.", this);

                return false;
            }
        }

        Dictionary<Items, int> required =
            new Dictionary<Items, int>();

        foreach (Items item in GetIngredients())
        {
            if (item == null)
                continue;

            if (!required.ContainsKey(item))
                required[item] = 0;

            required[item]++;
        }

        if (required.Count == 0)
        {
            if (logDetails)
                Debug.LogWarning(
                    name + ": This recipe has no ingredients.", this);

            return false;
        }

        bool canCraft = true;

        foreach (KeyValuePair<Items, int> entry in required)
        {
            int available =
                inventory.GetTotalItemAmount(entry.Key);

            if (logDetails)
            {
                Debug.Log(
                    name + " | Recipe: " + recipe.recipeName +
                    " | Ingredient: " + entry.Key.name +
                    " | Required: " + entry.Value +
                    " | Available: " + available,
                    this);
            }

            if (available < entry.Value)
                canCraft = false;
        }

        return canCraft;
    }

    public void RefreshButton()
    {
        if (buttonImage == null)
            return;

        buttonImage.color = CanPrepareRecipe()
            ? availableColor
            : unavailableColor;
    }

    public void CraftButtonPressed()
    {
        if (!CanPrepareRecipe(true))
        {
            Debug.Log(name + ": Quick crafting unavailable.", this);
            RefreshButton();
            return;
        }

        Items[] ingredients = GetIngredients();

        // Remove all required ingredients before filling the grid.
        foreach (Items item in ingredients)
        {
            if (item == null)
                continue;

            int removed = inventory.RemoveItemAmount(item, 1);

            if (removed != 1)
            {
                Debug.LogError(
                    "Could not remove " + item.name +
                    " while preparing the recipe.", this);

                return;
            }
        }

        for (int i = 0; i < ingredients.Length; i++)
        {
            if (ingredients[i] == null)
                continue;

            craftingSystem.craftinSlots[i].SetItem(
                ingredients[i], 1);
        }

        craftingSystem.UpdateCraftingResult();

        Debug.Log(
            "Prepared recipe: " + Recipe.recipeName, this);

        RefreshButton();
    }
}
