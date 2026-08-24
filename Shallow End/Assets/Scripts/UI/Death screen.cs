using UnityEngine;

public class GameOverManager : MonoBehaviour
{
  
    [SerializeField] private HealthScript playerHealth;
    [SerializeField] private PlayerController playerController;


    [SerializeField] private Monster monster;
    [SerializeField] private TimeController timeController;


    [SerializeField] private GameObject DeathScreen;

    [SerializeField] private PauseScript Pausemenu;

    [SerializeField] private GameObject PauseCanva;

    [SerializeField] private GameObject PlayerHud;

    private bool gameOver = false;

    private void Start()
    {
  
        if (DeathScreen != null)
        {
            DeathScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameOver)
            return;

        if (playerHealth == null)
            return;

        if (playerHealth.health <= 0f)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        gameOver = true;

     

   
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (monster != null)
        {
            monster.enabled = false;
        }


        if (timeController != null)
        {
            timeController.enabled = false;
        }

    
        if (DeathScreen != null)
        {
            DeathScreen.SetActive(true);
        }
        if (DeathScreen != null)
        {
            Pausemenu.enabled = false;
        }
        PlayerHud.SetActive(false);
        PauseCanva.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}