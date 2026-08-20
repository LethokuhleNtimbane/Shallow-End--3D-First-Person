using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private GameObject inGameHUD;
    private bool inventoryOpen;
    private bool craftingOpen;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsInventoryOpen()
    {
        return inventoryOpen;
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

    public bool IsCraftingOpen()
    {
        return craftingOpen;
    }

    public bool IsAnyUIOpen()
    {
        return inventoryOpen || craftingOpen;
    }

    private void UpdateUIState()
    {
        bool anyUIOpen = inventoryOpen || craftingOpen;

        // Hide normal gameplay UI
        if (inGameHUD != null)
        {
            inGameHUD.SetActive(!anyUIOpen);
        }

        // Stop player movement/look
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayerControl(!anyUIOpen);
        }

        // Cursor
        Cursor.visible = anyUIOpen;

        Cursor.lockState = anyUIOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}