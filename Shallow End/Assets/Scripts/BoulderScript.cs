using UnityEngine;
using UnityEngine.InputSystem;

public class BoulderScript : MonoBehaviour
{
    [SerializeField] private InputActionReference hammerAction;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Items rockItem;
    [SerializeField] private float hitRange = 3f;

    private void OnEnable()
    {
        if (hammerAction != null)
            hammerAction.action.Enable();
    }

    private void OnDisable()
    {
        if (hammerAction != null)
            hammerAction.action.Disable();
    }

    private void Update()
    {
        if (hammerAction == null)
            return;

        if (!hammerAction.action.WasPressedThisFrame())
            return;

        TryHitBoulder();
    }

    private void TryHitBoulder()
    {
        if (inventory == null || playerCamera == null)
            return;

        if (!inventory.IsHammerEquipped())
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, hitRange))
            return;

        if (hit.collider.transform != transform &&
            !hit.collider.transform.IsChildOf(transform))
        {
            return;
        }

        CollectRock();
    }

    private void CollectRock()
    {
        if (rockItem == null)
            return;

        bool collected = inventory.AddItem(rockItem, 1);

        if (!collected)
            return;

        inventory.UseEquippedDurability();
        inventory.UseEquippedDurability();
    }
}