
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static PauseScript Instance;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Gameplay Scripts")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private Monster monster;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthScript healthScript;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Return to the main menu.
    // This does NOT reset the current playthrough.
    public void MainMenu()
    {
        Time.timeScale = 1f;

        isPaused = false;
        WoodenBox.StartNewGame();
        SceneManager.LoadScene("MainMenuScene");
    }

    // Restart the current gameplay scene.
    // This starts a new playthrough.
    public void RestartScene()
    {
        Time.timeScale = 1f;

        isPaused = false;

        // Reset chest rewards and journal unlock.
        WoodenBox.StartNewGame();

        // Reload the current gameplay scene.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Pause the game.
    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        if (timeController != null)
            timeController.enabled = false;

        if (monster != null)
            monster.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Resume the game.
    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        if (timeController != null)
            timeController.enabled = true;

        if (monster != null)
            monster.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
