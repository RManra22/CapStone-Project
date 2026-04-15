using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class ButtonManager : MonoBehaviour
{

    public GameObject loadingUI; // Reference to the loading UI GameObject
    // This script is responsible for allowing the player to navigate the menus as per the functional requirements. 
    public void LoadScene(string sceneName) // load the correct scene based on the name given
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneWithLoadingScreen(string sceneName) // load the correct scene based on the name given
    {
       loadingUI.SetActive(true); // Show the loading UI
        StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!operation.isDone)
        {
            yield return null;
        }
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
