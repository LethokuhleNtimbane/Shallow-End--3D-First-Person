using UnityEngine;
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


    private int woodDelivered;
    private int rockDelivered;
    private int vinesDelivered;

    private Inventory inventory;

    private void Start()
    {
        finishedObject.SetActive(false);

        inventory = FindFirstObjectByType<Inventory>();

        UpdateRequirementText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }

        TryDeliverMaterials();
    }

    private void TryDeliverMaterials()
    {
        if (inventory == null)
        {
          
            return;
        }

        int woodNeeded = woodRequired - woodDelivered;

        if (woodNeeded > 0)
        {
            int playerWood = inventory.GetTotalItemAmount(woodItem);

            int amountToTake = Mathf.Min(playerWood, woodNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(woodItem, amountToTake);

                woodDelivered += amountToTake;
            }
        }


        int rockNeeded = rockRequired - rockDelivered;

        if (rockNeeded > 0)
        {
            int playerRock = inventory.GetTotalItemAmount(rockItem);

            int amountToTake = Mathf.Min(playerRock, rockNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(rockItem, amountToTake);

                rockDelivered += amountToTake;
            }
        }


        int vinesNeeded = vinesRequired - vinesDelivered;

        if (vinesNeeded > 0)
        {
            int playerVines = inventory.GetTotalItemAmount(vinesItem);

            int amountToTake = Mathf.Min(playerVines, vinesNeeded);

            if (amountToTake > 0)
            {
                inventory.RemoveItemAmount(vinesItem, amountToTake);

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
            woodText.text = woodDelivered + " / " + woodRequired;
        }

        if (rockText != null)
        {
            rockText.text = rockDelivered + " / " + rockRequired;
        }

        if (vinesText != null)
        {
            vinesText.text = vinesDelivered + " / " + vinesRequired;
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
        blueprintObject.SetActive(false);

        if (woodText != null)
            woodText.gameObject.SetActive(false);

        if (rockText != null)
            rockText.gameObject.SetActive(false);

        if (vinesText != null)
            vinesText.gameObject.SetActive(false);

        finishedObject.SetActive(true);

   
    }
}