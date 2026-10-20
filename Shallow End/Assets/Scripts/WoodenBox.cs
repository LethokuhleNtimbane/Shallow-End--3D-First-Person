
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WoodenBox : MonoBehaviour
{
    [System.Serializable]
    public class ChestReward
    {
        public Items item;
        public int amount = 1;
        public int durability = -1;
    }

    public bool IsOpen => isOpen;

    // Tracks rewards only during the current playthrough.
    private static bool rewardsClaimedThisPlaythrough = false;

    public static void StartNewGame()
    {
        rewardsClaimedThisPlaythrough = false;
    }

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private GameObject interactionUI;

    [Header("Chest UI")]
    [SerializeField] private GameObject chestUI;
    [SerializeField] private GameObject chestBackground;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject[] hudToHideWhileOpen;

    [Header("Chest Storage")]
    [SerializeField] private List<Slot> chestSlots = new List<Slot>();
    [SerializeField] private Inventory inventory;

    [SerializeField]
    private string fullInventoryMessage = "Your hotbar is full!";

    [Header("First-Time Rewards")]
    [SerializeField]
    private List<ChestReward> firstOpenRewards =
        new List<ChestReward>();

    [SerializeField] private GameObject firstOpenGameObject;

    [Header("Journal")]
    [SerializeField] private GameObject journalObject;
    [SerializeField] private BookManager bookManager;

    [Header("Tasks")]
    [SerializeField] private TaskManager taskManager;

    [Header("Warnings")]
    [SerializeField] private GameObject warningObject;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningDuration = 2f;

    [Header("Gameplay Scripts")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DashScript dash;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private TimeController timeController;

    [Header("Other Scripts")]
    [SerializeField] private PauseScript Pause;
    [SerializeField] private BookManager Book;

    [Header("Monster Scripts")]
    [SerializeField] private Monster monster;
    [SerializeField] private MonsterAttack monsterAttack;

    private bool playerInRange;
    private bool isOpen;
    private float warningTimer;

    private void Awake()
    {
        if (chestUI != null)
        {
            chestSlots.Clear();
            chestSlots.AddRange(
                chestUI.GetComponentsInChildren<Slot>(true)
            );
        }

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (bookManager == null)
            bookManager = FindFirstObjectByType<BookManager>();

        if (Book == null)
            Book = bookManager;

        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();
    }

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Disable();

        if (isOpen)
            CloseBox();
    }

    private void Start()
    {
        if (chestUI != null)
            chestUI.SetActive(false);

        if (chestBackground != null)
            chestBackground.SetActive(false);

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (journalObject != null)
            journalObject.SetActive(false);

        if (firstOpenGameObject != null)
            firstOpenGameObject.SetActive(false);

        if (warningObject != null)
            warningObject.SetActive(false);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        // Keep the hotbar visible while the chest is open.
        if (gameUI != null)
            gameUI.SetActive(true);
    }

    private void Update()
    {
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;

            if (warningTimer <= 0f)
                HideWarning();
        }

        if (!playerInRange || interactAction == null)
            return;

        if (interactAction.action.WasPressedThisFrame())
        {
            if (isOpen)
                CloseBox();
            else
                TryOpenBox();
        }
    }

    private void TryOpenBox()
    {
        if (isOpen)
            return;

        // The player must speak to Thomas first.
        if (taskManager != null &&
            !taskManager.IsTaskCompleted(
                TaskManager.TaskID.SpeakToThomasFirst))
        {
            ShowWarning("I shouldn't be snooping around.");
            return;
        }

        // Only check reward space if rewards have not been claimed.
        if (!rewardsClaimedThisPlaythrough && !CanFitAllRewards())
        {
            ShowWarning(fullInventoryMessage);
            return;
        }

        OpenBox();
    }

    private void OpenBox()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (chestUI != null)
            chestUI.SetActive(true);

        if (chestBackground != null)
            chestBackground.SetActive(true);

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        if (inventory != null)
            inventory.RegisterStorageSlots(chestSlots);

        if (hudToHideWhileOpen != null)
        {
            foreach (GameObject hudObject in hudToHideWhileOpen)
            {
                if (hudObject != null)
                    hudObject.SetActive(false);
            }
        }

        // Disable gameplay and menu scripts.
        DisableGameplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Award the first-time rewards only once per playthrough.
        if (!rewardsClaimedThisPlaythrough)
        {
            GiveFirstOpenRewards();

            rewardsClaimedThisPlaythrough = true;

            // Enable the object that tracks book opening.
            if (journalObject != null)
                journalObject.SetActive(true);

            // Show the prompt telling the player they can open the book.
            if (firstOpenGameObject != null)
                firstOpenGameObject.SetActive(true);

            if (bookManager != null)
            {
                bookManager.journalUnlocked = true;
                bookManager.gameObject.SetActive(true);
            }

            if (taskManager != null)
            {
                taskManager.CompleteTask(
                    TaskManager.TaskID.SearchWoodenBox
                );
            }
        }
    }

    public void CloseBox()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (inventory != null)
            inventory.UnregisterStorageSlots();

        if (chestUI != null)
            chestUI.SetActive(false);

        if (chestBackground != null)
            chestBackground.SetActive(false);

        // Keep the hotbar visible.
        if (gameUI != null)
            gameUI.SetActive(true);

        if (hudToHideWhileOpen != null)
        {
            foreach (GameObject hudObject in hudToHideWhileOpen)
            {
                if (hudObject != null)
                    hudObject.SetActive(true);
            }
        }

        // Restore gameplay scripts when appropriate.
        EnableGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactionUI != null && playerInRange)
            interactionUI.SetActive(true);
    }

    private void DisableGameplay()
    {
        if (playerController != null)
            playerController.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (timeController != null)
            timeController.enabled = false;

        // Disable PauseScript and BookManager.
        if (Pause != null)
            Pause.enabled = false;

        if (Book != null)
            Book.enabled = false;

        // Stop monster behaviour.
        if (monster != null)
            monster.enabled = false;

        if (monsterAttack != null)
            monsterAttack.enabled = false;
    }

    private void EnableGameplay()
    {
        // Don't resume gameplay if another menu is open.
        if (CraftingBench.IsCraftingOpen || BookManager.IsBookOpen)
            return;

        if (playerController != null)
            playerController.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

        if (timeController != null)
            timeController.enabled = true;

        // Re-enable PauseScript and BookManager.
        if (Pause != null)
            Pause.enabled = true;

        if (Book != null)
            Book.enabled = true;

        // Resume monster behaviour.
        if (monster != null)
            monster.enabled = true;

        if (monsterAttack != null)
            monsterAttack.enabled = true;
    }

    private bool CanFitAllRewards()
    {
        if (inventory == null)
            return false;

        List<Items> items = new List<Items>();
        List<int> amounts = new List<int>();

        foreach (ChestReward reward in firstOpenRewards)
        {
            if (reward == null ||
                reward.item == null ||
                reward.amount <= 0)
            {
                continue;
            }

            items.Add(reward.item);
            amounts.Add(reward.amount);
        }

        return inventory.CanFitItems(items, amounts);
    }

    private void GiveFirstOpenRewards()
    {
        if (inventory == null || firstOpenRewards == null)
            return;

        foreach (ChestReward reward in firstOpenRewards)
        {
            if (reward == null ||
                reward.item == null ||
                reward.amount <= 0)
            {
                continue;
            }

            inventory.AddItem(
                reward.item,
                reward.amount,
                reward.durability
            );
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
        }

        if (warningObject != null)
            warningObject.SetActive(true);

        warningTimer = warningDuration;
    }

    private void HideWarning()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);

        if (warningObject != null)
            warningObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (!isOpen && interactionUI != null)
            interactionUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (isOpen)
            CloseBox();
    }

    private void OnDestroy()
    {
        if (isOpen && inventory != null)
            inventory.UnregisterStorageSlots();
    }
}
