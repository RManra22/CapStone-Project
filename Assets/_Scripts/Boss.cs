using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Boss enemy with multiple attack patterns and phases.
public class Boss : MonoBehaviour {

  [Header("References")]
  [SerializeField] private Rigidbody2D bulletPrefab;       // Reuse the same bullet prefab as the player
  [SerializeField] private Transform bulletSpawn;           // Empty child object at the front of the boss
  [SerializeField] private ParticleSystem destroyedParticles;
  [SerializeField] private Asteroid asteroidPrefab;         // For the asteroid-spawn attack
  [SerializeField] private HealthPack healthPackPrefab;

  [Header("Health")]
  [SerializeField] private int maxHealth = 15;
  private int currentHealth;

  [Header("Movement")]
  [SerializeField] private float driftSpeed = 1.5f;         // Phase 1 drift speed
  [SerializeField] private float chaseSpeed = 3.5f;         // Phase 2 chase force
  [SerializeField] private float enragedSpeed = 5.5f;       // Phase 3 chase force
  [SerializeField] private float maxVelocity = 5f;
  [SerializeField] private float rotationSpeed = 90f;

  [Header("Attack - Bullet")]
  [SerializeField] private float bulletSpeed = 5f;
  [SerializeField] private float singleShotInterval = 2f;
  [SerializeField] private float spreadShotInterval = 3f;
  [SerializeField] private float spiralShotInterval = 0.1f;

  [Header("Attack - Asteroid Spawn")]
  [SerializeField] private float asteroidSpawnInterval = 6f;
  [SerializeField] private int asteroidsToSpawn = 2;

    [Header("Sound Effects")]

  [SerializeField] private AudioClip hitSound;
  [SerializeField] private AudioClip deathSound;
  [SerializeField] private AudioClip shootSound;
  [SerializeField] private AudioSource audioSource;
  

  private enum BossState { Drifting, Chasing, Enraged }
  private BossState state = BossState.Drifting;

  private Rigidbody2D rb;
  private Transform player;
  private GameManager gameManager;
  private bool isAlive = true;

  private float nextSingleShotTime;
  private float nextSpreadShotTime;
  private float nextAsteroidSpawnTime;
  private bool spiralRunning = false;
  private List<Asteroid> spawnedAsteroids = new List<Asteroid>(); // Track spawned asteroids to avoid overwhelming the player 



private int tier = 1; // Default to tier 1, will be set by GameManager on spawn
public void SetTier(int newTier) {
    tier = Mathf.Clamp(newTier, 1, 3); // Ensure tier is between 1 and 3
    switch (tier) {
        case 1:
            maxHealth = 5;
            // only aimed shot — no spread, no spiral
            asteroidSpawnInterval = Mathf.Infinity;
            break;

        case 2:
            maxHealth = 10;
            // aimed + spread + asteroid spawning
            spreadShotInterval = 3f;
            asteroidSpawnInterval = 6f;
            // still no spiral
            break;

        case 3:
            maxHealth = 10;
            // everything unlocked, faster and more aggressive
            spreadShotInterval = 2f;
            asteroidSpawnInterval = 7f;
            chaseSpeed = 5f;
            enragedSpeed = 8f;
            asteroidsToSpawn = 3;
            break;
    }
    }


  // Helper to rotate a vector by degrees
  private void Start() {
    rb = GetComponent<Rigidbody2D>();
    gameManager = FindAnyObjectByType<GameManager>();

    Player playerObj = FindAnyObjectByType<Player>();
    if (playerObj != null) player = playerObj.transform;

    currentHealth = maxHealth;

    Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    rb.AddForce(randomDir * driftSpeed, ForceMode2D.Impulse);

    // Stagger attack timers
    nextSingleShotTime = Time.time + 1f;
    nextSpreadShotTime = Time.time + 2.5f;
    nextAsteroidSpawnTime = Time.time + 4f;

    gameManager.asteroidCount++;
  }

  private void Update() {
    if (!isAlive) return;

    // Re-acquire player reference if it was destroyed (e.g. after respawn)
    if (player == null) {
        Player playerObj = FindAnyObjectByType<Player>();
        if (playerObj != null) player = playerObj.transform;
    }
    UpdateState();
    HandleAttacks();
  }

  private void FixedUpdate() {
    if (!isAlive || player == null) return;

    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);

    // Rotate toward player
    Vector2 dirToPlayer = (player.position - transform.position).normalized;
    float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
    float newAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
    rb.MoveRotation(newAngle);

