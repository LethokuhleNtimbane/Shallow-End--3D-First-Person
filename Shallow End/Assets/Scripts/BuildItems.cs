using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildableObject : MonoBehaviour
{

    [SerializeField] private GameObject blueprintObject;
    [SerializeField] private GameObject finishedObject;


    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI rockText;
    [SerializeField] private TextMeshProUGUI vinesText;


    [SerializeField] private Items woodItem;
    [SerializeField] private int woodRequired = 20;

    [SerializeField] private Items rockItem;
    [SerializeField] private int rockRequired = 30;

    [SerializeField] private Items vinesItem;
    [SerializeField] private int vinesRequired = 15;

 
    [SerializeField] private BuildableObject requiredBuildable;

    
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private Image taskImage;


    [SerializeField] private TextMeshProUGUI lockedMessage;

    private int woodDelivered;
    private int rockDelivered;
    private int vinesDelivered;

    private Inventory inventory;

    public bool IsBuilt { get; private set; }

    private void Start()
    {
        IsBuilt = false;

   
        if (finishedObject != null)
            finishedObject.SetActive(false);

        inventory = FindFirstObjectByType<Inventory>();

        UpdateRequirementText();
        UpdateTask();


        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

    
        if (requiredBuildable != null &&
            !requiredBuildable.IsBuilt)
        {
            ShowLockedMessage();
            return;
        }

        TryDeliverMaterials();
    }

    private void ShowLockedMessage()
    {
        if (lockedMessage != null)
        {
            lockedMessage.text = "I should start by building a tent.";
            lockedMessage.gameObject.SetActive(true);
        }
    }

    private void TryDeliverMaterials()
    {
        if (inventory == null)
            return;


        int woodNeeded = woodRequired - woodDelivered;

        if (woodNeeded > 0)
        {
            int playerWood =
                inventory.GetTotalItemAmount(woodItem);

            int amountToTake =
                Mathf.Min(playerWood, woodNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(
                    woodItem,
                    amountToTake
                );

                woodDelivered += amountToTake;
            }
        }

 

        int rockNeeded = rockRequired - rockDelivered;

        if (rockNeeded > 0)
        {
            int playerRock =
                inventory.GetTotalItemAmount(rockItem);

            int amountToTake =
                Mathf.Min(playerRock, rockNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(
                    rockItem,
                    amountToTake
                );

                rockDelivered += amountToTake;
            }
        }


        int vinesNeeded = vinesRequired - vinesDelivered;

        if (vinesNeeded > 0)
        {
            int playerVines =
                inventory.GetTotalItemAmount(vinesItem);

            int amountToTake =
                Mathf.Min(playerVines, vinesNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(
                    vinesItem,
                    amountToTake
                );

                vinesDelivered += amountToTake;
            }
        }

        UpdateRequirementText();

        CheckIfBuilt();
    }

    private void UpdateRequirementText()
    {
        if (woodText != null)
        {
            woodText.text =
                woodDelivered + " / " + woodRequired;
        }

        if (rockText != null)
        {
            rockText.text =
                rockDelivered + " / " + rockRequired;
        }

        if (vinesText != null)
        {
            vinesText.text =
                vinesDelivered + " / " + vinesRequired;
        }
    }

    private void CheckIfBuilt()
    {
        if (woodDelivered >= woodRequired &&
            rockDelivered >= rockRequired &&
            vinesDelivered >= vinesRequired)
        {
            Build();
        }
    }

    private void Build()
    {
        IsBuilt = true;



        if (blueprintObject != null)
            blueprintObject.SetActive(false);


        if (woodText != null)
            woodText.gameObject.SetActive(false);

        if (rockText != null)
            rockText.gameObject.SetActive(false);

        if (vinesText != null)
            vinesText.gameObject.SetActive(false);


        if (finishedObject != null)
            finishedObject.SetActive(true);

  

        if (taskText != null)
        {
            taskText.color = Color.green;
        }

        if (taskImage != null)
        {
            taskImage.color = Color.green;
        }

 

        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);



        NotifyNextBuildable();
    }

    private void NotifyNextBuildable()
    {
        BuildableObject[] allBuildables =
            FindObjectsByType<BuildableObject>(
                FindObjectsSortMode.None
            );

        foreach (BuildableObject buildable in allBuildables)
        {
            if (buildable.requiredBuildable == this)
            {
                buildable.UpdateTask();
            }
        }
    }

    private void UpdateTask()
    {
        if (taskText == null)
            return;


        if (IsBuilt)
        {
            taskText.color = Color.green;

            if (taskImage != null)
                taskImage.color = Color.green;

            return;
        }



        if (requiredBuildable != null)
        {
            if (requiredBuildable.IsBuilt)
            {
              
                taskText.color = Color.white;

                if (taskImage != null)
                    taskImage.color = Color.white;
            }
            else
            {
           
                taskText.color = Color.gray;

                if (taskImage != null)
                    taskImage.color = Color.gray;
            }
        }
        else
        {
      
            taskText.color = Color.white;

            if (taskImage != null)
                taskImage.color = Color.white;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);
    }
}