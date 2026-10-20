
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CraftingBench : MonoBehaviour
{
    public static bool IsCraftingOpen { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Crafting UI")]
    [SerializeField] private GameObject craftingSystem;
    [SerializeField] private GameObject background;
    [SerializeField] private TextMeshProUGUI craftText;
    [SerializeField] private GameObject playerHUD;

    [Header("Player Scripts")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DashScript dash;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private TimeController timeController;

    [Header("Other Scripts")]
    [SerializeField] private PauseScript Pause;
    [SerializeField] private BookManager Book;

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

        // Disable player-related scripts.
        if (timeController != null)
            timeController.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        // Disable PauseScript and BookManager.
        if (Pause != null)
            Pause.enabled = false;

        if (Book != null)
            Book.enabled = false;

        // Stop every monster, including newly spawned monsters.
        Monster.SetDialoguePaused(true);
        MonsterAttack.SetDialoguePaused(true);

        // Show crafting UI.
        if (craftingSystem != null)
            craftingSystem.SetActive(true);

        if (background != null)
            background.SetActive(true);

        if (playerHUD != null)
            playerHUD.SetActive(false);

        // Play crafting sound.
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

        // Hide crafting UI.
        if (craftingSystem != null)
            craftingSystem.SetActive(false);

        if (background != null)
            background.SetActive(false);

        // Re-enable player-related scripts.
        if (playerController != null)
            playerController.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (timeController != null)
            timeController.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

        // Re-enable PauseScript and BookManager.
        if (Pause != null)
            Pause.enabled = true;

        if (Book != null)
            Book.enabled = true;

        // Resume monsters.
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
