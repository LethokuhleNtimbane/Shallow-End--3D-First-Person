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

        eatFood();
    }
    private void eatFood()
    {
        if (inventory == null) return;

        if (healthScript == null) return;

        if (healthScript.playerIsFullHealth())return;
        

        Items hotbarItem = inventory.GetHotbarItem();

        if (hotbarItem == null) return;

        if (!isItFood(hotbarItem)) return;

        healthScript.Addhealth(foodIncrease);
        inventory.RemoveHotbarItem(1);
    }

    private bool isItFood(Items item)
    {
        foreach (Items food in foodItems)
        {
            if (item == food) return true;
        }
        return false;
    }
}
