using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeAttack : MonoBehaviour
{

    [SerializeField] private Inventory inventory;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private Camera playerCamera;

 
    [SerializeField] private float attackRange = 3f;


    [SerializeField] private float stunDuration = 3f;

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.Disable();
        }
    }

    private void Update()
    {
        if (attackAction == null)
            return;

        if (!attackAction.action.WasPressedThisFrame())
            return;

   
        if (inventory == null || !inventory.IsKnifeEquipped())
        {
      
            return;
        }

        StabMonster();
    }

    private void StabMonster()
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

        Monster monster =
            hit.collider.GetComponentInParent<Monster>();

        if (monster == null)
        {
          
            return;
        }

        

        monster.Stun(stunDuration);
    }
}