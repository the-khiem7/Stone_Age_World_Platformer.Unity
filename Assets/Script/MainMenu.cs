#define UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the game scene (assuming it's named "GameScene")
        SceneManager.LoadSceneAsync(1);
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();

        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
