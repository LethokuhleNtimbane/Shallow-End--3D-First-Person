using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Animator transition;

    public void LoadScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadLevel(int LevelIndex)
    {
        // Play transition animation
        transition.SetTrigger("start");

        // Wait for animation
        yield return new WaitForSeconds(1);

        // Load next scene
        SceneManager.LoadScene(LevelIndex);
    }
}