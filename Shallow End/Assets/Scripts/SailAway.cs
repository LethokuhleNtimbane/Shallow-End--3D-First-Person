using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RaftSailInteraction : MonoBehaviour
{

    [SerializeField] private InputActionReference interactAction;

    [SerializeField] private TextMeshProUGUI sailText;


    [SerializeField] private GameObject playerHUD;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TimeController timeController;
    [SerializeField] private Monster monster;
    [SerializeField] private MonsterAttack monsterAttack;
    [SerializeField] private HealthScript Health;


    [SerializeField] private GameObject sailingObject;

    private bool playerInRange = false;
    private bool sailing = false;

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
       
        if (sailText != null)
            sailText.gameObject.SetActive(false);

    
        if (sailingObject != null)
            sailingObject.SetActive(false);
    }

    private void Update()
    {
        
        if (!playerInRange)
            return;

        
        if (sailing)
            return;


        if (interactAction == null)
            return;

    
        if (interactAction.action.WasPressedThisFrame())
        {
            SailAway();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

   
        if (sailText != null)
        {
            sailText.text = "Press E to sail away";
            sailText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

     
        if (sailText != null)
            sailText.gameObject.SetActive(false);
    }

    private void SailAway()
    {
        sailing = true;



 
        if (sailText != null)
            sailText.gameObject.SetActive(false);

    
        if (playerHUD != null)
            playerHUD.SetActive(false);

    
        if (playerController != null)
            playerController.PlayerControl(false);

   
        if (timeController != null)
            timeController.enabled = false;

        
        if (monster != null)
            monster.enabled = false;

     
        if (monsterAttack != null)
            monsterAttack.enabled = false;
        if (Health != null)
        Health.enabled = false;

       

   
        if (sailingObject != null)
            sailingObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}