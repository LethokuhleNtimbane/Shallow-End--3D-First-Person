using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    [SerializeField] private GameObject inGameHUD;

    [Header("Inventory")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject inventoryContainer;

    [Header("Crafting")]
    [SerializeField] private GameObject craftingUI;

    private bool inventoryOpen;
    private bool craftingOpen;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Start with both interfaces closed
        inventoryOpen = false;
        craftingOpen = false;

        UpdateUIState();
    }

    public bool IsInventoryOpen()
    {
        return inventoryOpen;
    }

    public bool IsCraftingOpen()
    {
        return craftingOpen;
    }

    public bool IsAnyUIOpen()
    {
        return inventoryOpen || craftingOpen;
    }

    public void SetInventoryOpen(bool isOpen)
    {
        inventoryOpen = isOpen;

        UpdateUIState();
    }

    public void SetCraftingOpen(bool isOpen)
    {
        craftingOpen = isOpen;

        UpdateUIState();
    }

    private void UpdateUIState()
    {
        bool anyUIOpen = inventoryOpen || craftingOpen;

        // -------------------------
        // INVENTORY
        // -------------------------

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(inventoryOpen || craftingOpen);
        }

        if (inventoryContainer != null)
        {
            inventoryContainer.SetActive(inventoryOpen || craftingOpen);
        }

        // -------------------------
        // CRAFTING
        // -------------------------

        if (craftingUI != null)
        {
            craftingUI.SetActive(craftingOpen);
        }

        // -------------------------
        // NORMAL HUD
        // -------------------------

        if (inGameHUD != null)
        {
            inGameHUD.SetActive(!anyUIOpen);
        }

        // -------------------------
        // PLAYER CONTROL
        // -------------------------

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayerControl(!anyUIOpen);
        }

        // -------------------------
        // CURSOR
        // -------------------------

        Cursor.visible = anyUIOpen;

        Cursor.lockState = anyUIOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}