using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingBench : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference craftAct;

    [Header("UI")]
    [SerializeField] private GameObject GcraftingSystem;
    [SerializeField] private GameObject inventoryUI;


    [SerializeField] private GameObject inventoryContainer;

    private bool playerNearby = false;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (craftAct.action.WasPressedThisFrame())
        {
            ToggleCrafting();
        }
    }

    private void ToggleCrafting()
    {
        if (GcraftingSystem == null)
            return;

        bool shouldOpen = !GcraftingSystem.activeSelf;

        GcraftingSystem.SetActive(shouldOpen);

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(shouldOpen);
        }

        if (inventoryContainer != null)
        {
            inventoryContainer.SetActive(shouldOpen);
        }

        UIManager.Instance.SetCraftingOpen(shouldOpen);

        if (shouldOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            CloseCrafting();
        }
    }

    private void CloseCrafting()
    {
        if (GcraftingSystem != null)
        {
            GcraftingSystem.SetActive(false);
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        UIManager.Instance.SetCraftingOpen(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}

