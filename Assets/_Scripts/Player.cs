using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// This script is responsible for the behaviour of the player in the game.
public class Player : MonoBehaviour {
  [Header("Ship parameters")]
  [SerializeField] private float shipAcceleration = 6f;
  [SerializeField] private float shipMaxVelocity = 6f;
  [SerializeField] private float shipRotationSpeed = 180f;
  [SerializeField] private float bulletSpeed = 4f;

  [Header("Shooting parameters")]
  [SerializeField] private float singleShotFireRate = 0.5f;
  [SerializeField] private float spreadFireRate = 1f;
  [SerializeField] private float burstFireRate = 0.8f;
  private float nextFireTime = 0f;

  // Shooting styles: 1 = Single, 2 = Spread, 3 = Burst
  private int currentShootingStyle = 1;

  [Header("Object references")]
  [SerializeField] private Transform bulletSpawn;
  [SerializeField] private Rigidbody2D bulletPrefab;
  [SerializeField] private ParticleSystem destroyedParticles;

  [Header("Respawn invincibility")]
  [SerializeField] private float invincibilityDuration = 3f;
  [SerializeField] private float flashInterval = 0.15f;

  [Header("Powerups")]
  [SerializeField] private float fastShootDuration = 8f;
  [SerializeField] private float fastShootMultiplier = 0.35f;
  private float baseFireRateMultiplier = 1f;

  private PlayerInput playerInput;
  private Rigidbody2D shipRigidbody;
  private SpriteRenderer spriteRenderer;
  private bool isAlive = true;
  private bool isInvincible = false;
  private bool isAccelerating = false;

  private void Start() {
    shipRigidbody = GetComponent<Rigidbody2D>();
    playerInput = GetComponent<PlayerInput>();
    spriteRenderer = GetComponent<SpriteRenderer>();

    // Load the style the player selected from the shop/inventory
    currentShootingStyle = PlayerPrefs.GetInt("SelectedShootingStyle", 1);

    StartCoroutine(RespawnInvincibility());
  }

  private IEnumerator RespawnInvincibility() {
    isInvincible = true;
    float elapsed = 0f;

    while (elapsed < invincibilityDuration) {
      spriteRenderer.enabled = !spriteRenderer.enabled;
      yield return new WaitForSeconds(flashInterval);
      elapsed += flashInterval;
    }

    spriteRenderer.enabled = true;
    isInvincible = false;
  }

  private void Update() {
    if (isAlive) {
      HandleShipAcceleration();
      HandleShipRotation();
      HandleShooting();
    }
  }

  private void FixedUpdate() {
    if (isAlive && isAccelerating) {
      shipRigidbody.AddForce(shipAcceleration * transform.up);
      shipRigidbody.linearVelocity = Vector2.ClampMagnitude(shipRigidbody.linearVelocity, shipMaxVelocity);
    }
  }

  private void HandleShipAcceleration() {
    isAccelerating = playerInput.actions["Accel"].ReadValue<Vector2>().y > 0;
  }

  private void HandleShipRotation() {
    if (playerInput.actions["TurnLeft"].ReadValue<Vector2>().x < 0) {
      transform.Rotate(shipRotationSpeed * Time.deltaTime * transform.forward);
    } else if (playerInput.actions["TurnRight"].ReadValue<Vector2>().x > 0) {
      transform.Rotate(-shipRotationSpeed * Time.deltaTime * transform.forward);
    }
  }

  private void HandleShooting() {
    if (!playerInput.actions["Shoot"].triggered || Time.time < nextFireTime)
      return;

    switch (currentShootingStyle) {
      case 1:
        ShootSingle();
        nextFireTime = Time.time + singleShotFireRate * baseFireRateMultiplier;
        break;
      case 2:
        ShootSpread();
        nextFireTime = Time.time + spreadFireRate * baseFireRateMultiplier;
        break;
      case 3:
        StartCoroutine(ShootBurst());
        nextFireTime = Time.time + burstFireRate * baseFireRateMultiplier;
        break;
    }
  }

  public void ApplyFastShootPowerup() {
    StopCoroutine(nameof(FastShootTimer));
    StartCoroutine(nameof(FastShootTimer));
  }

  private IEnumerator FastShootTimer() {
    baseFireRateMultiplier = fastShootMultiplier;
    yield return new WaitForSeconds(fastShootDuration);
    baseFireRateMultiplier = 1f;
  }

  private void ShootSingle() {
    FireBullet(transform.up);
  }

  private void ShootSpread() {
    FireBullet(transform.up);
    FireBullet(Rotate2D(transform.up,  15f));
    FireBullet(Rotate2D(transform.up, -15f));
  }

  private IEnumerator ShootBurst() {
    for (int i = 0; i < 3; i++) {
      FireBullet(transform.up);
      yield return new WaitForSeconds(0.05f);
    }
  }

  private void FireBullet(Vector2 direction) {
    Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
    Vector2 shipVelocity = shipRigidbody.linearVelocity;
    float shipForwardSpeed = Mathf.Max(0f, Vector2.Dot(shipVelocity, (Vector2)transform.up));
    bullet.linearVelocity = direction.normalized * shipForwardSpeed;
    bullet.AddForce(bulletSpeed * direction.normalized, ForceMode2D.Impulse);
  }

  private Vector2 Rotate2D(Vector2 vector, float degrees) {
    float radians = degrees * Mathf.Deg2Rad;
    float cos = Mathf.Cos(radians);
    float sin = Mathf.Sin(radians);
    return new Vector2(
      vector.x * cos - vector.y * sin,
      vector.x * sin + vector.y * cos
    );
  }

  private void OnTriggerEnter2D(Collider2D collision) {
    if (!isAlive || isInvincible) return;
    if (collision.CompareTag("Asteroid") || collision.CompareTag("BossBullet") || collision.CompareTag("Boss")) {
      isAlive = false;
      GameManager.Instance.OnPlayerDied();
      Instantiate(destroyedParticles, transform.position, Quaternion.identity);
      Destroy(gameObject);
    }
  }
}