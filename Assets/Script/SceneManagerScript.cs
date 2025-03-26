using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Game Screen");
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
        SceneManager.LoadScene("MainMenu");
    }

    public void Replay()
    {
        SceneManager.LoadScene("__"); // to replay , add the scenename to " " 
    }

    public void Skip()
    {
        SceneManager.LoadScene("__"); // to skip tutorial, add the scenename to " " 
    }
}
