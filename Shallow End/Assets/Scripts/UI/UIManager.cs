using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;


    [SerializeField] private GameObject inGameHUD;

    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject inventoryContainer;


    [SerializeField] private GameObject craftingUI;

    private bool inventoryOpen;
    private bool craftingOpen;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    
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



        if (inventoryUI != null)
        {
            inventoryUI.SetActive(inventoryOpen || craftingOpen);
        }

        if (inventoryContainer != null)
        {
            inventoryContainer.SetActive(inventoryOpen || craftingOpen);
        }



        if (craftingUI != null)
        {
            craftingUI.SetActive(craftingOpen);
        }

       

        if (inGameHUD != null)
        {
            inGameHUD.SetActive(!anyUIOpen);
        }

    

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayerControl(!anyUIOpen);
        }



        Cursor.visible = anyUIOpen;

        Cursor.lockState = anyUIOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}