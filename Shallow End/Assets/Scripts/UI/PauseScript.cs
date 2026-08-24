using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static PauseScript Instance;


    [SerializeField] private GameObject pauseMenu;


    [SerializeField] private TimeController timeController;
    [SerializeField] private Monster monster;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthScript healthScript;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Instance = this;
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
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    public void RestartScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;


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

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

 
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