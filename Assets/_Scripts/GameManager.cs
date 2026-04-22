using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

  // Singleton pattern for easy access from other scripts
  public static GameManager Instance { 
    get; 
    private set; 
    }
  [SerializeField] private Asteroid asteroidPrefab; 
  [SerializeField] private Boss bossPrefab;
  [SerializeField] private Powerup powerupPrefab;
  [SerializeField] private float powerupSpawnInterval = 10f;
  [SerializeField] private TextMeshProUGUI powerupText;


  public int asteroidCount = 0;
  private int level = 0;
  private int bossesDefeated = 0;

  // SCORE SYSTEM
  public int currentScore = 0;
  public int highScore = 0;

  // LIVES SYSTEM
  public int maxLives = 3;
  public int currentLives;
  [SerializeField] private Player playerPrefab;
  [SerializeField] private Transform playerSpawnPoint;

  // Pause state
  public bool isPaused;
  public bool isTransitioning = false;

  // UI stuff
  [SerializeField] private GameObject pauseMenuUI;
  [SerializeField] private GameObject inGameUI;
  [SerializeField] private TextMeshProUGUI levelCompleteText;
  [SerializeField] private TextMeshProUGUI levelText;

  // Lives UI — (do later)
  [SerializeField] private GameObject[] lifeIcons;

  // Points awarded based on asteroid size
  [SerializeField] private int largeAsteroidPoints = 20;
  [SerializeField] private int mediumAsteroidPoints = 50;
  [SerializeField] private int smallAsteroidPoints = 100;

  void Awake() {
    Instance = this;
    Time.timeScale = 1f;
    currentLives = maxLives;
  }

  private void Start() {
    highScore = PlayerPrefs.GetInt("HighScore", 0);
    UpdateLivesUI();
    StartCoroutine(PowerupSpawnLoop());

    // Assign powerup text to the initial player
    Player initialPlayer = FindAnyObjectByType<Player>();
    if (initialPlayer != null)
        initialPlayer.SetPowerupText(powerupText);
  }

  // Main game loop
  private void Update() {
    if (asteroidCount == 0 && !isTransitioning) {
      level++;
      StartCoroutine(LevelTransition());
    }

    if (Input.GetKeyDown(KeyCode.Escape)) {
      if (isPaused) ResumeGame();
      else PauseGame();
    }
  }

  // Refresh life icons — active icons = remaining lives, inactive = lost lives
  public void UpdateLivesUI() {
    if (lifeIcons == null) return;
    for (int i = 0; i < lifeIcons.Length; i++) {
        if (lifeIcons[i] != null) {
            Image icon = lifeIcons[i].GetComponent<Image>();
            if (icon != null)
                icon.color = i < currentLives ? Color.white : Color.gray;
        }
    }
}
  // Called by Player when it is destroyed. Deducts a life and either respawns or ends the game.
  public void OnPlayerDied() {
    currentLives--;
    UpdateLivesUI();

    if (currentLives <= 0) {
      GameOver();
    } else {
      RespawnPlayer();
    }
  }

  // Spawns a fresh player immediately — the flashing on the Player itself signals invincibility.
  private void RespawnPlayer() {
    Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
    Player newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    newPlayer.SetPowerupText(powerupText);
  }

  // Level transition
  private IEnumerator LevelTransition() {
    isTransitioning = true;

    // Hide powerup text if it's showing
    Player currentPlayer = FindAnyObjectByType<Player>();
    if (currentPlayer != null)
        currentPlayer.HidePowerupText();  

    levelText.text = "Level " + level;

    bool isBossLevel = level % 2 == 0;

    if (level >= 1) {
      levelCompleteText.gameObject.SetActive(true);

      for (int i = 3; i > 0; i--) {
        levelCompleteText.text = level == 1
          ? "Game starts in " + i + "..."
          : "Next level starts in " + i + "...";
        yield return new WaitForSeconds(1f);
      }

      levelCompleteText.gameObject.SetActive(false);
    }
     // Hide again after countdown in case a powerup was picked up during it
    currentPlayer = FindAnyObjectByType<Player>();
    if (currentPlayer != null)
        currentPlayer.HidePowerupText();

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

  private void SpawnBoss() {
    Vector2 worldSpawnPosition = GetRandomEdgePosition();
    Boss boss = Instantiate(bossPrefab, worldSpawnPosition, Quaternion.identity);
    int tier = Mathf.Min(bossesDefeated + 1, 3);
    boss.SetTier(tier);
  }

  public void OnBossDefeated() {
    bossesDefeated++;
  }

  private void SpawnAsteroid() {
    Vector2 worldSpawnPosition = GetRandomEdgePosition();
    Asteroid asteroid = Instantiate(asteroidPrefab, worldSpawnPosition, Quaternion.identity);
    asteroid.gameManager = this;
  }

  // Spawns a powerup at a random position on screen at a fixed interval
  private IEnumerator PowerupSpawnLoop() {
    while (true) {
      yield return new WaitForSeconds(powerupSpawnInterval);
      SpawnPowerup();
    }
  }

  private void SpawnPowerup() {
    if (powerupPrefab == null) return;

    // Spawn at a random position within the visible play area
    Vector2 viewportPos = new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f));
    Vector2 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
    Instantiate(powerupPrefab, worldPos, Quaternion.identity);
  }

  private Vector2 GetRandomEdgePosition() {
    float offset = Random.Range(0f, 1f);
    Vector2 viewportPos;
    int edge = Random.Range(0, 4);
    if (edge == 0)      viewportPos = new Vector2(offset, 0);
    else if (edge == 1) viewportPos = new Vector2(offset, 1);
    else if (edge == 2) viewportPos = new Vector2(0, offset);
    else                viewportPos = new Vector2(1, offset);
    return Camera.main.ViewportToWorldPoint(viewportPos);
  }

// Called by Asteroid when destroyed to add points and check for high score updates
  public void AddScore(int asteroidSize) {
    int points = 0;
    if (asteroidSize == 3)      points = largeAsteroidPoints;
    else if (asteroidSize == 2) points = mediumAsteroidPoints;
    else if (asteroidSize == 1) points = smallAsteroidPoints;

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

    // Calculate and add credits
    int creditsEarned = currentScore / 100;
    int totalCredits = PlayerPrefs.GetInt("TotalCredits", 0) + creditsEarned;
    PlayerPrefs.SetInt("TotalCredits", totalCredits);
    PlayerPrefs.SetInt("CreditsEarned", creditsEarned);

    PlayerPrefs.SetInt("LastScore", currentScore);
    PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
    PlayerPrefs.SetString("LastHighScoreKey", "HighScore");
    PlayerPrefs.Save();
    StartCoroutine(Restart());
}

  private IEnumerator Restart() {
    Debug.Log("Game Over - Final Score: " + currentScore);
    yield return new WaitForSeconds(2f);
    SceneManager.LoadScene("GameOver");
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