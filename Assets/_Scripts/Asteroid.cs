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
    if (collision.CompareTag("Bullet")) {
        gameManager.asteroidCount--;
        gameManager.AddScore(size);

        // Disable immediately instead of Destroy so it can't hit children
        collision.gameObject.SetActive(false);
        Destroy(collision.gameObject, 0.1f); // clean up shortly after


        // If the asteroid is large enough, spawn two smaller asteroids and have an offset for its spawn.
        if (size > 2) {
            for (int i = 0; i < 2; i++) {
                Vector2 offset = Random.insideUnitCircle.normalized * 0.5f;
                Asteroid newAsteroid = Instantiate(this, (Vector2)transform.position + offset, Quaternion.identity);
                newAsteroid.size = size - 1;
                newAsteroid.gameManager = gameManager;
            }
        }

        Instantiate(destroyedParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
}

