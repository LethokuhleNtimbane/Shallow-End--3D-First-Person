using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class SpearAttack : MonoBehaviour
{
 
    [SerializeField] private Inventory inventory;


    [SerializeField] private InputActionReference interactAction;


    [SerializeField] private Camera playerCamera;

    
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float damage = 10f;

  
    [SerializeField] private Items crabMeat;


    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageCoroutine;

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

        // Find the parent with the Crab tag
        while (crab != null && !crab.CompareTag("Crab"))
        {
            crab = crab.parent;
        }

        if (crab == null)
            return;

        // Player has spear
        if (inventory != null && inventory.IsSpearEquipped())
        {
            KillCrab(crab);
            return;
        }

        // Player does not have spear
        ShowMessage("Ouch! I need a spear");

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
        if (PlayerController.Instance == null)
            return;

        HealthScript health =
            PlayerController.Instance.GetComponent<HealthScript>();

        if (health == null)
            return;

        health.TakeDamage(damage);
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;

        
        messageText.gameObject.SetActive(true);

      
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(HideMessage());
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        messageCoroutine = null;
    }
}