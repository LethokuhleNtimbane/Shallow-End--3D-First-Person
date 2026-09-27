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

        transition.SetTrigger("start");

        yield return new WaitForSeconds(1);


        SceneManager.LoadScene(LevelIndex);
    }
}