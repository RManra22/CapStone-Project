using UnityEngine;

// This script is responsible for the behaviour of the asteroids in the game.
public class Asteroid : MonoBehaviour {
  // The particle system to spawn when the asteroid is destroyed.
  [SerializeField] private ParticleSystem destroyedParticles;
  // The size of the asteroid, larger asteroids are slower and spawn smaller asteroids when destroyed.
  public int size = 3;

  public GameManager gameManager;

  private void Start() {
    // Scale based on the size.
    transform.localScale = 0.5f * size * Vector3.one;

    // Add movement, bigger asteroids are slower.
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    Vector2 direction = new Vector2(Random.value, Random.value).normalized;
    float spawnSpeed = Random.Range(4f - size, 5f - size);
    rb.AddForce(direction * spawnSpeed, ForceMode2D.Impulse);

    // Register creation
    gameManager.asteroidCount++;
  }

  private void OnTriggerEnter2D(Collider2D collision) {
    // Asteroids are only destroyed with bullets.
    if (collision.CompareTag("Bullet")) {
      // Register the destruction with the game manager.
      gameManager.asteroidCount--;

      // Adds score when an asteroid is destroyed
      gameManager.AddScore(size);

      // Destroy the bullet so it doesn't carry on and hit more things.
      Destroy(collision.gameObject);

      // If size > 1 spawn 2 smaller asteroids of size-1.
      if (size > 1) {
        for (int i = 0; i < 2; i++) {
          Asteroid newAsteroid = Instantiate(this, transform.position, Quaternion.identity);
          newAsteroid.size = size - 1;
          newAsteroid.gameManager = gameManager;
        }
      }

      // Spawn particles on destruction.
      Instantiate(destroyedParticles, transform.position, Quaternion.identity);

      // Destroy this asteroid.
      Destroy(gameObject);
    }
  }
}

