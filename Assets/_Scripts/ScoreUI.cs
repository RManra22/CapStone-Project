using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour {
  [SerializeField] private TextMeshProUGUI scoreText;
  [SerializeField] private TextMeshProUGUI highScoreText;
  
  private GameManager gameManager;

  private void Start() {
    gameManager = FindAnyObjectByType<GameManager>();
  }

  private void Update() {
    if (gameManager != null) {
      scoreText.text = "Score: " + gameManager.currentScore;
      highScoreText.text = "High Score: " + gameManager.highScore;
    }
  }
}