using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LookAtObject : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float raycastDistance = 3f;


    [SerializeField] private Image objectImage;
    [SerializeField] private TextMeshProUGUI objectText;


    [SerializeField] private int lookedAtLayer = 9;


    private GameObject currentObject;
    private int originalLayer;

    private void Start()
    {
        if (objectImage != null)
            objectImage.gameObject.SetActive(false);

        if (objectText != null)
            objectText.gameObject.SetActive(false);
    }

    private void Update()
    {
        DetectLookedAtObject();
    }

    private void DetectLookedAtObject()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            raycastDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Rocks"))
            {
                LookAtObjectFound(hitObject, "To pickup Rock");
                return;
            }

            if (hitObject.CompareTag("Vines"))
            {
                LookAtObjectFound(hitObject, "To pickup Vines");
                return;
            }

            if (hitObject.CompareTag("Wood"))
            {
                LookAtObjectFound(hitObject, "To pickup Wood");
                return;
            }

            if (hitObject.CompareTag("Knife"))
            {
                LookAtObjectFound(hitObject, "To pickup Knife");
                return;
            }

            if (hitObject.CompareTag("Hammer"))
            {
                LookAtObjectFound(hitObject, "To pickup Hammer");
                return;
            }

            if (hitObject.CompareTag("Whole Coconut"))
            {
                LookAtObjectFound(hitObject, "To pickup Coconut");
                return;
            }

            if (hitObject.CompareTag("CraftingBench"))
            {
                LookAtObjectFound(hitObject, "To use Crafting Bench");
                return;
            }

            if (hitObject.CompareTag("Flint"))
            {
                LookAtObjectFound(hitObject, "To pickup Flint");
                return;
            }

            if (hitObject.CompareTag("T.M"))
            {
                LookAtObjectFound(hitObject, "To interact");
                return;
            }

            if (hitObject.CompareTag("Bed"))
            {
                LookAtObjectFound(hitObject, "To sleep");
                return;
            }
            if (hitObject.CompareTag("Chest"))
            {
                LookAtObjectFound(hitObject, "To open");
                return;
            }



            if (hitObject.CompareTag("Fireplace"))
            {
                LookAtObjectFound(hitObject, "To use Fireplace");
                return;
            }
        }

        StopLookingAtObject();
    }

    private void LookAtObjectFound(
        GameObject target,
        string displayName)
    {
        if (currentObject != target)
        {
            StopLookingAtObject();

            currentObject = target;

     
            originalLayer = currentObject.layer;

      
            currentObject.layer = 9;
        }

        if (objectImage != null)
            objectImage.gameObject.SetActive(true);

        if (objectText != null)
        {
            objectText.text = displayName;
            objectText.gameObject.SetActive(true);
        }
    }

    private void StopLookingAtObject()
    {
        if (currentObject != null)
        {
           
            currentObject.layer = originalLayer;

            currentObject = null;
        }

        if (objectImage != null)
            objectImage.gameObject.SetActive(false);

        if (objectText != null)
        {
            objectText.text = "";
            objectText.gameObject.SetActive(false);
        }
    }
}