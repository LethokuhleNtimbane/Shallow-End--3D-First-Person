using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingBench : MonoBehaviour
{
    [SerializeField] private InputActionReference craftAct;
    public GameObject GcraftingSystem;
    private bool playerNearby = false;

    private void Update()
    {
        if (!playerNearby) return;

        if (UIManager.Instance.IsAnyUIOpen()) return;

        if (craftAct.action.WasPressedThisFrame())
        {
            TCrafting();
        }
    }
    private void TCrafting()
    {
        if (GcraftingSystem == null) return;

        bool isOpen = GcraftingSystem.activeSelf;

        GcraftingSystem.SetActive(!isOpen);

        Cursor.visible = !isOpen;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        
            UIManager.Instance.SetCraftingOpen(isOpen);
       
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

          if (GcraftingSystem != null) 
            { 

            {
             GcraftingSystem.SetActive(false);
            }
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                UIManager.Instance.SetCraftingOpen(false);
             
            }
             
        }
    }
}

