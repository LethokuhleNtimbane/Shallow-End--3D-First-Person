using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CraftingBench : MonoBehaviour
{
    public static bool IsCraftingOpen { get; private set; }

    [SerializeField] private InputActionReference interactAction;

    [SerializeField] private GameObject craftingSystem;
    [SerializeField] private GameObject background;

    [SerializeField] private TextMeshProUGUI craftText;

    [SerializeField] private GameObject playerHUD;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private DashScript dash;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private TimeController timeController;

    private bool playerInRange = false;
    private bool craftingOpen = false;

    private AudioManager audioManager;

    private void Awake()
    {
        IsCraftingOpen = false;

        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager =
                audioObject.GetComponent<AudioManager>();
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.Disable();
        }
    }

    private void Start()
    {
        craftingOpen = false;
        IsCraftingOpen = false;

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


        if (!interactAction.action.enabled)
        {
            interactAction.action.Enable();
        }

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
        if (craftingOpen)
            return;

        craftingOpen = true;
        IsCraftingOpen = true;

        if (craftText != null)
            craftText.gameObject.SetActive(false);

       
        if (timeController != null)
            timeController.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        // Stop EVERY monster, including monsters spawned
        // by MonsterSpawner.
        Monster.SetDialoguePaused(true);
        MonsterAttack.SetDialoguePaused(true);

        if (craftingSystem != null)
            craftingSystem.SetActive(true);

        if (background != null)
            background.SetActive(true);

        if (playerHUD != null)
            playerHUD.SetActive(false);

        if (audioManager != null)
        {
            audioManager.PlaySfx(audioManager.FlintLight);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseCrafting()
    {
        if (!craftingOpen)
            return;

        craftingOpen = false;
        IsCraftingOpen = false;

        if (craftingSystem != null)
            craftingSystem.SetActive(false);

        if (background != null)
            background.SetActive(false);

   
        if (playerController != null)
            playerController.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (timeController != null)
            timeController.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

     
        Monster.SetDialoguePaused(false);
        MonsterAttack.SetDialoguePaused(false);

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

    private void OnDestroy()
    {
        if (craftingOpen)
        {
            Monster.SetDialoguePaused(false);
            MonsterAttack.SetDialoguePaused(false);
        }

        IsCraftingOpen = false;
    }
}