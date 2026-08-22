using UnityEngine;
using UnityEngine.InputSystem;

public class Fireplace : MonoBehaviour
{
    [SerializeField] private GameObject fireMat;
    [SerializeField] private Light fireLight;
    [SerializeField] private FireProtect fireProtection;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Items flint;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private GameObject fireDamageZone;
    [SerializeField] private GameObject protectionZone;

    private bool fireIsLit = false;

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
        // Make absolutely sure everything starts OFF.
        TurnFireOff();
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        CheckMorning();

        if (fireIsLit)
            return;

        CheckForFireInteraction();
    }

    private void CheckForFireInteraction()
    {
        if (PlayerController.Instance == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        if (distance > interactionRange)
            return;

        if (!IsNightTime())
            return;

        if (!HasFlintEquipped())
            return;

        if (interactAction != null &&
            interactAction.action.IsPressed())
        {
            LightFire();
        }
    }

    private bool HasFlintEquipped()
    {
        if (inventory == null)
            return false;

        Items equippedItem = inventory.GetHotbarItem();

        return equippedItem == flint;
    }

    private bool IsNightTime()
    {
        int hour = TimeController.instance.CurrentTime.Hour;

        return hour >= 20 || hour < 6;
    }

    private void LightFire()
    {
        fireIsLit = true;

        if (fireMat != null)
        {
            fireMat.SetActive(true);
        }

        if (fireLight != null)
        {
            fireLight.enabled = true;
        }

        // Monster protection ON
        if (protectionZone != null)
        {
            protectionZone.SetActive(true);
        }

        // Fire damage ON
        if (fireDamageZone != null)
        {
            fireDamageZone.SetActive(true);
        }

        if (fireProtection != null)
        {
            fireProtection.SetFire(true);
        }

        // Consume one Flint
        if (inventory != null)
        {
            inventory.RemoveHotbarItem(1);
        }
    }

    private void TurnFireOff()
    {
        fireIsLit = false;

        if (fireMat != null)
        {
            fireMat.SetActive(false);
        }

        if (fireLight != null)
        {
            fireLight.enabled = false;
        }

        // Monster protection OFF
        if (protectionZone != null)
        {
            protectionZone.SetActive(false);
        }

        // Fire damage OFF
        if (fireDamageZone != null)
        {
            fireDamageZone.SetActive(false);
        }

        if (fireProtection != null)
        {
            fireProtection.SetFire(false);
        }
    }

    private void CheckMorning()
    {
        if (!fireIsLit)
            return;

        int hour = TimeController.instance.CurrentTime.Hour;

        if (hour >= 6 && hour < 20)
        {
            TurnFireOff();
        }
    }
}