using UnityEngine;
using UnityEngine.InputSystem;

public class CoconutSplitter : MonoBehaviour
{ 
    [SerializeField] private InputActionReference LeftClickSplit;


    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera playerCamera;

    
    [SerializeField] private Items wholeCoconut;

  
    [SerializeField] private GameObject coconut;

    
    [SerializeField] private float splitRange = 3f;

    private void OnEnable()
    {
        LeftClickSplit.action.Enable();
    }

    private void OnDisable()
    {
        LeftClickSplit.action.Disable();
    }

    private void Update()
    {
        if (!LeftClickSplit.action.WasPressedThisFrame())
            return;

        TrySplitCoconut();
    }

    private void TrySplitCoconut()
    {
        
        if (inventory == null)
        {
        
            return;
        }

        
        if (playerCamera == null)
        {
         
            return;
        }

        
        if (!inventory.IsHammerEquipped())
        {
          
            return;
        }


        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, splitRange))
        {
     

            GroundItem groundItem =
                hit.collider.GetComponent<GroundItem>();

            if (groundItem == null)
            {
      
                return;
            }

           
            if (groundItem.item != wholeCoconut)
            {
        
                return;
            }

            SplitCoconut(groundItem.gameObject);
        }
        else
        {
         
        }
    }

    private void SplitCoconut(GameObject wholeCoconutObject)
    {
        if (coconut == null)
        {
           
            return;
        }

        Vector3 position = wholeCoconutObject.transform.position;

        Destroy(wholeCoconutObject);

      
        Instantiate(
            coconut,
            position + Vector3.right * 0.3f,
            Quaternion.identity
        );

      
        Instantiate(
            coconut,
            position + Vector3.left * 0.3f,
            Quaternion.identity
        );

      
    }
}