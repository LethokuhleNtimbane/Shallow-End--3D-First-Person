using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("Item variables")]
    public Items WoodItem;
    public Items Spear;
    public Items Hammer;
    public Items Knife;
    public Items Vines;
    public Items WholeCoconut;
    public Items Coconut;
    public Items AxeItem;
    public Items FishingRodItem;
    public Items Fish;
    public Items Mushroom;
    [SerializeField] private Items CrabItem;
    [SerializeField] private Items FlintItem;
    [SerializeField] private Items RockItem;
    [SerializeField] private Items Bucket;
    [SerializeField] private Items WaterBucket;
    [SerializeField] private Items CookedFish;
    [SerializeField] private Items CookedCrab;
    [SerializeField] private Items BoiledWaterBucket;

    [Header("Input Action")]
    [SerializeField] private InputActionReference pickupobj;
    [SerializeField] private InputActionReference[] hotbarActions;
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private InputActionReference hotbarScroll;

    public float pickupRange = 3f;

    private Material originalmaerial;
    private Renderer lookedAtRender = null;
    private int equippedHotBarIndex = 0;

    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    [Header("In hand item")]
    public Transform hand;
    [SerializeField] private GameObject axeHandItem;
    [SerializeField] private GameObject bucket;
    [SerializeField] private GameObject waterbucket;
    [SerializeField] private GameObject spearHandItem;
    [SerializeField] private GameObject hammerHandItem;
    [SerializeField] private GameObject knifeHandItem;
    [SerializeField] private GameObject FlintHandItem;
    [SerializeField] private GameObject RockHandItem;
    [SerializeField] private GameObject CrabHandItem;
    [SerializeField] private GameObject WoodHandItem;
    [SerializeField] private GameObject VineHandItem;
    [SerializeField] private GameObject WholeCoconutHandItem;
    [SerializeField] private GameObject CoconutHandItem;
    [SerializeField] private GameObject fishingRodHandItem;
    [SerializeField] private GameObject CookedFishHandItem;
    [SerializeField] private GameObject CookedCrabHandItem;
    [SerializeField] private GameObject BoiledWaterBucketHandItem;
    public GameObject fish;
    public GameObject mushroom;
    private GameObject currentHandItem;

    private AudioManager audioManager;

    [Header("Slots")]
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();
    private List<Slot> craftingSlots = new List<Slot>();
    private List<Slot> storageSlots = new List<Slot>();

    private Slot fireplaceFuelSlot;
    private Slot fireplaceCookingSlot;

    private Items fireplaceWoodItem;
    private Items fireplaceFishItem;
    private Items fireplaceCrabItem;
    private Items fireplaceWaterItem;

    [Header("UI elements")]
    [SerializeField] private float messageDuration = 2f;
    [SerializeField] private TextMeshProUGUI interactionMessage;
    [SerializeField] private CanvasGroup dropImageCanvasGroup;
    [SerializeField] private float dropImageFadeDuration = 0.3f;
    [SerializeField] private float dropImageVisibleDuration = 0.4f;

    public Image DragIcon;
    private Slot dragslot = null;
    private bool isDraggin = false;
    private Coroutine messageCoroutine;
    private Coroutine dropImageCoroutine;

    public GameObject Crafting;
    public GameObject hotBar;
    public CraftinSystem craftingSystem;

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }

        hotbarSlots.AddRange(hotBar.GetComponentsInChildren<Slot>(true));
        craftingSlots.AddRange(Crafting.GetComponentsInChildren<Slot>(true));
        allSlots.AddRange(hotbarSlots);
    }

    public void RegisterStorageSlots(List<Slot> slots)
    {
        storageSlots.Clear();

        if (slots == null)
            return;

        foreach (Slot slot in slots)
        {
            if (slot != null && !storageSlots.Contains(slot))
            {
                storageSlots.Add(slot);
            }
        }
    }

    public void UnregisterStorageSlots()
    {
        storageSlots.Clear();

        if (Mouse.current != null && DragIcon != null)
        {
            DragIcon.enabled = false;
        }
    }

    public void RegisterFireplaceSlots(
        Slot fuelSlot,
        Slot cookingSlot,
        Items wood,
        Items fishItem,
        Items crabItem,
        Items waterItem)
    {
        fireplaceFuelSlot = fuelSlot;
        fireplaceCookingSlot = cookingSlot;
        fireplaceWoodItem = wood;
        fireplaceFishItem = fishItem;
        fireplaceCrabItem = crabItem;
        fireplaceWaterItem = waterItem;
    }

    public void UnregisterFireplaceSlots()
    {
        fireplaceFuelSlot = null;
        fireplaceCookingSlot = null;
        fireplaceWoodItem = null;
        fireplaceFishItem = null;
        fireplaceCrabItem = null;
        fireplaceWaterItem = null;

        if (DragIcon != null)
        {
            DragIcon.enabled = false;
        }

        dragslot = null;
        isDraggin = false;
    }

    private bool IsFireplaceSlot(Slot slot)
    {
        return slot != null &&
               (slot == fireplaceFuelSlot ||
                slot == fireplaceCookingSlot);
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
            interactionMessage.gameObject.SetActive(false);
        }

        messageCoroutine = null;
    }

    private void ShowDropImage()
    {
        if (dropImageCanvasGroup == null)
            return;

        if (dropImageCoroutine != null)
        {
            StopCoroutine(dropImageCoroutine);
        }

        dropImageCoroutine = StartCoroutine(DropImageFlash());
    }

    private IEnumerator DropImageFlash()
    {
        dropImageCanvasGroup.alpha = 0f;
        dropImageCanvasGroup.gameObject.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(
                FadeCanvasGroup(0f, 1f)
            );

            yield return new WaitForSecondsRealtime(
                dropImageVisibleDuration
            );

            yield return StartCoroutine(
                FadeCanvasGroup(1f, 0f)
            );

            if (i < 2)
            {
                yield return new WaitForSecondsRealtime(
                    dropImageVisibleDuration
                );
            }
        }

        dropImageCanvasGroup.alpha = 0f;
        dropImageCanvasGroup.gameObject.SetActive(false);

        dropImageCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < dropImageFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(elapsed / dropImageFadeDuration);

            dropImageCanvasGroup.alpha =
                Mathf.Lerp(startAlpha, endAlpha, progress);

            yield return null;
        }

        dropImageCanvasGroup.alpha = endAlpha;
    }

    private void EquippedHandItem()
    {
        axeHandItem.SetActive(false);
        spearHandItem.SetActive(false);
        hammerHandItem.SetActive(false);
        knifeHandItem.SetActive(false);
        fish.SetActive(false);
        mushroom.SetActive(false);
        FlintHandItem.SetActive(false);
        RockHandItem.SetActive(false);
        fishingRodHandItem.SetActive(false);
        bucket.SetActive(false);
        waterbucket.SetActive(false);
        CrabHandItem.SetActive(false);
        WoodHandItem.SetActive(false);
        VineHandItem.SetActive(false);
        WholeCoconutHandItem.SetActive(false);
        CoconutHandItem.SetActive(false);
        CookedFishHandItem.SetActive(false);
        CookedCrabHandItem.SetActive(false);
        BoiledWaterBucketHandItem.SetActive(false);

        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
        {
            return;
        }

        Slot currentSlot = hotbarSlots[equippedHotBarIndex];

        if (!currentSlot.Hasitem())
            return;

        Items item = currentSlot.GetItem();

        if (item == null)
            return;

        if (item == AxeItem)
            axeHandItem.SetActive(true);

        if (item == Mushroom)
            mushroom.SetActive(true);

        if (item == Bucket)
            bucket.SetActive(true);

        if (item == WaterBucket)
            waterbucket.SetActive(true);

        if (item == Fish)
            fish.SetActive(true);

        if (item == CookedFish)
            CookedFishHandItem.SetActive(true);

        if (item == CookedCrab)
            CookedCrabHandItem.SetActive(true);

        if (item == BoiledWaterBucket)
            BoiledWaterBucketHandItem.SetActive(true);

        if (item == Spear)
            spearHandItem.SetActive(true);
        else if (item == Hammer)
            hammerHandItem.SetActive(true);
        else if (item == Knife)
            knifeHandItem.SetActive(true);
        else if (item == FlintItem)
            FlintHandItem.SetActive(true);
        else if (item == RockItem)
            RockHandItem.SetActive(true);
        else if (item == CrabItem)
            CrabHandItem.SetActive(true);
        else if (item == WoodItem)
            WoodHandItem.SetActive(true);
        else if (item == Vines)
            VineHandItem.SetActive(true);
        else if (item == WholeCoconut)
            WholeCoconutHandItem.SetActive(true);
        else if (item == Coconut)
            CoconutHandItem.SetActive(true);
        else if (item == FishingRodItem)
            fishingRodHandItem.SetActive(true);
    }

    private void HandleHotBarScroll()
    {
        if (hotbarScroll == null)
            return;

        Vector2 scrollValue = hotbarScroll.action.ReadValue<Vector2>();

        if (scrollValue.y > 0)
        {
            if (equippedHotBarIndex > 0)
            {
                equippedHotBarIndex--;
                UpdateHotBarOpacity();
                EquippedHandItem();
            }
        }
        else if (scrollValue.y < 0)
        {
            if (equippedHotBarIndex < hotbarSlots.Count - 1)
            {
                equippedHotBarIndex++;
                UpdateHotBarOpacity();
                EquippedHandItem();
            }
        }
    }

    private void OnEnable()
    {
        if (pickupobj != null)
            pickupobj.action.Enable();

        foreach (InputActionReference action in hotbarActions)
        {
            if (action != null)
                action.action.Enable();
        }

        if (dropAction != null)
            dropAction.action.Enable();

        if (hotbarScroll != null)
            hotbarScroll.action.Enable();
    }

    private void OnDisable()
    {
        if (pickupobj != null)
            pickupobj.action.Disable();

        foreach (InputActionReference action in hotbarActions)
        {
            if (action != null)
                action.action.Disable();
        }

        if (dropAction != null)
            dropAction.action.Disable();

        if (hotbarScroll != null)
            hotbarScroll.action.Disable();
    }

    public Items GetHotbarItem()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return null;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return null;

        return equippedSlot.GetItem();
    }

    public int GetEquippedDurability()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return -1;

        Slot slot = hotbarSlots[equippedHotBarIndex];

        if (!slot.Hasitem())
            return -1;

        return slot.GetDurability();
    }

    public bool UseEquippedDurability()
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return false;

        bool broke = equippedSlot.UseDurability(1);

        EquippedHandItem();
        return broke;
    }

    public void RemoveHotbarItem(int amount)
    {
        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return;

        equippedSlot.RemoveAmount(amount);
        EquippedHandItem();
    }

    public bool IsHammerEquipped()
    {
        Items item = GetHotbarItem();
        return item != null && item == Hammer;
    }

    public bool IsAxeEquipped()
    {
        Items item = GetHotbarItem();
        return item != null && item == AxeItem;
    }

    public bool IsKnifeEquipped()
    {
        Items item = GetHotbarItem();
        return item != null && item == Knife;
    }

    public bool IsSpearEquipped()
    {
        Items item = GetHotbarItem();
        return item != null && item == Spear;
    }

    public bool IsFlintEquipped(Items flintItem)
    {
        Items item = GetHotbarItem();
        return item != null && item == flintItem;
    }

    private void Update()
    {
        DetectLookedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotBarSelection();
        HandleHotBarScroll();
        HandleDropEquippedItem();

        UpdateHotBarOpacity();
    }

    public int GetTotalItemAmount(Items itemToCheck)
    {
        int total = 0;

        foreach (Slot slot in hotbarSlots)
        {
            if (slot.Hasitem() && slot.GetItem() == itemToCheck)
            {
                total += slot.GetAmount();
            }
        }

        return total;
    }

    public int RemoveItemAmount(Items itemToRemove, int amount)
    {
        int remaining = amount;

        foreach (Slot slot in hotbarSlots)
        {
            if (remaining <= 0)
                break;

            if (slot.Hasitem() && slot.GetItem() == itemToRemove)
            {
                int amountInSlot = slot.GetAmount();
                int amountToRemove = Mathf.Min(amountInSlot, remaining);

                slot.RemoveAmount(amountToRemove);
                remaining -= amountToRemove;
            }
        }

        EquippedHandItem();

        return amount - remaining;
    }

    public bool AddItem(Items itemToAdd, int amount, int durability = -1)
    {
        if (itemToAdd == null || amount <= 0)
            return false;

        if (itemToAdd.hasDurability)
        {
            int emptySlots = 0;

            foreach (Slot slot in hotbarSlots)
            {
                if (!slot.Hasitem())
                    emptySlots++;
            }

            if (emptySlots < amount)
            {
                EquippedHandItem();
                return false;
            }

            for (int i = 0; i < amount; i++)
            {
                Slot emptySlot = null;

                foreach (Slot slot in hotbarSlots)
                {
                    if (!slot.Hasitem())
                    {
                        emptySlot = slot;
                        break;
                    }
                }

                if (emptySlot == null)
                    return false;

                int durabilityToUse = durability;

                if (durabilityToUse < 0)
                    durabilityToUse = itemToAdd.maxDurability;

                emptySlot.SetItem(itemToAdd, 1, durabilityToUse);
                durability = -1;
            }

            EquippedHandItem();
            return true;
        }

        int maxStack = Mathf.Max(1, itemToAdd.maxStack);
        int availableSpace = 0;

        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.Hasitem())
            {
                availableSpace += maxStack;
                continue;
            }

            if (slot.GetItem() == itemToAdd)
            {
                availableSpace += Mathf.Max(
                    0,
                    maxStack - slot.GetAmount()
                );
            }
        }

        if (availableSpace < amount)
        {
            EquippedHandItem();
            return false;
        }

        int remaining = amount;

        foreach (Slot slot in hotbarSlots)
        {
            if (remaining <= 0)
                break;

            if (!slot.Hasitem() || slot.GetItem() != itemToAdd)
                continue;

            int currentAmount = slot.GetAmount();
            int spaceLeft = maxStack - currentAmount;

            if (spaceLeft <= 0)
                continue;

            int amountToAdd = Mathf.Min(spaceLeft, remaining);
            slot.SetItem(itemToAdd, currentAmount + amountToAdd);
            remaining -= amountToAdd;

            if (remaining <= 0)
                break;
        }

        foreach (Slot slot in hotbarSlots)
        {
            if (remaining <= 0)
                break;

            if (slot.Hasitem())
                continue;

            int amountToPlace = Mathf.Min(maxStack, remaining);
            slot.SetItem(itemToAdd, amountToPlace);
            remaining -= amountToPlace;
        }

        EquippedHandItem();
        return remaining <= 0;
    }

    private void StartDrag()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.Hasitem())
            {
                dragslot = hovered;
                isDraggin = true;

                if (DragIcon != null)
                {
                    DragIcon.sprite = hovered.GetItem().icon;
                    DragIcon.color = new Color(1, 1, 1, 0.5f);
                    DragIcon.enabled = true;
                }
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

                HandleDrop(dragslot, hovered);

                if (craftingSlots.Contains(hovered) ||
                    craftingSlots.Contains(originalSlot))
                {
                    if (craftingSystem != null)
                    {
                        craftingSystem.UpdateCraftingResult();
                    }
                }

                if (DragIcon != null)
                {
                    DragIcon.enabled = false;
                }

                dragslot = null;
                isDraggin = false;

                EquippedHandItem();
            }
            else
            {
                if (DragIcon != null)
                {
                    DragIcon.enabled = false;
                }

                dragslot = null;
                isDraggin = false;
            }
        }
    }

    private Slot GetHoveredSlot()
    {
        foreach (Slot slot in allSlots)
        {
            if (slot.hovering)
                return slot;
        }

        foreach (Slot slot in storageSlots)
        {
            if (slot.hovering)
                return slot;
        }

        foreach (Slot slot in craftingSlots)
        {
            if (slot.hovering)
                return slot;
        }

        if (fireplaceFuelSlot != null &&
            fireplaceFuelSlot.hovering)
        {
            return fireplaceFuelSlot;
        }

        if (fireplaceCookingSlot != null &&
            fireplaceCookingSlot.hovering)
        {
            return fireplaceCookingSlot;
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
        if (from == null || to == null || from == to)
            return;

        if (IsFireplaceSlot(to))
        {
            HandleDropIntoFireplace(from, to);
            return;
        }

        if (craftingSystem != null &&
            to == craftingSystem.resultSlot)
            return;

        if (craftingSystem != null &&
            from == craftingSystem.resultSlot)
        {
            TryTakeCraftedItem(to);
            return;
        }

        if (craftingSlots.Contains(to))
        {
            if (to.Hasitem() || IsFireplaceSlot(from))
                return;

            to.SetItem(from.GetItem(), 1, from.GetDurability());
            from.RemoveAmount(1);
            return;
        }

        if (craftingSlots.Contains(from))
        {
            if (!to.Hasitem())
            {
                to.SetItem(
                    from.GetItem(),
                    from.GetAmount(),
                    from.GetDurability()
                );

                from.ClearSlot();
                return;
            }

            if (to.GetItem() == from.GetItem() &&
                !from.GetItem().hasDurability)
            {
                int max = to.GetItem().maxStack;
                int space = max - to.GetAmount();

                if (space > 0)
                {
                    int move = Mathf.Min(space, from.GetAmount());
                    to.SetItem(to.GetItem(), to.GetAmount() + move);
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
                from.GetAmount(),
                from.GetDurability()
            );

            from.ClearSlot();
            return;
        }

        if (to.GetItem() == from.GetItem() &&
            !from.GetItem().hasDurability)
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
                    from.GetAmount() - move,
                    from.GetDurability()
                );

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

                return;
            }
        }

        Items tempItem = to.GetItem();
        int tempAmount = to.GetAmount();
        int tempDurability = to.GetDurability();

        to.SetItem(
            from.GetItem(),
            from.GetAmount(),
            from.GetDurability()
        );

        from.SetItem(
            tempItem,
            tempAmount,
            tempDurability
        );
    }

    private void HandleDropIntoFireplace(Slot from, Slot to)
    {
        if (from == null || to == null ||
            !from.Hasitem() || to.Hasitem())
            return;

        if (IsFireplaceSlot(from))
            return;

        if (craftingSystem != null &&
            from == craftingSystem.resultSlot)
            return;

        Items item = from.GetItem();

        if (item == null)
            return;

        if (to == fireplaceFuelSlot)
        {
            if (item != fireplaceWoodItem)
                return;
        }
        else if (to == fireplaceCookingSlot)
        {
            if (item != fireplaceFishItem &&
                item != fireplaceCrabItem &&
                item != fireplaceWaterItem)
                return;
        }
        else
        {
            return;
        }

        to.SetItem(item, 1, from.GetDurability());
        from.RemoveAmount(1);

        EquippedHandItem();
    }

    public bool CanFitItems(List<Items> items, List<int> amounts)
    {
        if (items == null || amounts == null ||
            items.Count != amounts.Count)
            return false;

        int emptySlots = 0;

        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.Hasitem())
                emptySlots++;
        }

        Dictionary<Items, int> availableStackSpace =
            new Dictionary<Items, int>();

        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.Hasitem())
                continue;

            Items item = slot.GetItem();

            if (item == null || item.hasDurability)
                continue;

            int space = item.maxStack - slot.GetAmount();

            if (space <= 0)
                continue;

            if (!availableStackSpace.ContainsKey(item))
                availableStackSpace[item] = 0;

            availableStackSpace[item] += space;
        }

        for (int i = 0; i < items.Count; i++)
        {
            Items item = items[i];
            int amount = amounts[i];

            if (item == null || amount <= 0)
                continue;

            if (item.hasDurability)
            {
                if (emptySlots < amount)
                    return false;

                emptySlots -= amount;
                continue;
            }

            int remaining = amount;

            if (availableStackSpace.ContainsKey(item))
            {
                int stackSpace = availableStackSpace[item];
                int amountFromStacks =
                    Mathf.Min(stackSpace, remaining);

                availableStackSpace[item] -= amountFromStacks;
                remaining -= amountFromStacks;
            }

            if (remaining <= 0)
                continue;

            int maxStack = Mathf.Max(1, item.maxStack);

            int slotsNeeded = Mathf.CeilToInt(
                (float)remaining / maxStack
            );

            if (emptySlots < slotsNeeded)
                return false;

            emptySlots -= slotsNeeded;
        }

        return true;
    }

    private void UpdateDragItemPosition()
    {
        if (isDraggin && Mouse.current != null && DragIcon != null)
        {
            DragIcon.transform.position =
                Mouse.current.position.ReadValue();
        }
    }

    private void Pickup()
    {
        if (lookedAtRender == null ||
            pickupobj == null ||
            !pickupobj.action.WasPressedThisFrame())
            return;

        GroundItem item = lookedAtRender.GetComponent<GroundItem>();

        if (item == null)
            return;

        bool pickedUp = AddItem(
            item.item,
            item.amount,
            item.currentDurability
        );

        if (!pickedUp)
        {
            ShowDropImage();
            return;
        }

        if (audioManager != null)
        {
            audioManager.PlaySfx(audioManager.PickupItem);
        }

        RespawnMR respawn = item.GetComponent<RespawnMR>();

        if (respawn != null)
        {
            respawn.RespawnResource();
        }
        else
        {
            Destroy(item.gameObject);
        }

        EquippedHandItem();
    }

    private void DetectLookedAtItem()
    {
        if (lookedAtRender != null)
        {
            lookedAtRender.material = originalmaerial;
            lookedAtRender = null;
            originalmaerial = null;
        }

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            GroundItem item = hit.collider.GetComponent<GroundItem>();

            if (item == null)
                return;

            Renderer rend = item.GetComponent<Renderer>();

            if (rend != null)
            {
                originalmaerial = rend.material;
                lookedAtRender = rend;
            }
        }
    }

    private void TryTakeCraftedItem(Slot destination)
    {
        if (craftingSystem == null || destination == null)
            return;

        Slot resultSlot = craftingSystem.resultSlot;

        if (resultSlot == null)
            return;

        Items craftedItem = resultSlot.GetItem();
        int craftedAmount = resultSlot.GetAmount();

        if (craftedItem == null || craftedAmount <= 0)
            return;

        if (!destination.Hasitem())
        {
            destination.SetItem(craftedItem, craftedAmount);
        }
        else if (destination.GetItem() == craftedItem &&
                 !craftedItem.hasDurability)
        {
            int max = craftedItem.maxStack;
            int space = max - destination.GetAmount();

            if (space < craftedAmount)
                return;

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
            hotbarSlots[i].SelectedFrame(i == equippedHotBarIndex);
        }
    }

    private void HandleHotBarSelection()
    {
        if (hotbarActions == null)
            return;

        for (int i = 0; i < hotbarActions.Length; i++)
        {
            if (hotbarActions[i] == null ||
                hotbarActions[i].action == null)
                continue;

            if (hotbarActions[i].action.WasPressedThisFrame())
            {
                if (i >= hotbarSlots.Count)
                    continue;

                equippedHotBarIndex = i;
                UpdateHotBarOpacity();
                EquippedHandItem();
            }
        }
    }

    private void HandleDropEquippedItem()
    {
        if (dropAction == null ||
            !dropAction.action.WasPressedThisFrame())
            return;

        if (equippedHotBarIndex < 0 ||
            equippedHotBarIndex >= hotbarSlots.Count)
            return;

        Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

        if (!equippedSlot.Hasitem())
            return;

        Items item = equippedSlot.GetItem();
        GameObject prefab = item.ItenPrefab;

        if (prefab == null)
            return;

        if (ItemManager.Instance != null &&
            !ItemManager.Instance.CanSpawn(prefab))
            return;

        GameObject dropped = Instantiate(
            prefab,
            Camera.main.transform.position +
            Camera.main.transform.forward,
            Quaternion.identity
        );

        GroundItem item1 = dropped.GetComponent<GroundItem>();

        if (item1 != null)
        {
            item1.item = item;
            item1.amount = equippedSlot.GetAmount();
            item1.currentDurability = equippedSlot.GetDurability();
            item1.sourcePrefab = prefab;
        }

        equippedSlot.ClearSlot();
        EquippedHandItem();
    }
}