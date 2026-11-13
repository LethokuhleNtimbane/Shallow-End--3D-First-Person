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

    private static bool rewardsClaimedThisPlaythrough = false;
    private static bool journalUnlockedThisPlaythrough = false;

    public static void StartNewGame()
    {
        rewardsClaimedThisPlaythrough = false;
        journalUnlockedThisPlaythrough = false;
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
    [SerializeField] private FoodScript foodScript;
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

        if (foodScript == null)
            foodScript = FindFirstObjectByType<FoodScript>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (dash == null)
            dash = FindFirstObjectByType<DashScript>();

        if (healthScript == null)
            healthScript = FindFirstObjectByType<HealthScript>();

        if (timeController == null)
            timeController = FindFirstObjectByType<TimeController>();

        if (Pause == null)
            Pause = FindFirstObjectByType<PauseScript>();
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

        if (gameUI != null)
            gameUI.SetActive(true);

        if (journalUnlockedThisPlaythrough)
        {
            UnlockJournal();
        }
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

        if (taskManager != null &&
            !taskManager.IsTaskCompleted(
                TaskManager.TaskID.SpeakToThomasFirst))
        {
            ShowWarning("I shouldn't be snooping around.");
            return;
        }

        if (!rewardsClaimedThisPlaythrough &&
            !CanFitAllRewards())
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

        DisableGameplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!rewardsClaimedThisPlaythrough)
        {
            GiveFirstOpenRewards();

            rewardsClaimedThisPlaythrough = true;

            if (taskManager != null)
            {
                taskManager.CompleteTask(
                    TaskManager.TaskID.SearchWoodenBox
                );
            }
        }

        UnlockJournal();
    }

    private void UnlockJournal()
    {
        journalUnlockedThisPlaythrough = true;

        if (journalObject != null)
            journalObject.SetActive(true);

        if (firstOpenGameObject != null)
            firstOpenGameObject.SetActive(true);

        if (bookManager != null)
        {
            bookManager.journalUnlocked = true;
            bookManager.gameObject.SetActive(true);
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

        EnableGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactionUI != null && playerInRange)
            interactionUI.SetActive(true);
    }

    private void DisableGameplay()
    {
        if (foodScript != null)
            foodScript.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (timeController != null)
            timeController.enabled = false;

        if (Pause != null)
            Pause.enabled = false;

        if (Book != null)
            Book.enabled = false;

        if (monster != null)
            monster.enabled = false;

        if (monsterAttack != null)
            monsterAttack.enabled = false;
    }

    private void EnableGameplay()
    {
        if (CraftingBench.IsCraftingOpen || BookManager.IsBookOpen)
            return;

        if (foodScript != null)
            foodScript.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

        if (timeController != null)
            timeController.enabled = true;

        if (Pause != null)
            Pause.enabled = true;

        if (Book != null)
            Book.enabled = true;

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