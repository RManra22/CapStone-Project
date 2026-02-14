using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
  public static GameManager Instance { get; private set; }
  [SerializeField] private Asteroid asteroidPrefab;
  
  public int asteroidCount = 0;
  private int level = 0;
  
  // SCORE SYSTEM
  public int currentScore = 0;
  public int highScore = 0;
  
  // Pause state
  public bool isPaused;

  // UI stuff
  [SerializeField] private GameObject pauseMenuUI;
  [SerializeField] private GameObject inGameUI;
  
  // Points awarded based on asteroid size
  [SerializeField] private int largeAsteroidPoints = 20;
  [SerializeField] private int mediumAsteroidPoints = 50;
  [SerializeField] private int smallAsteroidPoints = 100;

  void Awake() {
    Instance = this;
    Time.timeScale = 1f; // Makes the game starts unpaused
  }
  private void Start() {
    // Load the high score from PlayerPrefs
    highScore = PlayerPrefs.GetInt("HighScore", 0);
  }

  private void Update() {
    if (asteroidCount == 0) {
      level++;
      int numAsteroids = 2 + (2 * level);
      for (int i = 0; i < numAsteroids; i++) {
        SpawnAsteroid();
      }
    }

    // Toggle pause on Escape key press
    if (Input.GetKeyDown(KeyCode.Escape)) {
      if (isPaused) {
        ResumeGame();
      } else {
        PauseGame(); 
      }
    }
  }

  private void SpawnAsteroid() {
    float offset = Random.Range(0f, 1f);
    Vector2 viewportSpawnPosition = Vector2.zero;

    int edge = Random.Range(0, 4);
    if (edge == 0) {
      viewportSpawnPosition = new Vector2(offset, 0);
    } else if (edge == 1) {
      viewportSpawnPosition = new Vector2(offset, 1);
    } else if (edge == 2) {
      viewportSpawnPosition = new Vector2(0, offset);
    } else if (edge == 3) {
      viewportSpawnPosition = new Vector2(1, offset);
    }

    Vector2 worldSpawnPosition = Camera.main.ViewportToWorldPoint(viewportSpawnPosition);
    Asteroid asteroid = Instantiate(asteroidPrefab, worldSpawnPosition, Quaternion.identity);
    asteroid.gameManager = this;
  }

  // NEW METHOD: Add points based on asteroid size
  public void AddScore(int asteroidSize) {
    int points = 0;
    
    if (asteroidSize == 3) {
      points = largeAsteroidPoints;
    } else if (asteroidSize == 2) {
      points = mediumAsteroidPoints;
    } else if (asteroidSize == 1) {
      points = smallAsteroidPoints;
    }
    
    currentScore += points;
    
    // Update high score if current score exceeds it
    if (currentScore > highScore) {
      highScore = currentScore;
      PlayerPrefs.SetInt("HighScore", highScore);
      PlayerPrefs.Save(); // Save immediately
    }
  }

  public void GameOver() {
    // Save high score one more time on game over
    if (currentScore > PlayerPrefs.GetInt("HighScore", 0)) {
      PlayerPrefs.SetInt("HighScore", currentScore);
      PlayerPrefs.Save();
    }
    
    StartCoroutine(Restart());
  }

  private IEnumerator Restart() {
    Debug.Log("Game Over - Final Score: " + currentScore);
    yield return new WaitForSeconds(2f);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    yield return null;
  }

  public void PauseGame()
  {
    isPaused = true;
    Time.timeScale = 0f;
    inGameUI.SetActive(false);
    pauseMenuUI.SetActive(true);
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    
  }

  public void ResumeGame()
  {
    isPaused = false;
    Time.timeScale = 1f;
    pauseMenuUI.SetActive(false);
    inGameUI.SetActive(true);
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    
  }
}