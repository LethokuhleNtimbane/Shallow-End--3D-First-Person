using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Items WoodItem;
    public Items Spear;
    public Items Hammer;
    public Items RMushroom;
    public Items YMushroom;
    public Items PMushroom;
    public Items AxeItem;
    public GameObject hotBrObj;
    public GameObject inventorySlotParent;

    public GameObject container;


    [SerializeField] private InputActionReference OpenInventory;
    [SerializeField] private InputActionReference pickupobj;

    [SerializeField] private InputActionReference[] hotbarActions;
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private InputActionReference hotbarScroll;
    public Image DragIcon;
    public float pickupRange = 3f;
    private GroundItem lookedAtItem = null;
   
    private Material originalmaerial;
    private Renderer lookedAtRender = null;

    private int equippedHotBarIndex = 0;

    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;
    private Slot dragslot = null;
    private bool isDraggin = false;

    private List<Slot> InventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake()
    {
        InventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotBrObj.GetComponentsInChildren<Slot>());

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
        hotbarScroll.action.Enable();
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
        hotbarScroll.action.Disable();
    }
    // Update is called once per frame
    void Update()
    {

        if (OpenInventory.action.WasPressedThisFrame())
        {
            container.SetActive(!container.activeInHierarchy);
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            PlayerController.Instance.updateingRotation = !container.activeInHierarchy;

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

    public void AddItem(Items itemToAdd, int amount)
    {
        int remaining = amount;

        foreach (Slot slot in allSlots)
        {
            if (slot.Hasitem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStack;

                if (currentAmount < maxStack)
                {
                    int spaceleft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceleft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                    {
                        return;
                    }
                }
            }
        }
        foreach (Slot slot in allSlots)
        {
            if (!slot.Hasitem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStack, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                {
                    return;
                }
            }
        }
        if (remaining > 0)
        {

        }
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
                HandleDrop(dragslot, hovered);

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
        return null;
    }
    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to)
            return;

        // Destination is empty
        if (!to.Hasitem())
        {
            to.SetItem(from.GetItem(), from.GetAmount());
            from.ClearSlot();
            return;
        }

        // Destination has the same item - stack
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

        // Destination has a different item - swap
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
                AddItem(item.item, item.amount);
                Destroy(item.gameObject);
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
 Vector2 scroll = hotbarScroll.action.ReadValue<Vector2>();

    if (scroll.y > 0)
    {
        equippedHotBarIndex++;

        if (equippedHotBarIndex >= hotbarSlots.Count)
        {
            equippedHotBarIndex = 0;
        }

        UpdateHotBarOpacity();
    }
    else if (scroll.y < 0)
    {
        equippedHotBarIndex--;

        if (equippedHotBarIndex < 0)
        {
            equippedHotBarIndex = hotbarSlots.Count - 1;
        }

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