using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    [SerializeField] private Camera playerCamera;

    public GameObject hotBrObj;



    public CraftinSystem craftingSystem;

  
    [SerializeField] private InputActionReference pickupobj;

    [SerializeField] private InputActionReference[] hotbarActions;
    [SerializeField] private InputActionReference dropAction;

    public Image DragIcon;

    public float pickupRange = 3f;

    private Material originalmaerial;
    private Renderer lookedAtRender = null;

    public GameObject Crafting;

    private int equippedHotBarIndex = 0;

    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    private Slot dragslot = null;
    private bool isDraggin = false;

    public Transform hand;

    [SerializeField] private GameObject axeHandItem;
    [SerializeField] private GameObject spearHandItem;
    [SerializeField] private GameObject hammerHandItem;
    [SerializeField] private GameObject knifeHandItem;
    [SerializeField] private GameObject FlintHandItem;
    [SerializeField] private GameObject RockHandItem;
    [SerializeField] private Items CrabItem;
    [SerializeField] private Items FlintItem;
    [SerializeField] private GameObject YellowMushroomHandItem;
    [SerializeField] private GameObject PurpleMushroomHandItem;
    [SerializeField] private GameObject RedMushroomHandItem;
    [SerializeField] private GameObject CrabHandItem;
    [SerializeField] private GameObject WoodHandItem;
    [SerializeField] private GameObject VineHandItem;
    [SerializeField] private GameObject WholeCoconutHandItem;
    [SerializeField] private GameObject CoconutHandItem;

    [SerializeField] private Items RockItem;
    private GameObject currentHandItem;

   
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();
    private List<Slot> craftingSlots = new List<Slot>();

  

   
    [SerializeField] private TextMeshProUGUI interactionMessage;

    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageCoroutine;



    private void Awake()
    {
       

        hotbarSlots.AddRange(
            hotBrObj.GetComponentsInChildren<Slot>(true)
        );

        craftingSlots.AddRange(
            Crafting.GetComponentsInChildren<Slot>(true)
        );

    
        allSlots.AddRange(hotbarSlots);

    }


    public void ShowInteractionMessage(string message)
    {
        if (interactionMessage == null)
            return;

        interactionMessage.text = message;

  
        interactionMessage.gameObject.SetActive(true);

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(HideInteractionMessage());
    }

    private IEnumerator HideInteractionMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        if (interactionMessage != null)
        {
      
            interactionMessage.text = "";
        }

        messageCoroutine = null;
    }



   
