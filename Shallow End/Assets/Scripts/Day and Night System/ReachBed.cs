using UnityEngine;
using UnityEngine.InputSystem;

public class Bed : MonoBehaviour
{
    [SerializeField] private SleepManager sleepManager;
    [SerializeField] private InputActionReference interactAction;

    private bool playerNearby;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerNearby)
            return;

        sleepManager.TrySleep();
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
        }
    }
}