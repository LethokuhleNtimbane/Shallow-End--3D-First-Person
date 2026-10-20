
using UnityEngine;
using UnityEngine.InputSystem;

public class BookManager : MonoBehaviour
{
    [Header("Book UI")]
    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject playerHUD;

    [Header("Input")]
    [SerializeField] private InputActionReference openBookAction;

    [Header("Gameplay Scripts")]
    [SerializeField] private MonoBehaviour timeController;
    [SerializeField] private MonoBehaviour monsterSpawner;
    [SerializeField] private MonoBehaviour monsterAttack;
    [SerializeField] private HealthScript health;
    [SerializeField] private DashScript dash;
    [SerializeField] private PlayerController playerController;

    [Header("Journal Unlock")]
    public bool journalUnlocked = false;

    public static bool IsBookOpen { get; private set; }

    private bool bookOpen;

    private void Awake()
    {
        bookOpen = false;
        IsBookOpen = false;
    }

    private void OnEnable()
    {
        if (openBookAction != null && openBookAction.action != null)
            openBookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (bookOpen)
            CloseBook();

        if (openBookAction != null && openBookAction.action != null)
            openBookAction.action.Disable();
    }

    private void Start()
    {
        if (bookUI != null)
            bookUI.SetActive(false);

        if (playerHUD != null)
            playerHUD.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (openBookAction == null || openBookAction.action == null)
            return;

        if (!openBookAction.action.enabled)
            return;

        if (!openBookAction.action.WasPressedThisFrame())
            return;

        if (!journalUnlocked)
            return;

        if (bookOpen)
            CloseBook();
        else
            OpenBook();
    }

    public void OpenBook()
    {
        if (bookOpen || !journalUnlocked)
            return;

        bookOpen = true;
        IsBookOpen = true;

        if (bookUI != null)
            bookUI.SetActive(true);

        if (playerHUD != null)
            playerHUD.SetActive(false);

        DisableGameplay();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseBook()
    {
        if (!bookOpen)
            return;

        bookOpen = false;
        IsBookOpen = false;

        if (bookUI != null)
            bookUI.SetActive(false);

        if (playerHUD != null)
            playerHUD.SetActive(true);

        EnableGameplay();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DisableGameplay()
    {
        if (timeController != null)
            timeController.enabled = false;

        if (monsterSpawner != null)
            monsterSpawner.enabled = false;

        if (monsterAttack != null)
            monsterAttack.enabled = false;

        if (health != null)
            health.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (playerController != null)
            playerController.enabled = false;
    }

    private void EnableGameplay()
    {
        // Do not resume gameplay while another interface is open.
        if (CraftingBench.IsCraftingOpen || ChestIsOpen())
            return;

        if (timeController != null)
            timeController.enabled = true;

        if (monsterSpawner != null)
            monsterSpawner.enabled = true;

        if (monsterAttack != null)
            monsterAttack.enabled = true;

        if (health != null)
            health.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (playerController != null)
            playerController.enabled = true;
    }

    private bool ChestIsOpen()
    {
        WoodenBox[] boxes = FindObjectsByType<WoodenBox>(
            FindObjectsSortMode.None
        );

        foreach (WoodenBox box in boxes)
        {
            if (box != null && box.IsOpen)
                return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (bookOpen)
        {
            bookOpen = false;
            IsBookOpen = false;
            EnableGameplay();
        }
    }
}
