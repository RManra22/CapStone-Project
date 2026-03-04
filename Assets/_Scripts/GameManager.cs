using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
  public static GameManager Instance { get; private set; }
  [SerializeField] private Asteroid asteroidPrefab;
  [SerializeField] private Boss bossPrefab;
  
  public int asteroidCount = 0;
  private int level = 0;
  private int bossesDefeated = 0;
  
  // SCORE SYSTEM
  public int currentScore = 0;
  public int highScore = 0;
  
  // Pause state
  public bool isPaused;
  private bool isTransitioning = false;

  // UI stuff
  [SerializeField] private GameObject pauseMenuUI;
  [SerializeField] private GameObject inGameUI;
  [SerializeField] private TextMeshProUGUI levelCompleteText;
  [SerializeField] private TextMeshProUGUI levelText;

  
  // Points awarded based on asteroid size
  [SerializeField] private int largeAsteroidPoints = 20;
  [SerializeField] private int mediumAsteroidPoints = 50;
  [SerializeField] private int smallAsteroidPoints = 100;

  void Awake() {
    Instance = this;
    Time.timeScale = 1f;
  }

  private void Start() {
    highScore = PlayerPrefs.GetInt("HighScore", 0);
  }

  // Main game loop - checks if all asteroids are destroyed to spawn the next wave or boss
  private void Update() {
    if (asteroidCount == 0 && !isTransitioning) {
      level++;
      StartCoroutine(LevelTransition());
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

  // Handles the level transition, including UI updates and spawning the next wave or boss.
 private IEnumerator LevelTransition() {
    isTransitioning = true;

    levelText.text = "Level " + level;

    bool isBossLevel = level % 2 == 0;

    if (level >= 1) {
    levelCompleteText.gameObject.SetActive(true);

    for (int i = 3; i > 0; i--) {
        if (level == 1) {
            levelCompleteText.text = "Game starts in " + i + "...";
        } else {
            levelCompleteText.text = "Next level starts in " + i + "...";
        }
        yield return new WaitForSeconds(1f);
    }

    levelCompleteText.gameObject.SetActive(false);
}

    if (isBossLevel) {
        SpawnBoss();
    } else {
        int numAsteroids = level;
        for (int i = 0; i < numAsteroids; i++) {
            SpawnAsteroid();
        }
    }

    isTransitioning = false;
}


  // Spawns a boss at a random edge of the screen with a tier based on how many bosses have been defeated.
  private void SpawnBoss() {
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
    Boss boss = Instantiate(bossPrefab, worldSpawnPosition, Quaternion.identity);
    int tier = Mathf.Min(bossesDefeated + 1, 3);
    boss.SetTier(tier);
  }

  public void OnBossDefeated() {
    bossesDefeated++;
  }

  // Spawns an asteroid at a random edge of the screen.
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

  // Add points based on asteroid size
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
    
    if (currentScore > highScore) {
      highScore = currentScore;
      PlayerPrefs.SetInt("HighScore", highScore);
      PlayerPrefs.Save();
    }
  }

  public void GameOver() {
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

  public void PauseGame() {
    isPaused = true;
    Time.timeScale = 0f;
    inGameUI.SetActive(false);
    pauseMenuUI.SetActive(true);
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void ResumeGame() {
    isPaused = false;
    Time.timeScale = 1f;
    pauseMenuUI.SetActive(false);
    inGameUI.SetActive(true);
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }
}