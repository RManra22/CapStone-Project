using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
public class MainMenuManager : MonoBehaviour
{
    public void LoadScene(string sceneName) // load the correct scene based on the name given
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ExitGame() // quit the application from any menu
    {
        Application.Quit();
    }

    /*
    public void UnpauseGame() // unpause the game from the pause menu
    {
        GameManager.Instance.ResumeGame();
    }
    */
}
