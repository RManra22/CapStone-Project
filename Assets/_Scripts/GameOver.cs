using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Start() {
        int finalScore = PlayerPrefs.GetInt("LastScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0); 

        finalScoreText.text = "Score: " + finalScore;
        highScoreText.text = "Best: " + highScore;
    }

    public void PlayAgain() {
    string lastScene = PlayerPrefs.GetString("LastScene", "GameMode1");
    SceneManager.LoadScene(lastScene);
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MainMenu");
    }

}   