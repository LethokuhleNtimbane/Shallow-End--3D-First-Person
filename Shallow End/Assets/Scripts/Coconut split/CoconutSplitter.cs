using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CoconutSplitter : MonoBehaviour
{
    [SerializeField] private InputActionReference LeftClickSplit;

    [SerializeField] private Inventory inventory;

    [SerializeField] private Camera playerCamera;

    [SerializeField] private Items wholeCoconut;

    [SerializeField] private GameObject coconut;

    [SerializeField] private float splitRange = 3f;

    [SerializeField] private TextMeshProUGUI messageText;

    private void OnEnable()
    {
        if (LeftClickSplit != null)
            LeftClickSplit.action.Enable();
    }

    private void OnDisable()
    {
        if (LeftClickSplit != null)
            LeftClickSplit.action.Disable();
    }

    private void Update()
    {
        if (LeftClickSplit == null)
            return;

        if (!LeftClickSplit.action.WasPressedThisFrame())
            return;

        TrySplitCoconut();
    }


    private void TrySplitCoconut()
    {
        Debug.Log("Coconut splitter: Click detected.");

        if (inventory == null || playerCamera == null)
        {
            Debug.LogWarning("Coconut splitter: Inventory or camera is missing.");
            return;
        }

        Debug.Log("Hammer equipped: " + inventory.IsHammerEquipped());

        if (!inventory.IsHammerEquipped())
        {
            ShowMessage("I need a hammer");
            Debug.Log("Coconut splitter: Hammer is not equipped.");
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, splitRange))
        {
            Debug.Log("Coconut splitter hit: " + hit.collider.name);

            GroundItem groundItem =
                hit.collider.GetComponent<GroundItem>();

            if (groundItem == null)
            {
                Debug.Log("Coconut splitter: Hit object has no GroundItem.");
                return;
            }

            Debug.Log("Hit item: " + groundItem.item);

            if (groundItem.item != wholeCoconut)
            {
                Debug.Log("Coconut splitter: Hit item is not a whole coconut.");
                return;
            }

            if (coconut == null)
            {
                Debug.LogError("Coconut splitter: Coconut prefab is missing.");
                return;
            }

            SplitCoconut(groundItem.gameObject);

            inventory.UseEquippedDurability();

            Debug.Log("Coconut splitter: Coconut split successfully.");
        }
        else
        {
            Debug.Log("Coconut splitter: Raycast did not hit anything.");
        }
    }


    private void SplitCoconut(
        GameObject wholeCoconutObject)
    {
        if (coconut == null)
            return;

        Vector3 position =
            wholeCoconutObject.transform.position;

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

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }
}