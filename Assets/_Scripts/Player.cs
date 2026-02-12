using System.Collections;
using UnityEngine;

// This script is responsible for the behaviour of the player in the game.
public class Player : MonoBehaviour {
  [Header("Ship parameters")]
  // The acceleration applied to the ship when the up arrow is held down.
  [SerializeField] private float shipAcceleration = 10f;


  // The maximum velocity the ship can reach.
  [SerializeField] private float shipMaxVelocity = 10f;

  // The rotation speed of the ship when left or right arrows are held down.
  [SerializeField] private float shipRotationSpeed = 180f;

  // The speed at which bullets are fired when space is pressed.
  [SerializeField] private float bulletSpeed = 8f;

  [Header("Object references")]
  [SerializeField] private Transform bulletSpawn;
  [SerializeField] private Rigidbody2D bulletPrefab;
  [SerializeField] private ParticleSystem destroyedParticles;

  private Rigidbody2D shipRigidbody;
  private bool isAlive = true;
  private bool isAccelerating = false;

  private void Start() {
    // Get a reference to the attached RigidBody2D.
    shipRigidbody = GetComponent<Rigidbody2D>();
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
      // Increase velocity upto a maximum.
      shipRigidbody.AddForce(shipAcceleration * transform.up);
      shipRigidbody.velocity = Vector2.ClampMagnitude(shipRigidbody.velocity, shipMaxVelocity);
    }
  }

  private void HandleShipAcceleration() {
    // Are we accelerating?
    isAccelerating = Input.GetKey(KeyCode.UpArrow);
  }

  private void HandleShipRotation() {
    // Ship rotation.
    if (Input.GetKey(KeyCode.LeftArrow)) {
      transform.Rotate(shipRotationSpeed * Time.deltaTime * transform.forward);
    } else if (Input.GetKey(KeyCode.RightArrow)) {
      transform.Rotate(-shipRotationSpeed * Time.deltaTime * transform.forward);
    }
  }

  private void HandleShooting() {
    // Shooting.
    if (Input.GetKeyDown(KeyCode.Space)) {

      Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

      // Inherit velicity only in the forward direction of ship.
      Vector2 shipVelocity = shipRigidbody.velocity;
      Vector2 shipDirection = transform.up;
      float shipForwardSpeed = Vector2.Dot(shipVelocity, shipDirection);

      // Don't want to inherit in the opposite direction, else we'll get stationary bullets.
      if (shipForwardSpeed < 0) { 
        shipForwardSpeed = 0; 
      }

      bullet.velocity = shipDirection * shipForwardSpeed;

      // Add force to propel bullet in direction the player is facing.
      bullet.AddForce(bulletSpeed * transform.up, ForceMode2D.Impulse);
    }
  }

  private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.CompareTag("Asteroid")) {
      isAlive = false;

      // Get a reference to the GameManager
      GameManager gameManager = FindAnyObjectByType<GameManager>();

      // Restart game after delay.
      gameManager.GameOver();

      // Show the destroyed effect.
      Instantiate(destroyedParticles, transform.position, Quaternion.identity);

      // Destroy the player.
      Destroy(gameObject);
    }
  }
}