using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// This script is responsible for managing the overall game state, including spawning asteroids and handling game over conditions.
public class GameManager : MonoBehaviour {
  [SerializeField] private Asteroid asteroidPrefab;
  
  // The number of asteroids currently in the game. When this reaches 0, we spawn more.
  public int asteroidCount = 0;

  // The current level, which determines how many asteroids to spawn. Increases each time all asteroids are destroyed.
  private int level = 0;

  private void Update() {
    // If there are no asteroids left, spawn more!
    if (asteroidCount == 0) {
      // Increase the level.
      level++;

      // Spawn the correct number for this level.
      // 1=>4, 2=>6, 3=>8, 4=>10 ...
      int numAsteroids = 2 + (2 * level);
      for (int i = 0; i < numAsteroids; i++) {
        SpawnAsteroid();
      }
    }
  }

  private void SpawnAsteroid() {
    // How far along the edge.
    float offset = Random.Range(0f, 1f);
    Vector2 viewportSpawnPosition = Vector2.zero;

    // Which edge.
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

    // Create the asteroid.
    Vector2 worldSpawnPosition = Camera.main.ViewportToWorldPoint(viewportSpawnPosition);
    Asteroid asteroid = Instantiate(asteroidPrefab, worldSpawnPosition, Quaternion.identity);
    asteroid.gameManager = this;
  }

  public void GameOver() {
    StartCoroutine(Restart());
  }

  private IEnumerator Restart() {
    Debug.Log("Game Over");

    // Wait a bit before restarting.
    yield return new WaitForSeconds(2f);

    // Restart scene.
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    yield return null;
  }
}
