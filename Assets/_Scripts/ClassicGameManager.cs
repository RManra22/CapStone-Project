using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ClassicGameManager : MonoBehaviour {

    public static ClassicGameManager Instance { get; private set; }

    [SerializeField] private Asteroid asteroidPrefab;
    [SerializeField] private int asteroidsOnScreen = 5;

    public int asteroidCount = 0;

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

    // UI
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject[] lifeIcons;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

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
        highScore = PlayerPrefs.GetInt("ClassicHighScore", 0);
        UpdateLivesUI();
        UpdateScoreUI();

        // Spawn initial asteroids
        for (int i = 0; i < asteroidsOnScreen; i++) {
            SpawnAsteroid();
        }
    }

    private void Update() {
    if (asteroidCount < asteroidsOnScreen) {
        SpawnAsteroid();
    }

    if (Input.GetKeyDown(KeyCode.Escape)) {
        if (isPaused) ResumeGame();
        else PauseGame();
    }
}

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

    private void UpdateScoreUI() {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (highScoreText != null) highScoreText.text = "Best: " + highScore;
    }

    public void OnPlayerDied() {
        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0) {
            GameOver();
        } else {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer() {
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    private void SpawnAsteroid() {
        Vector2 spawnPos = GetRandomEdgePosition();
        Asteroid asteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        asteroid.gameManager = null;
        asteroid.classicGameManager = this;
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

    public void AddScore(int asteroidSize) {
        int points = 0;
        if (asteroidSize == 3)      points = largeAsteroidPoints;
        else if (asteroidSize == 2) points = mediumAsteroidPoints;
        else if (asteroidSize == 1) points = smallAsteroidPoints;

        currentScore += points;

        if (currentScore > highScore) {
            highScore = currentScore;
            PlayerPrefs.SetInt("ClassicHighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
    }

    public void GameOver() {
        if (currentScore > PlayerPrefs.GetInt("ClassicHighScore", 0)) {
            PlayerPrefs.SetInt("ClassicHighScore", currentScore);
            PlayerPrefs.Save();
        }
        PlayerPrefs.SetInt("LastScore", currentScore);
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetString("LastHighScoreKey", "ClassicHighScore");
        PlayerPrefs.Save();
        StartCoroutine(Restart());
    }

    private IEnumerator Restart() {
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