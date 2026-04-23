/*
    This script is responsible for controlling 
    the behavior of asteroids in the game as
    per the requirements.
*/
using UnityEngine;

public class Asteroid : MonoBehaviour {
    [SerializeField] private ParticleSystem destroyedParticles;
    [SerializeField] private AudioClip destroyedSound;
    [SerializeField] private AudioSource audioSource;
    public int size = 3;

    public GameManager gameManager;
    public ClassicGameManager classicGameManager;


    // Start is called before the first frame update
    private void Start() {
        // Scale asteroid based on size
        transform.localScale = 0.3f * size * Vector3.one;

        // Add random force for movement and rotation
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 direction = new Vector2(Random.value, Random.value).normalized;
        float spawnSpeed = Random.Range(4f - size, 5f - size);
        rb.AddForce(direction * spawnSpeed, ForceMode2D.Impulse);

        // Register with whichever manager is active
        if (gameManager != null) gameManager.asteroidCount++;
        else if (classicGameManager != null) classicGameManager.asteroidCount++;
    }

    // Handle collision with bullets
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Bullet")) {
            // Decrement correct manager
            if (gameManager != null) {
                gameManager.asteroidCount--;
                gameManager.AddScore(size);
            } else if (classicGameManager != null) {
                classicGameManager.asteroidCount--;
                classicGameManager.AddScore(size);
            }

            audioSource.clip = destroyedSound;
            audioSource.Play();
       
            // Disable visuals and collider immediately
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            // Destroy the bullet
            collision.gameObject.SetActive(false);
            Destroy(collision.gameObject, 0.1f);

            // Spawn smaller asteroids if size > 2, otherwise just play sound and particles
            if (size > 2) {
                for (int i = 0; i < 2; i++) {
                    Vector2 offset = Random.insideUnitCircle.normalized * 0.5f;
                    Asteroid newAsteroid = Instantiate(this, (Vector2)transform.position + offset, Quaternion.identity);
                    newAsteroid.size = size - 1;
                    newAsteroid.gameManager = gameManager;
                    newAsteroid.classicGameManager = classicGameManager;

                    newAsteroid.GetComponent<SpriteRenderer>().enabled = true;
                    newAsteroid.GetComponent<Collider2D>().enabled = true;
                    newAsteroid.GetComponent<AudioSource>().volume = 1f;
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(destroyedSound, transform.position, 50f);
            }

            Instantiate(destroyedParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}