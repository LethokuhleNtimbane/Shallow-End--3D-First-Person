using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ThomasInteraction : MonoBehaviour
{
   
    [SerializeField] private InputActionReference interactAction;

   
    [SerializeField] private GameObject thomas;


    [SerializeField] private TextMeshProUGUI interactionText;

   
    [SerializeField] private TextMeshProUGUI dialogueText;

  
    [SerializeField] private string randomLine1 = "What you leave behind... the crabs shall find";
    [SerializeField] private string randomLine2 = "Do not believe what your eyes see in the dark";
    [SerializeField] private string randomLine3 = "The light only scares it away but the darkness will always remain";
    [SerializeField] private string randomLine4 = "The sea is a hunting ground, it is not a place of freedom";
    [SerializeField] private string randomLine5 = "The Island is beyond cruelty";


    [SerializeField] private TimeController timeController;

    private bool playerInRange = false;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }

    private void Start()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);

        UpdateThomasVisibility();
    }

    private void Update()
    {
        UpdateThomasVisibility();

        if (!playerInRange)
            return;

        if (interactAction == null)
            return;

        if (!interactAction.action.WasPressedThisFrame())
            return;

        InteractWithThomas();
    }

    private void InteractWithThomas()
    {
        float currentHour = GetCurrentHour();

      

        if (currentHour >= 21f || currentHour < 6f)
        {
            if (interactionText != null)
                interactionText.gameObject.SetActive(false);

            if (dialogueText != null)
            {
                dialogueText.text = "He is probably asleep";
                dialogueText.gameObject.SetActive(true);
            }

            return;
        }

      

        if (currentHour >= 6f && currentHour < 21f)
        {
            ShowRandomDialogue();
        }
    }

    private void ShowRandomDialogue()
    {
        if (dialogueText == null)
            return;

        string[] dialogueLines =
        {
            randomLine1,
            randomLine2,
            randomLine3,
            randomLine4,
            randomLine5
        };

        int randomIndex = Random.Range(0, dialogueLines.Length);

        dialogueText.text = dialogueLines[randomIndex];
        dialogueText.gameObject.SetActive(true);
    }

    private void UpdateThomasVisibility()
    {
        if (thomas == null)
            return;

        float currentHour = GetCurrentHour();

        
        if (currentHour >= 21f || currentHour < 6f)
        {
            thomas.SetActive(false);
        }
       
        else
        {
            thomas.SetActive(true);
        }
    }

    private float GetCurrentHour()
    {
        if (timeController == null)
            return 12f;

        return (float)timeController.CurrentTime.TimeOfDay.TotalHours;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        float currentHour = GetCurrentHour();

 
        if (interactionText != null)
        {
            interactionText.text = "Press E to interact with Thomas";
            interactionText.gameObject.SetActive(true);
        }

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

     

     
    }
}