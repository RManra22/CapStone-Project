using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
public class ButtonManager : MonoBehaviour
{
    // This script is responsible for allowing the player to navigate the menus as per the functional requirements. 
    public void LoadScene(string sceneName) // load the correct scene based on the name given
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    // This script is responsible for allowing the player to exit the game as per the functional requirements.
    public void ExitGame() // quit the application from any menu
    {
        Application.Quit();
    }

    // This script is responsible for allowing the player to unpause the game as per the functional requirements.
    public void UnpauseGame()
    {
        GameManager.Instance.ResumeGame();
    }
}
