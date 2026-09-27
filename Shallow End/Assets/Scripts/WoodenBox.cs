using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WoodenBox : MonoBehaviour
{
    [System.Serializable]
    public class ChestReward
    {
        public Items item;
        public int amount = 1;


        public int durability = -1;
    }


    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private GameObject interactionUI;

    

    [SerializeField] private GameObject gameUI;


    [SerializeField] private Inventory inventory;


    [SerializeField] private List<ChestReward> firstOpenRewards = new List<ChestReward>();


    [SerializeField] private GameObject journalObject;


    [SerializeField] private GameObject firstOpenGameObject;


    [SerializeField] private TaskManager taskManager;


    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private GameObject bookManager;
    [SerializeField] private string fullInventoryMessage = "My hotbar is full";

    private bool playerNearby;
    private bool boxOpen;
    private bool hasBeenOpened;
    private float warningTimer;

    private void Start()
    {


        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (journalObject != null)
            journalObject.SetActive(false);

        if (firstOpenGameObject != null)
            firstOpenGameObject.SetActive(false);

        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

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

    private void Update()
    {
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;

            if (warningTimer <= 0f && warningText != null)
                warningText.gameObject.SetActive(false);
        }

        if (!playerNearby)
            return;

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            if (boxOpen)
            {
                CloseBox();
            }
            else
            {
                TryOpenBox();
            }
        }
    }

    private void TryOpenBox()
    {

        if (!HasSpokenToThomas())
        {
            ShowLockedMessage();
            return;
        }

        OpenBox();
    }

    private bool HasSpokenToThomas()
    {
        if (taskManager == null)
            return false;

    
        if (taskManager.IsTaskCompleted(
            TaskManager.TaskID.SpeakToThomasFirst))
        {
            return true;
        }


        if (taskManager.IsCurrentTask(
            TaskManager.TaskID.SearchWoodenBox))
        {
            return true;
        }

        return false;
    }
    private bool CanFitAllRewards()
    {
        if (inventory == null)
            return false;

        List<Items> items = new List<Items>();
        List<int> amounts = new List<int>();

        foreach (ChestReward reward in firstOpenRewards)
        {
            if (reward == null || reward.item == null)
                continue;

            items.Add(reward.item);
            amounts.Add(Mathf.Max(1, reward.amount));
        }

        return inventory.CanFitItems(items, amounts);
    }
    private void ShowFullInventoryMessage()
    {
        if (warningText == null)
            return;

        warningText.text = fullInventoryMessage;
        warningText.gameObject.SetActive(true);

        warningTimer = warningDuration;
    }
    private void OpenBox()
    {

        if (!CanFitAllRewards())
        {
            ShowFullInventoryMessage();
            return;
        }

        boxOpen = true;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!hasBeenOpened)
        {
            hasBeenOpened = true;

            GiveFirstOpenRewards();

            if (journalObject != null)
                journalObject.SetActive(true);

            if (bookManager != null)
            {
                bookManager.SetActive(true);
            }

            if (firstOpenGameObject != null)
                firstOpenGameObject.SetActive(true);

            if (taskManager != null)
            {
                taskManager.CompleteTask(
                    TaskManager.TaskID.SearchWoodenBox
                );
            }
        }
    }
    private void CloseBox()
    {
        boxOpen = false;

        if (gameUI != null)
            gameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerNearby && interactionUI != null)
            interactionUI.SetActive(true);
    }

    private void GiveFirstOpenRewards()
    {
        if (inventory == null)
        {

            return;
        }

        foreach (ChestReward reward in firstOpenRewards)
        {
            if (reward == null || reward.item == null)
                continue;

            int amount = Mathf.Max(1, reward.amount);

            for (int i = 0; i < amount; i++)
            {
  
                if (reward.item.hasDurability)
                {
                    inventory.AddItem(
                        reward.item,
                        1,
                        reward.durability
                    );
                }
                else
                {
                    inventory.AddItem(
                        reward.item,
                        1
                    );
                }
            }
        }
    }

    private void ShowLockedMessage()
    {
        if (warningText == null)
            return;

        warningText.text = "I shouldn't be snooping around";
        warningText.gameObject.SetActive(true);

        warningTimer = warningDuration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        if (!boxOpen && interactionUI != null)
            interactionUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (boxOpen)
            CloseBox();
    }

    public bool IsOpen()
    {
        return boxOpen;
    }
}