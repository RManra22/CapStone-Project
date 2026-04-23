/*
    This script is responsible for allowing the
    player to navigate the menus as per the functional requirements.
    
    This script is also responsible for allowing the
    player to exit the game as per the functional requirements.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class ButtonManager : MonoBehaviour
{

    public GameObject loadingUI; // Reference to the loading UI GameObject
    public void LoadScene(string sceneName) // load the correct scene based on the name given
    {
        Time.timeScale = 1f; // Ensure time is running at normal speed when loading a new scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneWithLoadingScreen(string sceneName) // load the correct scene based on the name given
    {
       Time.timeScale = 1f; // Ensure time is running at normal speed when loading a new scene
       loadingUI.SetActive(true); // Show the loading UI
       StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent the scene from activating immediately

        float minimumLoadingTime = 1f; // Minimum time to show the loading screen
        float elapsedTime = 0f;

        while (elapsedTime < minimumLoadingTime || !operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            // Scene is ready but we're still waiting for minimum time
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                operation.allowSceneActivation = true; // Now let the scene load
            }
            yield return null; // Wait for the next frame
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

    public void UnpauseGameClassic()
    {
        ClassicGameManager.Instance.ResumeGame();
    }
}
