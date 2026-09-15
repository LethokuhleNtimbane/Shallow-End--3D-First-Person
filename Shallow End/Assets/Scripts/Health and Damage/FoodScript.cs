using UnityEngine;
using UnityEngine.InputSystem;

public class FoodScript : MonoBehaviour
{
    [SerializeField] private InputActionReference eatAct;
    [SerializeField] private Inventory inventory;
    [SerializeField] private HealthScript healthScript;

    [SerializeField] private Items[] foodItems;
    [SerializeField] private float foodIncrease = 20f;

    private void OnEnable()
    {
        eatAct.action.Enable();
    }

    private void OnDisable()
    {
        eatAct.action.Disable();
    }

    private void Update()
    {
        if (!eatAct.action.WasPressedThisFrame()) return;

        EatFood();
    }

    private void EatFood()
    {
        if (inventory == null) return;

        if (healthScript == null) return;

        if (healthScript.PlayerIsFullHealth()) return;

        Items hotbarItem = inventory.GetHotbarItem();

        if (hotbarItem == null) return;

        if (!IsItFood(hotbarItem)) return;

        healthScript.AddHealth(foodIncrease);

        inventory.RemoveHotbarItem(1);
    }

    private bool IsItFood(Items item)
    {
        foreach (Items food in foodItems)
        {
            if (item == food) return true;
        }

        return false;
    }
}