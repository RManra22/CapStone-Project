using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// This script is responsible for the behaviour of the player in the game.
public class Player : MonoBehaviour {
  [Header("Ship parameters")]
  [SerializeField] private float shipAcceleration = 3f;
  [SerializeField] private float shipMaxVelocity = 3f;
  [SerializeField] private float shipRotationSpeed = 180f;
  [SerializeField] private float bulletSpeed = 4f;

  [Header("Shooting parameters")]
  [SerializeField] private float singleShotFireRate = 0.5f;
  [SerializeField] private float spreadFireRate = 1f;
  [SerializeField] private float burstFireRate = 0.8f;
  private float nextFireTime = 0f;

  // Shooting styles: 1 = Single, 2 = Spread, 3 = Burst
  [SerializeField] private int currentShootingStyle = 1;

  [Header("Object references")]
  [SerializeField] private Transform bulletSpawn;
  [SerializeField] private Rigidbody2D bulletPrefab;
  [SerializeField] private ParticleSystem destroyedParticles;

  [Header("Respawn invincibility")]
  [SerializeField] private float invincibilityDuration = 2f;
  [SerializeField] private float flashInterval = 0.15f;

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

    if (currentShootingStyle < 1 || currentShootingStyle > 3)
      currentShootingStyle = 1;

    // Always start with invincibility 
    StartCoroutine(RespawnInvincibility());
  }

  // Flashes the sprite and blocks damage for invincibilityDuration seconds.
  private IEnumerator RespawnInvincibility() {
    isInvincible = true;
    float elapsed = 0f;

    while (elapsed < invincibilityDuration) {
      spriteRenderer.enabled = !spriteRenderer.enabled;
      yield return new WaitForSeconds(flashInterval);
      elapsed += flashInterval;
    }

    // Ensure we end fully visible
    spriteRenderer.enabled = true;
    isInvincible = false;
  }

  private void Update() {
    if (isAlive) {
      HandleShipAcceleration();
      HandleShipRotation();
      HandleShooting();
      HandleShootingStyleSwitch();
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

  private void HandleShootingStyleSwitch() {
    if (Input.GetKeyDown(KeyCode.Q)) {
      currentShootingStyle++;
      if (currentShootingStyle > 3) currentShootingStyle = 1;
      Debug.Log("Shooting Style: " + GetShootingStyleName());
    }
    if (Input.GetKeyDown(KeyCode.Alpha1)) currentShootingStyle = 1;
    if (Input.GetKeyDown(KeyCode.Alpha2)) currentShootingStyle = 2;
    if (Input.GetKeyDown(KeyCode.Alpha3)) currentShootingStyle = 3;
  }

  private string GetShootingStyleName() {
    switch (currentShootingStyle) {
      case 1: return "Single Shot";
      case 2: return "Triple Spread";
      case 3: return "Burst Shot";
      default: return "Unknown";
    }
  }

  private void HandleShooting() {
    if (!playerInput.actions["Shoot"].triggered || Time.time < nextFireTime)
      return;

    switch (currentShootingStyle) {
      case 1:
        ShootSingle();
        nextFireTime = Time.time + singleShotFireRate;
        break;
      case 2:
        ShootSpread();
        nextFireTime = Time.time + spreadFireRate;
        break;
      case 3:
        StartCoroutine(ShootBurst());
        nextFireTime = Time.time + burstFireRate;
        break;
    }
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
    if (collision.CompareTag("Asteroid") || collision.CompareTag("BossBullet")) {
      isAlive = false;
      GameManager.Instance.OnPlayerDied();
      Instantiate(destroyedParticles, transform.position, Quaternion.identity);
      Destroy(gameObject);
    }
  }
}