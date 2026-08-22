using UnityEngine;
using UnityEngine.InputSystem;

public class SpearAttack : MonoBehaviour
{

    [SerializeField] private Inventory inventory;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Camera playerCamera;

   
    [SerializeField] private float attackRange = 4f;

 
    [SerializeField] private Items crabMeat;


    [SerializeField] private float damage = 10f;

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

    private void Update()
    {
        if (interactAction == null)
            return;

        if (!interactAction.action.WasPressedThisFrame())
            return;

        CheckCrab();
    }

    private void CheckCrab()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            attackRange))
        {
            return;
        }

        
        Transform crab = hit.collider.transform;

        while (crab != null && !crab.CompareTag("Crab"))
        {
            crab = crab.parent;
        }

   
        if (crab == null)
            return;


        if (inventory != null && inventory.IsSpearEquipped())
        {
            KillCrab(crab);
            return;
        }

      

        HurtPlayer();

        
    }

    private void KillCrab(Transform crab)
    {
        if (inventory == null)
            return;

   
        bool receivedMeat = inventory.AddItem(crabMeat, 1);

        if (!receivedMeat)
        {
            
            return;
        }

      

        Destroy(crab.gameObject);
    }

    private void HurtPlayer()
    {
        HealthScript health =
            PlayerController.Instance.GetComponent<HealthScript>();

        if (health == null)
        {
          

            return;
        }

        health.TakeDamage(damage);

 
    }
}