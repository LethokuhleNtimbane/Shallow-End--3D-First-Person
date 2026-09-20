using UnityEngine;
using UnityEngine.InputSystem;

public class FoodScript : MonoBehaviour
{
    [SerializeField] private InputActionReference eatAct;
    [SerializeField] private Inventory inventory;
    [SerializeField] private HealthScript healthScript;

    [System.Serializable]
    public class FoodItem
    {
        public Items item;

        [Header("Food Values")]
        public float hungerIncrease;
        public float thirstIncrease;
    }

    [SerializeField] private FoodItem[] foodItems;

    private void OnEnable()
    {
        if (eatAct != null)
        {
            eatAct.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (eatAct != null)
        {
            eatAct.action.Disable();
        }
    }

    private void Update()
    {
        if (!eatAct.action.WasPressedThisFrame())
            return;

        EatFood();
    }

    private void EatFood()
    {
        if (inventory == null)
            return;

        if (healthScript == null)
            return;

        Items hotbarItem = inventory.GetHotbarItem();

        if (hotbarItem == null)
            return;

        FoodItem food = GetFoodData(hotbarItem);

        if (food == null)
            return;

        // If the food increases hunger and hunger is already full,
        // do not allow the player to eat it.
        if (food.hungerIncrease > 0 &&
            healthScript.PlayerIsFullHunger())
        {
            return;
        }

        // If the food increases thirst and thirst is already full,
        // do not allow the player to consume it.
        if (food.thirstIncrease > 0 &&
            healthScript.PlayerIsFullThirst())
        {
            return;
        }

        // Add the food's hunger and thirst values.
        healthScript.AddHunger(food.hungerIncrease);
        healthScript.AddThirst(food.thirstIncrease);

        // Remove one item from the hotbar.
        inventory.RemoveHotbarItem(1);
    }

    private FoodItem GetFoodData(Items item)
    {
        foreach (FoodItem food in foodItems)
        {
            if (food.item == item)
            {
                return food;
            }
        }

        return null;
    }
}