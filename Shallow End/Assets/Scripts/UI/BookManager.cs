using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BookController : MonoBehaviour
{

    [SerializeField] private InputActionReference openBookAction;

    [SerializeField] private GameObject bookUI;
    [SerializeField] private Items bookItem;


    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TimeController timeController;
    [SerializeField] private Monster monster;
    [SerializeField] private HealthScript healthScript;


    [SerializeField] private GameObject playerHUD;
    [SerializeField] private MonsterAttack monsterattack;
    private bool bookOpen = false;

    private float previousTimeMultiplier;

    private void OnEnable()
    {
        if (openBookAction != null)
            openBookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (openBookAction != null)
            openBookAction.action.Disable();
    }

    private void Start()
    {
        bookOpen = false;

        if (bookUI != null)
            bookUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (openBookAction == null)
            return;

        if (!openBookAction.action.WasPressedThisFrame())
            return;

        if (bookOpen)
        {
            CloseBook();
            return;
        }


        if (!IsBookEquipped())
        {
          
            return;
        }

        OpenBook();
    }

    private bool IsBookEquipped()
    {
        if (inventory == null)
            return false;

        Items equippedItem = inventory.GetHotbarItem();

        if (equippedItem == null)
            return false;

        return equippedItem == bookItem;
    }

    private void OpenBook()
    {
        bookOpen = true;


        if (timeController != null)
        {
          timeController.enabled = false;
        }

 
        if (bookUI != null)
            bookUI.SetActive(true);

     
        if (playerController != null)
            playerController.PlayerControl(false);

        
        if (monster != null)
            monster.enabled = false;
            monsterattack.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

   
        if (playerHUD != null)
            playerHUD.SetActive(false);


        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseBook()
    {
        bookOpen = false;

     

        if (bookUI != null)
            bookUI.SetActive(false);

  
        if (playerController != null)
            playerController.PlayerControl(true);


        if (monster != null)
            monster.enabled = true;
            monsterattack.enabled=true;

    
        if (healthScript != null)
            healthScript.enabled = true;

 
        if (timeController != null)
        {
            timeController.enabled=true;
        }


        if (playerHUD != null)
            playerHUD.SetActive(true);

 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}