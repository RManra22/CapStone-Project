using System.Collections;
using UnityEngine;

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
  [SerializeField] private int currentShootingStyle = 1;

  [Header("Object references")]
  [SerializeField] private Transform bulletSpawn;
  [SerializeField] private Rigidbody2D bulletPrefab;
  [SerializeField] private ParticleSystem destroyedParticles;

  private Rigidbody2D shipRigidbody;
  private bool isAlive = true;
  private bool isAccelerating = false;

  private void Start() {
    shipRigidbody = GetComponent<Rigidbody2D>();
    // Ensure we always start with a valid shooting style
    if (currentShootingStyle < 1 || currentShootingStyle > 3)
      currentShootingStyle = 1;
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
    isAccelerating = Input.GetKey(KeyCode.UpArrow);
  }

  private void HandleShipRotation() {
    if (Input.GetKey(KeyCode.LeftArrow)) {
      transform.Rotate(shipRotationSpeed * Time.deltaTime * transform.forward);
    } else if (Input.GetKey(KeyCode.RightArrow)) {
      transform.Rotate(-shipRotationSpeed * Time.deltaTime * transform.forward);
    }
  }

  private void HandleShootingStyleSwitch() {
    // Press 'Q' to cycle through shooting styles
    if (Input.GetKeyDown(KeyCode.Q)) {
      currentShootingStyle++;
      if (currentShootingStyle > 3) {  // Changed from 4 to 3
        currentShootingStyle = 1;
      }
      Debug.Log("Shooting Style: " + GetShootingStyleName());
    }
    
    // Or use number keys 1-3 for direct selection
    if (Input.GetKeyDown(KeyCode.Alpha1)) currentShootingStyle = 1;
    if (Input.GetKeyDown(KeyCode.Alpha2)) currentShootingStyle = 2;
    if (Input.GetKeyDown(KeyCode.Alpha3)) currentShootingStyle = 3;
  }


  // Helper method to get the name of the current shooting style
  private string GetShootingStyleName() {
    switch (currentShootingStyle) {
      case 1: return "Single Shot";
      case 2: return "Triple Spread";
      case 3: return "Burst Shot";
      default: return "Unknown";
    }
  }


  // Handles shooting based on the current shooting style and fire rate.
  private void HandleShooting() {
    if (!Input.GetKey(KeyCode.Space) || Time.time < nextFireTime)
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


  // Helper method to get the current fire rate based on shooting style
  private float GetCurrentFireRate() {
    switch (currentShootingStyle) {
      case 1: return singleShotFireRate;
      case 2: return spreadFireRate;
      case 3: return burstFireRate;
      default: return 0.5f;
    }
  }

  // Style 1: Single shot
  private void ShootSingle() {
    FireBullet(transform.up);
  }

  // Style 2: Triple spread shot
  private void ShootSpread() {
    // Center bullet
    FireBullet(transform.up);
    
    // Left bullet (15 degrees)
    Vector2 leftDirection = Rotate2D(transform.up, 15f);
    FireBullet(leftDirection);
    
    // Right bullet (-15 degrees)
    Vector2 rightDirection = Rotate2D(transform.up, -15f);
    FireBullet(rightDirection);
  }

  // Style 3: Burst shot (3 quick shots)
  private IEnumerator ShootBurst() {
    for (int i = 0; i < 3; i++) {
      FireBullet(transform.up);
      yield return new WaitForSeconds(0.05f);
    }
  }

  // Helper method to fire a single bullet in a given direction
  private void FireBullet(Vector2 direction) {
    Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

    // Inherit velocity only in the forward direction of ship
    Vector2 shipVelocity = shipRigidbody.linearVelocity;
    Vector2 shipDirection = transform.up;
    float shipForwardSpeed = Vector2.Dot(shipVelocity, shipDirection);

    if (shipForwardSpeed < 0) { 
      shipForwardSpeed = 0; 
    }

    bullet.linearVelocity = direction.normalized * shipForwardSpeed;
    bullet.AddForce(bulletSpeed * direction.normalized, ForceMode2D.Impulse);
  }

  // Helper method to rotate a 2D vector by an angle in degrees
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
    if (collision.CompareTag("Asteroid")) {
      isAlive = false;

      GameManager gameManager = FindAnyObjectByType<GameManager>();
      gameManager.GameOver();

      Instantiate(destroyedParticles, transform.position, Quaternion.identity);
      Destroy(gameObject);
    }
  }
}