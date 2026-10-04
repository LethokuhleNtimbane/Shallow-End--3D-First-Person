using UnityEngine;
using UnityEngine.InputSystem;

public class Book : MonoBehaviour
{
    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject playerHUD;

    [SerializeField] private InputActionReference openBookAction;

    [SerializeField] private MonoBehaviour timeController;
    [SerializeField] private MonoBehaviour monsterSpawner;
    [SerializeField] private MonoBehaviour monsterAttack;
    [SerializeField] private HealthScript health;
    [SerializeField] private DashScript dash;
    [SerializeField] private PlayerController playerController;

    private bool bookOpen = false;

    private void OnEnable()
    {
        if (openBookAction != null)
            openBookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (openBookAction != null)
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

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (openBookAction == null)
            return;

        if (openBookAction.action.WasPressedThisFrame())
        {
            if (!bookOpen)
            {
                OpenBook();
            }
            else
            {
                CloseBook();
            }
        }
    }

    public void OpenBook()
    {
        if (bookOpen)
            return;

        bookOpen = true;

        Time.timeScale = 0f;

        if (bookUI != null)
            bookUI.SetActive(true);

        if (playerHUD != null)
            playerHUD.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DisableGameplay();
    }

    public void CloseBook()
    {
        if (!bookOpen)
            return;

        bookOpen = false;

        Time.timeScale = 1f;

        if (bookUI != null)
            bookUI.SetActive(false);

        if (playerHUD != null)
            playerHUD.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        EnableGameplay();
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
}