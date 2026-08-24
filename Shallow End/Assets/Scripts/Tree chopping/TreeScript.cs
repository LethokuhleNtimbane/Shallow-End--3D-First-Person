using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TreeScript : MonoBehaviour
{
  
    [SerializeField] private Inventory inventory;
    [SerializeField] private TimeController timeController;

  
    [SerializeField] private GameObject tree;

  
    [SerializeField] private InputActionReference interactAction;


    [SerializeField] private Items woodItem;
    [SerializeField] private Items coconutItem;
    [SerializeField] private Items vineItems;

    [SerializeField] private int minimumDrop = 1;
    [SerializeField] private int maximumDrop = 4;
    [SerializeField] private float dropRange = 2f;


    [SerializeField] private int ReGrow = 6;

    private bool isChopped;
    private DateTime choppedTime;

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
        if (timeController != null)
        {
            timeController.OnHourChanged += CheckRegrowth;
        }
    }

    private void OnDestroy()
    {
        if (timeController != null)
        {
            timeController.OnHourChanged -= CheckRegrowth;
        }
    }

    private void Update()
    {
        if (interactAction == null)
            return;

        if (interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (isChopped)
            return;

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }

        if (inventory == null)
            return;

        if (Camera.main == null)
            return;

        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, 3f))
            return;

       
        if (hit.transform == transform ||
            hit.transform.IsChildOf(transform))
        {
           
            if (!inventory.IsAxeEquipped())
            {
                inventory.ShowInteractionMessage("I need an axe");
                return;
            }

        
            ChopTree();
        }
    }

    private void ChopTree()
    {
        isChopped = true;

        if (timeController != null)
        {
            choppedTime = timeController.CurrentTime;
        }

        SpawnDrops();

        if (tree != null)
        {
            tree.SetActive(false);
        }
    }

    private void SpawnDrops()
    {
        SpawnDrop(woodItem);
        SpawnDrop(coconutItem);
        SpawnDrop(vineItems);
    }

    private void SpawnDrop(Items item)
    {
        if (item == null)
            return;

        if (item.ItenPrefab == null)
            return;

        int amount = UnityEngine.Random.Range(
            minimumDrop,
            maximumDrop + 1
        );

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    UnityEngine.Random.Range(
                        -dropRange,
                        dropRange
                    ),
                    0.5f,
                    UnityEngine.Random.Range(
                        -dropRange,
                        dropRange
                    )
                );

            GameObject droppedObject = Instantiate(
                item.ItenPrefab,
                randomPosition,
                Quaternion.identity
            );

            GroundItem groundItem =
                droppedObject.GetComponent<GroundItem>();

            if (groundItem != null)
            {
                groundItem.item = item;
                groundItem.amount = 1;
            }
        }
    }

    private void CheckRegrowth()
    {
        if (!isChopped)
            return;

        if (timeController == null)
            return;

        TimeSpan timepassed =
            timeController.CurrentTime - choppedTime;

        if (timepassed.TotalHours >= ReGrow)
        {
            RegrowTree();
        }
    }

    private void RegrowTree()
    {
        isChopped = false;

        if (tree != null)
        {
            tree.SetActive(true);
        }
    }
}