using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
   public void LoadScene(string Island)
    {
        SceneManager.LoadScene(Island);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
