using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Items WoodItem;
    public Items Spear;
    public Items Hammer;
    public Items Knife;
    public Items Vines;
    public Items WholeCoconut;
    public Items Coconut;
    public Items RMushroom;
    public Items YMushroom;
    public Items PMushroom;
    public Items AxeItem;
    public GameObject hotBrObj;
    public GameObject inventorySlotParent;

    public GameObject container;
    public CraftinSystem craftingSystem;

    [SerializeField] private InputActionReference OpenInventory;
    [SerializeField] private InputActionReference pickupobj;

    [SerializeField] private InputActionReference[] hotbarActions;
    [SerializeField] private InputActionReference dropAction;

    public Image DragIcon;
    public float pickupRange = 3f;
    private GroundItem lookedAtItem = null;
   
    private Material originalmaerial;
    private Renderer lookedAtRender = null;

    public GameObject Crafting;

    private int equippedHotBarIndex = 0;

    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;
    private Slot dragslot = null;
    private bool isDraggin = false;

    private List<Slot> InventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();
    private List<Slot> craftingSlots = new List<Slot>();

    private void Awake()
    {
        InventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotBrObj.GetComponentsInChildren<Slot>());
        craftingSlots.AddRange(Crafting.GetComponentsInChildren<Slot>());

        allSlots.AddRange(InventorySlots);
        allSlots.AddRange(hotbarSlots);
      
    }
    private void OnEnable()
    {
        OpenInventory.action.Enable();
        pickupobj.action.Enable();

        foreach (InputActionReference action in hotbarActions)
        {
            action.action.Enable();
        }

        dropAction.action.Enable();
     
    }
    public Items GetHotbarItem()
    {
        if (equippedHotBarIndex < 0 || equippedHotBarIndex >= hotbarSlots.Count)
            return null;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return null;

        return equippedSlot.GetItem();
    }
    public void RemoveHotbarItem(int amount)
    {
        if (equippedHotBarIndex < 0 || equippedHotBarIndex >= hotbarSlots.Count)
            return;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return;

        equippedSlot.RemoveAmount(amount);
    }
    public bool IsHammerEquipped()
    {
        if (equippedHotBarIndex < 0 || equippedHotBarIndex >= hotbarSlots.Count) return false;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem()) return false;

        return equippedSlot.GetItem() == Hammer;
    }
    public bool IsAxeEquipped()
    {
        if (equippedHotBarIndex < 0 || equippedHotBarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return false;

        return equippedSlot.GetItem() == AxeItem;
    }
    private void OnDisable()
    {
        OpenInventory.action.Disable();
        pickupobj.action.Disable();

        foreach (InputActionReference action in hotbarActions)
        {
            action.action.Disable();
        }

        dropAction.action.Disable();
       
    }
    // Update is called once per frame
    void Update()
    {

        if (OpenInventory.action.WasPressedThisFrame())
        {
            container.SetActive(!container.activeInHierarchy);

            bool inventoryOpen = container.activeInHierarchy;

            Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;

            Cursor.visible = inventoryOpen;

            UIManager.Instance.SetInventoryOpen(inventoryOpen);

        }
        DetectLookedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotBarSelection();
        HandleDropEquippedItem();
        UpdateHotBarOpacity();
    }

    public bool AddItem(Items itemToAdd, int amount)
    {
        int remaining = amount;

        foreach (Slot slot in hotbarSlots)
        {
            if (slot.Hasitem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStack;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);

                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return true;
                }
            }
        }

        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.Hasitem())
            {
                int amountToPlace = Mathf.Min(
                    itemToAdd.maxStack,
                    remaining
                );

                slot.SetItem(itemToAdd, amountToPlace);

                remaining -= amountToPlace;

                if (remaining <= 0)
                    return true;
            }
        }

        foreach (Slot slot in InventorySlots)
        {
            if (slot.Hasitem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStack;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);

                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return true;
                }
            }
        }
        foreach (Slot slot in InventorySlots)
        {
            if (!slot.Hasitem())
            {
                int amountToPlace = Mathf.Min(
                    itemToAdd.maxStack,
                    remaining
                );

                slot.SetItem(itemToAdd, amountToPlace);

                remaining -= amountToPlace;

                if (remaining <= 0)
                    return true;
            }
        }
        return false;
    }
    private void StartDrag()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.Hasitem())
            {
                dragslot = hovered;

                isDraggin = true;

                DragIcon.sprite = hovered.GetItem().icon;
                DragIcon.color = new Color(1, 1, 1, 0.5f);
                DragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame && isDraggin)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null)
            {
                Slot originalSlot = dragslot;
                HandleDrop(dragslot, hovered);

                if (craftingSlots.Contains(hovered) || craftingSlots.Contains(originalSlot))
                {
                    Debug.Log("Checking crafting recipe...");
                    craftingSystem.UpdateCraftingResult();
                }
                DragIcon.enabled = false;

                dragslot = null;
                isDraggin = false;
            }
        }
    }
    private Slot GetHoveredSlot()
    {
        foreach (Slot s in allSlots)
        {
            if (s.hovering)
            {
               
                return s;
            }
        }

        foreach (Slot s in craftingSlots)
        {
            if (s.hovering)
            {
             
                return s;
            }
        }

        if (craftingSystem != null &&
            craftingSystem.resultSlot != null &&
            craftingSystem.resultSlot.hovering)
        {
        
            return craftingSystem.resultSlot;
        }

       

        return null;
    }
    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to)
            return;

        if (to == craftingSystem.resultSlot)
        {
            return;
        }

        if (from == craftingSystem.resultSlot)
        {
            TryTakeCraftedItem(to);
            return;
        }

        if (craftingSlots.Contains(to))
        {

            if (to.Hasitem())
                return;


            to.SetItem(from.GetItem(), 1);


            from.RemoveAmount(1);

            return;
        }
        if (craftingSlots.Contains(from))
        {

            if (!to.Hasitem())
            {
                to.SetItem(
                    from.GetItem(),
                    from.GetAmount()
                );

                from.ClearSlot();

                return;
            }
            if (to.GetItem() == from.GetItem())
            {
                int max = to.GetItem().maxStack;
                int space = max - to.GetAmount();

                if (space > 0)
                {
                    int move = Mathf.Min(
                        space,
                        from.GetAmount()
                    );

                    to.SetItem(
                        to.GetItem(),
                        to.GetAmount() + move
                    );

                    from.RemoveAmount(move);
                }

                return;
            }

            return;
        }


        if (!to.Hasitem())
            {
                to.SetItem(from.GetItem(), from.GetAmount());
                from.ClearSlot();
                return;
            }

            if (to.GetItem() == from.GetItem())
            {
                int max = to.GetItem().maxStack;
                int space = max - to.GetAmount();

                if (space > 0)
                {
                    int move = Mathf.Min(space, from.GetAmount());

                    to.SetItem(
                        to.GetItem(),
                        to.GetAmount() + move
                    );

                    from.SetItem(
                        from.GetItem(),
                        from.GetAmount() - move
                    );

                    if (from.GetAmount() <= 0)
                    {
                        from.ClearSlot();
                    }

                    return;
                }
            }

            Items tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(
                from.GetItem(),
                from.GetAmount()
            );

            from.SetItem(
                tempItem,
                tempAmount
            );
        }
    
    
    private void UpdateDragItemPosition()
    {
        if (isDraggin && Mouse.current != null)
        {

            DragIcon.transform.position = Mouse.current.position.ReadValue();
        }
    }

    private void Pickup()
    {
        if (lookedAtRender != null && pickupobj.action.WasPressedThisFrame())
        {
            GroundItem item = lookedAtRender.GetComponent<GroundItem>();
            if (item != null)
            {
                bool pickedUp = AddItem(item.item, item.amount);

                if (pickedUp)
                {
                    Destroy(item.gameObject);
                }
            }
        }
    }
    private void DetectLookedAtItem()
    {
        if (lookedAtRender != null)
        {
            lookedAtRender.material = originalmaerial;
            lookedAtRender = null;
            originalmaerial = null;

        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            GroundItem item = hit.collider.GetComponent<GroundItem>();
            if (item != null)
            {
                Renderer rend = item.GetComponent<Renderer>();
                if (rend != null)
                {
                    originalmaerial = rend.material;
                 
                    lookedAtRender = rend;
                }
            }
        }
    }
    private void TryTakeCraftedItem(Slot destination)
    {
        if (craftingSystem == null) return;
        if (destination == null) return;

        Slot resultSlot = craftingSystem.resultSlot;

        if (resultSlot == null) return;

        Items craftedItem = resultSlot.GetItem();
        int craftedAmount = resultSlot.GetAmount();

        if (!destination.Hasitem())
        {
            destination.SetItem(craftedItem, craftedAmount);
        }

        else if (destination.GetItem() == craftedItem)
        {
            int maxStack = craftedItem.maxStack;
            int space = maxStack - destination.GetAmount();

            if (space < craftedAmount) return;

            destination.AddAmount(craftedAmount);
        }
        else
        {
            return;
        }
        UseCraftingIngredients();
        resultSlot.ClearSlot();
        craftingSystem.UpdateCraftingResult();
    }
    private void UseCraftingIngredients()
    {
        foreach (Slot slot in craftingSlots)
        {
            if (slot.Hasitem())
            {
                slot.RemoveAmount(1);
            }
        }
    }
    private void UpdateHotBarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image Icon = hotbarSlots[i].GetComponent<Image>();
            if (Icon != null)
            {
                Icon.color = (i == equippedHotBarIndex) ? new Color(1, 1, 1, equippedOpacity) : new Color(1, 1, 1, normalOpacity);
            }

        }
    }

    private void HandleHotBarSelection()
    {
        for (int i = 0; i < hotbarActions.Length; i++)
        {
            if (hotbarActions[i].action.WasPressedThisFrame())
            {
                equippedHotBarIndex = i;
                UpdateHotBarOpacity();
            }

        }
    }
    private void HandleDropEquippedItem()
    {
        if (!dropAction.action.WasPressedThisFrame()) return;

            Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

            if (!equippedSlot.Hasitem()) return;

            Items item = equippedSlot.GetItem();
        GameObject prefab = item.ItenPrefab;

        if (prefab == null) return;

        GameObject dropped = Instantiate(prefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);

        GroundItem item1 = dropped.GetComponent<GroundItem>();
        item1.item = item;
        item1.amount = equippedSlot.GetAmount();

        equippedSlot.ClearSlot();
    }
}