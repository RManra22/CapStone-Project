using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    // On start, unlock the cursor and display the final score and high score retrieved from PlayerPrefs
    private void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        int finalScore = PlayerPrefs.GetInt("LastScore", 0);
        string highScoreKey = PlayerPrefs.GetString("LastHighScoreKey", "HighScore");
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        int creditsEarned = PlayerPrefs.GetInt("CreditsEarned", 0);
        int totalCredits = PlayerPrefs.GetInt("TotalCredits", 0);

        finalScoreText.text = "Score: " + finalScore;
        highScoreText.text = "Best: " + highScore;
    }

    public void PlayAgain() {
        string lastScene = PlayerPrefs.GetString("LastScene", "GameScene");
        SceneManager.LoadScene(lastScene);
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}