private void EquippedHandItem()
    {


   
        axeHandItem.SetActive(false);
        spearHandItem.SetActive(false);
        hammerHandItem.SetActive(false);
        knifeHandItem.SetActive(false);
        FlintHandItem.SetActive(false);
        RockHandItem.SetActive(false);
        YellowMushroomHandItem.SetActive(false);
        PurpleMushroomHandItem.SetActive(false);
        RedMushroomHandItem.SetActive (false);
        CrabHandItem.SetActive(false);
        WoodHandItem.SetActive(false);
        VineHandItem.SetActive(false);
        WholeCoconutHandItem.SetActive(false);
        CoconutHandItem.SetActive(false);


        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
        {
            return;
        }

        Slot currentSlot =
            hotbarSlots[equippedHotBarIndex];

       
        if (!currentSlot.Hasitem())
        {
       
            return;
        }

        Items item =
            currentSlot.GetItem();

        if (item == null)
            return;

        if (item == AxeItem)
        {
            axeHandItem.SetActive(true);
    
        }

        
        else if (item == Spear)
        {
            spearHandItem.SetActive(true);
   
        }

     
        else if (item == Hammer)
        {
            hammerHandItem.SetActive(true);
          
        }

 
        else if (item == Knife)
        {
            knifeHandItem.SetActive(true);
           
        }
        else if (item == FlintItem)
        {
            FlintHandItem.SetActive(true);

        }
        else if (item == RockItem)
        {
            RockHandItem.SetActive(true);

        }
        else if (item == YMushroom)
        {
            YellowMushroomHandItem.SetActive(true);

        }
        else if (item == PMushroom)
        {
            PurpleMushroomHandItem.SetActive(true);

        }
        else if (item == RMushroom)
        {
            RedMushroomHandItem.SetActive(true);

        }
        else if (item == CrabItem)
        {
            CrabHandItem.SetActive(true);

        }
        else if (item == WoodItem)
        {
            WoodHandItem.SetActive(true);

        }
        else if (item == Vines)
        {
            VineHandItem.SetActive(true);

        }
        else if (item == WholeCoconut)
        {
            WholeCoconutHandItem.SetActive(true);

        }
        else if (item == Coconut)
        {
            CoconutHandItem.SetActive(true);

        }
       

    }




    private void OnEnable()
    {
       
        pickupobj.action.Enable();

        foreach (InputActionReference action in hotbarActions)
        {
            action.action.Enable();
        }

        dropAction.action.Enable();
    }

    private void OnDisable()
    {

        pickupobj.action.Disable();

        foreach (InputActionReference action in hotbarActions)
        {
            action.action.Disable();
        }

        dropAction.action.Disable();
    }

    

    public Items GetHotbarItem()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
        {
            return null;
        }

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return null;

        return equippedSlot.GetItem();
    }

 

    public void RemoveHotbarItem(int amount)
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
        {
            return;
        }

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return;

        equippedSlot.RemoveAmount(amount);

        EquippedHandItem();
    }

 

    public bool IsHammerEquipped()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return false;

        return equippedSlot.GetItem() == Hammer;
    }

    public bool IsAxeEquipped()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return false;

        return equippedSlot.GetItem() == AxeItem;
    }

    public bool IsKnifeEquipped()
    {
        Items equippedItem = GetHotbarItem();

        if (equippedItem == null)
            return false;

        if (Knife == null)
            return false;

        return equippedItem == Knife;
    }

    public bool IsSpearEquipped()
    {
        Items equippedItem = GetHotbarItem();

        if (equippedItem == null)
            return false;

        return equippedItem == Spear;
    }

    public bool IsFlintEquipped(Items flintItem)
    {
        Items equippedItem = GetHotbarItem();

        if (equippedItem == null)
            return false;

        return equippedItem == flintItem;
    }



    private void Update()
    {
   

        DetectLookedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotBarSelection();
        HandleDropEquippedItem();
        UpdateHotBarOpacity();
    }



    public int GetTotalItemAmount(Items itemToCheck)
    {
        int total = 0;

        foreach (Slot slot in hotbarSlots)
        {
            if (slot.Hasitem() &&
                slot.GetItem() == itemToCheck)
            {
                total += slot.GetAmount();
            }
        }

    

        return total;
    }


    public int RemoveItemAmount(
        Items itemToRemove,
        int amount)
    {
        int remaining = amount;

        foreach (Slot slot in hotbarSlots)
        {
            if (remaining <= 0)
                break;

            if (slot.Hasitem() &&
                slot.GetItem() == itemToRemove)
            {
                int amountInSlot = slot.GetAmount();

                int amountToRemove =
                    Mathf.Min(amountInSlot, remaining);

                slot.RemoveAmount(amountToRemove);

                remaining -= amountToRemove;
            }
        }



        EquippedHandItem();

        return amount - remaining;
    }


    public bool AddItem(Items itemToAdd, int amount)
    {
        int remaining = amount;

        foreach (Slot slot in hotbarSlots)
        {
            if (slot.Hasitem() &&
                slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStack;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;

                    int amountToAdd =
                        Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(
                        itemToAdd,
                        currentAmount + amountToAdd
                    );

                    remaining -= amountToAdd;

                    if (remaining <= 0)
                    {
                        EquippedHandItem();
                        return true;
                    }
                }
            }
        }


        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.Hasitem())
            {
                int amountToPlace =
                    Mathf.Min(
                        itemToAdd.maxStack,
                        remaining
                    );

                slot.SetItem(
                    itemToAdd,
                    amountToPlace
                );

                equippedHotBarIndex =
                    hotbarSlots.IndexOf(slot);

                remaining -= amountToPlace;

                EquippedHandItem();

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        EquippedHandItem();
        return false;
    }

    private void StartDrag()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null &&
                hovered.Hasitem())
            {
                dragslot = hovered;
                isDraggin = true;

                DragIcon.sprite =
                    hovered.GetItem().icon;

                DragIcon.color =
                    new Color(1, 1, 1, 0.5f);

                DragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasReleasedThisFrame &&
            isDraggin)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null)
            {
                Slot originalSlot = dragslot;

                HandleDrop(
                    dragslot,
                    hovered
                );

                if (craftingSlots.Contains(hovered) ||
                    craftingSlots.Contains(originalSlot))
                {
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
                return s;
        }

        foreach (Slot s in craftingSlots)
        {
            if (s.hovering)
                return s;
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
            return;

        if (from == craftingSystem.resultSlot)
        {
            TryTakeCraftedItem(to);
            return;
        }

        if (craftingSlots.Contains(to))
        {
            if (to.Hasitem())
                return;

            to.SetItem(
                from.GetItem(),
                1
            );

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
                    int move =
                        Mathf.Min(
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
                int move =
                    Mathf.Min(
                        space,
                        from.GetAmount()
                    );

                to.SetItem(
                    to.GetItem(),
                    to.GetAmount() + move
                );

                from.SetItem(
                    from.GetItem(),
                    from.GetAmount() - move
                );

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

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
        if (isDraggin &&
            Mouse.current != null)
        {
            DragIcon.transform.position =
                Mouse.current.position.ReadValue();
        }
    }

    

    private void Pickup()
    {
        if (lookedAtRender != null &&
            pickupobj.action.WasPressedThisFrame())
        {
            GroundItem item =
                lookedAtRender.GetComponent<GroundItem>();

            if (item != null)
            {
                bool pickedUp =
                    AddItem(
                        item.item,
                        item.amount
                    );

                if (pickedUp)
                {
                    ResourceRespawn respawn =
                        item.GetComponent<ResourceRespawn>();

                    if (respawn != null)
                    {
                        respawn.RespawnResource();
                    }
                    else
                    {
                        Destroy(item.gameObject);
                        EquippedHandItem();
                    }
                }
            }
        }
    }



    private void DetectLookedAtItem()
    {
        if (lookedAtRender != null)
        {
            lookedAtRender.material =
                originalmaerial;

            lookedAtRender = null;
            originalmaerial = null;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            pickupRange))
        {
            GroundItem item =
                hit.collider.GetComponent<GroundItem>();

            if (item != null)
            {
                Renderer rend =
                    item.GetComponent<Renderer>();

                if (rend != null)
                {
                    originalmaerial =
                        rend.material;

                    lookedAtRender = rend;
                }
            }
        }
    }

 

    private void TryTakeCraftedItem(Slot destination)
    {
        if (craftingSystem == null)
            return;

        if (destination == null)
            return;

        Slot resultSlot =
            craftingSystem.resultSlot;

        if (resultSlot == null)
            return;

        Items craftedItem =
            resultSlot.GetItem();

        int craftedAmount =
            resultSlot.GetAmount();

        if (!destination.Hasitem())
        {
            destination.SetItem(
                craftedItem,
                craftedAmount
            );
        }
        else if (destination.GetItem() == craftedItem)
        {
            int max =
                craftedItem.maxStack;

            int space =
                max - destination.GetAmount();

            if (space < craftedAmount)
                return;

            destination.AddAmount(
                craftedAmount
            );
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
        for (int i = 0;
            i < hotbarSlots.Count;
            i++)
        {
            hotbarSlots[i].SelectedFrame(
                i == equippedHotBarIndex
            );
        }
    }

    private void HandleHotBarSelection()
    {
        for (int i = 0;
            i < hotbarActions.Length;
            i++)
        {
            if (hotbarActions[i]
                .action
                .WasPressedThisFrame())
            {
                equippedHotBarIndex = i;

                UpdateHotBarOpacity();
                EquippedHandItem();
            }
        }
    }



    private void HandleDropEquippedItem()
    {
        if (!dropAction.action.WasPressedThisFrame())
            return;

        Slot equippedSlot =
            hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return;

        Items item =
            equippedSlot.GetItem();

        GameObject prefab =
            item.ItenPrefab;

        if (prefab == null)
            return;

        if (GroundItemManager.Instance != null)
        {
            if (!GroundItemManager.Instance.CanSpawn(prefab))
                return;
        }

        GameObject dropped =
            Instantiate(
                prefab,
                Camera.main.transform.position +
                Camera.main.transform.forward,
                Quaternion.identity
            );

        GroundItem item1 =
            dropped.GetComponent<GroundItem>();

        if (item1 != null)
        {
            item1.item = item;
            item1.amount =
                equippedSlot.GetAmount();

            item1.sourcePrefab = prefab;
        }

        equippedSlot.ClearSlot();

        EquippedHandItem();
    }
}