    // Movement based on state
    if (state == BossState.Chasing) {
      rb.AddForce(dirToPlayer * chaseSpeed);
    } else if (state == BossState.Enraged) {
      rb.AddForce(dirToPlayer * enragedSpeed);
    }
  }

  private void UpdateState() {
    float healthPercent = (float)currentHealth / maxHealth;
    if (healthPercent <= 0.33f)       state = BossState.Enraged;
    else if (healthPercent <= 0.66f)  state = BossState.Chasing;
    else                              state = BossState.Drifting;
  }

  private void HandleAttacks() {
    // Single aimed shot — all tiers
    if (Time.time >= nextSingleShotTime) {
        ShootAtPlayer();
        nextSingleShotTime = Time.time + singleShotInterval;
    }

    // Spread shot — tier 2+ only
    if (tier >= 2 && state != BossState.Drifting && Time.time >= nextSpreadShotTime) {
        ShootSpread();
        nextSpreadShotTime = Time.time + spreadShotInterval;
    }

    // Spiral — tier 3 enraged only
    if (tier >= 3 && state == BossState.Enraged && !spiralRunning) {
        StartCoroutine(ShootSpiral());
    }

    // Asteroid spawn — tier 2+ only
    if (tier >= 2 && Time.time >= nextAsteroidSpawnTime) {
        SpawnAsteroids();
        nextAsteroidSpawnTime = Time.time + asteroidSpawnInterval;
    }
}

  private void ShootAtPlayer() {
    if (player == null) return;
    Vector2 dir = (player.position - transform.position).normalized;
    FireBullet(dir);
  }

  private void ShootSpread() {
    int bulletCount = 5;
    float angleStep = 20f;
    float startAngle = -((bulletCount - 1) / 2f) * angleStep;
    for (int i = 0; i < bulletCount; i++) {
      FireBullet(Rotate2D(transform.up, startAngle + i * angleStep));
    }
  }

  // Spiral shoot coroutine — fires bullets in a rotating pattern for a few seconds
  private IEnumerator ShootSpiral() {
    spiralRunning = true;
    int totalBullets = 16;
    float angleStep = 360f / totalBullets;
    for (int i = 0; i < totalBullets; i++) {
      if (!isAlive) break;
      FireBullet(Rotate2D(Vector2.up, i * angleStep));
      yield return new WaitForSeconds(spiralShotInterval);
    }
    yield return new WaitForSeconds(4f);
    spiralRunning = false;
  }

  // Spawn smaller asteroids around the boss's position that drift outward in random directions
 private void SpawnAsteroids() {
    for (int i = 0; i < asteroidsToSpawn; i++) {
        Vector2 offset = Random.insideUnitCircle.normalized * 2f;
        Asteroid a = Instantiate(asteroidPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        a.size = 1;
        a.gameManager = gameManager;
        spawnedAsteroids.Add(a);
        StartCoroutine(DespawnAsteroid(a, 15f));
    }
}
// Helper to despawn asteroids after a delay to prevent overwhelming the player
private IEnumerator DespawnAsteroid(Asteroid asteroid, float delay) {
    yield return new WaitForSeconds(delay);
    if (asteroid != null) {
        spawnedAsteroids.Remove(asteroid);
        gameManager.asteroidCount--;
        Destroy(asteroid.gameObject);
    }
}

  // Helper to fire a bullet in a direction
  private void FireBullet(Vector2 direction) {
    if (bulletSpawn == null) return;
    Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
    bullet.gameObject.layer = LayerMask.NameToLayer("BossBullet");
    bullet.gameObject.tag = "BossBullet";
    bullet.AddForce(direction.normalized * bulletSpeed, ForceMode2D.Impulse);
    audioSource.clip = shootSound;
    audioSource.Play();
}

  private void OnTriggerEnter2D(Collider2D collision) {
    if (!isAlive) return;
    if (collision.CompareTag("Bullet")) {
      Destroy(collision.gameObject);
      TakeDamage(1);
    }
  }

  private void TakeDamage(int amount) {
    currentHealth -= amount;
    audioSource.clip = hitSound;
    audioSource.Play();
    StartCoroutine(FlashOnHit());
    if (currentHealth <= 0) Die();
  }

  private IEnumerator FlashOnHit() {
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr == null) yield break;
    Color original = sr.color;
    sr.color = Color.red;
    yield return new WaitForSeconds(0.08f);
    if (sr != null) sr.color = original;
  }

  private void Die() {
    isAlive = false;
    audioSource.clip = deathSound;
    audioSource.Play();
    GetComponent<SpriteRenderer>().enabled = false;
    GetComponent<Collider2D>().enabled = false;
    

    // Destroy all spawned asteroids
    foreach (Asteroid a in spawnedAsteroids) {
        if (a != null) {
            gameManager.asteroidCount--;
            Destroy(a.gameObject);
        }
    }
    spawnedAsteroids.Clear();

    gameManager.asteroidCount--;
    gameManager.OnBossDefeated();

    int bossPoints = tier == 1 ? 500 : tier == 2 ? 1000 : 1500;
    gameManager.currentScore += bossPoints;

    if (healthPackPrefab != null)
        Instantiate(healthPackPrefab, transform.position, Quaternion.identity);

    Instantiate(destroyedParticles, transform.position, Quaternion.identity);
    Destroy(gameObject, deathSound.length);
}

  private Vector2 Rotate2D(Vector2 vector, float degrees) {
    float radians = degrees * Mathf.Deg2Rad;
    float cos = Mathf.Cos(radians);
    float sin = Mathf.Sin(radians);
    return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
  }
}
