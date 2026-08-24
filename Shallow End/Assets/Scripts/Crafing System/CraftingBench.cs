using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CraftingInteraction : MonoBehaviour
{

    [SerializeField] private InputActionReference interactAction;


    [SerializeField] private GameObject inventoryContainer;
    [SerializeField] private GameObject craftingSystem;
    [SerializeField] private GameObject background;


    [SerializeField] private TextMeshProUGUI craftText;


    [SerializeField] private GameObject playerHUD;


    [SerializeField] private PlayerController playerController;
    [SerializeField] private Monster monster;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private TimeController timeController;

    private bool playerInRange = false;
    private bool craftingOpen = false;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }

    private void Start()
    {
        craftingOpen = false;

        if (inventoryContainer != null)
            inventoryContainer.SetActive(false);

        if (craftingSystem != null)
            craftingSystem.SetActive(false);

        if (background != null)
            background.SetActive(false);


        if (craftText != null)
            craftText.gameObject.SetActive(false);

        
        if (playerHUD != null)
            playerHUD.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
     
        if (!playerInRange)
            return;

        if (interactAction == null)
            return;

        if (!interactAction.action.WasPressedThisFrame())
            return;

        if (!craftingOpen)
        {
            OpenCrafting();
        }
        else
        {
            CloseCrafting();
        }
    }

    private void OpenCrafting()
    {
        craftingOpen = true;

  

        
        if (craftText != null)
            craftText.gameObject.SetActive(false);

   
        if (timeController != null)
            timeController.enabled = false;

      
        if (inventoryContainer != null)
            inventoryContainer.SetActive(true);

    
        if (craftingSystem != null)
            craftingSystem.SetActive(true);

      
        if (background != null)
            background.SetActive(true);

 
        if (playerController != null)
            playerController.PlayerControl(false);

      
        if (monster != null)
            monster.enabled = false;

        
        if (healthScript != null)
            healthScript.enabled = false;

     
        if (playerHUD != null)
            playerHUD.SetActive(false);

    
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseCrafting()
    {
        craftingOpen = false;

        
        if (inventoryContainer != null)
            inventoryContainer.SetActive(false);

 
        if (craftingSystem != null)
            craftingSystem.SetActive(false);

    
        if (background != null)
            background.SetActive(false);

       
        if (playerController != null)
            playerController.PlayerControl(true);


        if (monster != null)
            monster.enabled = true;

   
        if (timeController != null)
            timeController.enabled = true;

    
        if (healthScript != null)
            healthScript.enabled = true;

  
        if (playerHUD != null)
            playerHUD.SetActive(true);

 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        
        if (playerInRange && craftText != null)
        {
            craftText.text = "Press E to craft";
            craftText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

       

        // Show crafting prompt
        if (!craftingOpen && craftText != null)
        {
            craftText.text = "Press E to craft";
            craftText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

    

      
        if (craftText != null)
            craftText.gameObject.SetActive(false);

     
        if (craftingOpen)
        {
            CloseCrafting();
        }
    }
}