using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.Instance.LoadSceneWithLoading("Tutorial3DCart");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Credits");
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        GameManager.Instance.LoadSceneWithLoading("MainMenu");
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.RestartScene();
    }
}
