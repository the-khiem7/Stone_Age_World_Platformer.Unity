#define UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenSettings()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void OpenGuide()
    {
        SceneManager.LoadSceneAsync(3);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
