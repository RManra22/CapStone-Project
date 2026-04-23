/*
    This script is responsible for controlling 
    the player's ship, including movement, shooting,
    powerup effects, and handling death. It also manages
    the player's invincibility after respawning and displays
    powerup text when a new powerup is picked up.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour {
    [Header("Ship parameters")]
    [SerializeField] private float shipAcceleration = 2f;
    [SerializeField] private float shipMaxVelocity = 2f;
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
    [SerializeField] private float homingDuration = 4f;
    [SerializeField] private Rigidbody2D homingBulletPrefab;
    [SerializeField] private float speedBoostDuration = 8f;
    [SerializeField] private float boostedAcceleration = 4f;
    [SerializeField] private float boostedMaxVelocity = 5f;
    [SerializeField] private float reverseShotDuration = 8f;
    [SerializeField] private float shieldDuration = 5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;

    private bool isShielded = false;
    private Coroutine shieldCoroutine;
    private bool isReverseShot = false;
    private Coroutine reverseShotCoroutine;
    private float baseFireRateMultiplier = 1f;
    private Coroutine fastShootCoroutine;
    private bool isHoming = false;
    private Coroutine homingCoroutine;
    private Coroutine speedBoostCoroutine;
    private Coroutine showPowerupTextCoroutine;

    private PlayerInput playerInput;
    private Rigidbody2D shipRigidbody;
    private SpriteRenderer spriteRenderer;
    private bool isAlive = true;
    private bool isInvincible = false;
    private bool isAccelerating = false;

    private TextMeshProUGUI powerupText;
    public void SetPowerupText(TextMeshProUGUI text) {
        powerupText = text;
    }

    // On start, get necessary components, set initial shooting style, and start respawn invincibility
    private void Start() {
        shipRigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentShootingStyle = 1;
        StartCoroutine(RespawnInvincibility());
    }

    // Coroutine that makes the player invincible for a short duration after respawning, with flashing effect
    private IEnumerator RespawnInvincibility() {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration) {
            if (!isShielded)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        spriteRenderer.enabled = true;
        if (!isShielded)
            isInvincible = false;
    }

    // In Update, handle player input for movement and shooting, but only if alive
    private void Update() {
        if (isAlive) {
            HandleShipAcceleration();
            HandleShipRotation();
            HandleShooting();
        }
    }

    // In FixedUpdate, apply acceleration force if the player is accelerating, and clamp velocity to max speed
    private void FixedUpdate() {
        if (isAlive && isAccelerating) {
            shipRigidbody.AddForce(shipAcceleration * transform.up);
            shipRigidbody.linearVelocity = Vector2.ClampMagnitude(shipRigidbody.linearVelocity, shipMaxVelocity);
        }
    }

    // Handle input for accelerating forward, and set isAccelerating flag accordingly
    private void HandleShipAcceleration() {
        isAccelerating = playerInput.actions["Accel"].ReadValue<Vector2>().y > 0;
    }

    // Handle input for turning left and right, and rotate the ship accordingly
    private void HandleShipRotation() {
        if (playerInput.actions["TurnLeft"].ReadValue<Vector2>().x < 0)
            transform.Rotate(shipRotationSpeed * Time.deltaTime * transform.forward);
        else if (playerInput.actions["TurnRight"].ReadValue<Vector2>().x > 0)
            transform.Rotate(-shipRotationSpeed * Time.deltaTime * transform.forward);
    }

    // Handle input for shooting, check fire rate cooldown, and fire bullets based on current shooting style
    private void HandleShooting() {
        if (!playerInput.actions["Shoot"].triggered || Time.time < nextFireTime)
            return;

        switch (currentShootingStyle) {
            case 1:
                ShootSingle();
                nextFireTime = Time.time + singleShotFireRate * baseFireRateMultiplier;
                audioSource.clip = shootSound;
                audioSource.Play();
                break;
            case 2:
                ShootSpread();
                nextFireTime = Time.time + spreadFireRate * baseFireRateMultiplier;
                audioSource.clip = shootSound;
                audioSource.Play();
                break;
            case 3:
                StartCoroutine(ShootBurst());
                nextFireTime = Time.time + burstFireRate * baseFireRateMultiplier;
                audioSource.clip = shootSound;
                audioSource.Play();
                break;
        }
    }

    //  Apply a powerup effect based on the type, which starts the corresponding coroutine and shows powerup text
    public void ApplyPowerup(PowerupType type) {
        // Stop any existing powerup text coroutine
        if (showPowerupTextCoroutine != null) {
            StopCoroutine(showPowerupTextCoroutine);
            showPowerupTextCoroutine = null;
        }

        switch (type) {
            case PowerupType.SpreadShot:
                CancelFastShoot();
                currentShootingStyle = 2;
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("TRIPLE SHOT"));
                break;
            case PowerupType.BurstShot:
                CancelFastShoot();
                currentShootingStyle = 3;
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("BURST SHOT"));
                break;
            case PowerupType.FastShoot:
                if (fastShootCoroutine != null)
                    StopCoroutine(fastShootCoroutine);
                fastShootCoroutine = StartCoroutine(FastShootTimer());
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("FAST SHOOT"));
                break;
            case PowerupType.HomingShot:
                if (homingCoroutine != null)
                    StopCoroutine(homingCoroutine);
                homingCoroutine = StartCoroutine(HomingTimer());
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("HOMING SHOT"));
                break;
            case PowerupType.SpeedBoost:
                if (speedBoostCoroutine != null)
                    StopCoroutine(speedBoostCoroutine);
                speedBoostCoroutine = StartCoroutine(SpeedBoostTimer());
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("SPEED BOOST"));
                break;
            case PowerupType.ReverseShot:
                if (reverseShotCoroutine != null)
                    StopCoroutine(reverseShotCoroutine);
                reverseShotCoroutine = StartCoroutine(ReverseShotTimer());
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("REVERSE SHOT"));
                break;
            case PowerupType.Shield:
                if (shieldCoroutine != null)
                    StopCoroutine(shieldCoroutine);
                shieldCoroutine = StartCoroutine(ShieldTimer());
                showPowerupTextCoroutine = StartCoroutine(ShowPowerupText("SHIELD"));
                break;
        }
    }

    // Cancel the fast shoot effect
    private void CancelFastShoot() {
        if (fastShootCoroutine != null) {
            StopCoroutine(fastShootCoroutine);
            fastShootCoroutine = null;
        }
        baseFireRateMultiplier = 1f;
    }

    // Coroutine for the fast shoot powerup effect, which temporarily reduces the fire rate multiplier
    private IEnumerator FastShootTimer() {
        baseFireRateMultiplier = fastShootMultiplier;
        yield return new WaitForSeconds(fastShootDuration);
        baseFireRateMultiplier = 1f;
        fastShootCoroutine = null;
    }

    // Coroutine for the homing shot powerup effect, which sets isHoming to true for a duration
    private IEnumerator HomingTimer() {
        isHoming = true;
        yield return new WaitForSeconds(homingDuration);
        isHoming = false;
        homingCoroutine = null;
    }

    // Coroutine for the speed boost powerup effect, which temporarily increases acceleration and max velocity
    private IEnumerator SpeedBoostTimer() {
        float originalAcceleration = shipAcceleration;
        float originalMaxVelocity = shipMaxVelocity;
        shipAcceleration = boostedAcceleration;
        shipMaxVelocity = boostedMaxVelocity;
        yield return new WaitForSeconds(speedBoostDuration);
        shipAcceleration = originalAcceleration;
        shipMaxVelocity = originalMaxVelocity;
        speedBoostCoroutine = null;
    }

    // Coroutine for the reverse shot powerup effect, which sets isReverseShot to true for a duration
    private IEnumerator ReverseShotTimer() {
        isReverseShot = true;
        yield return new WaitForSeconds(reverseShotDuration);
        isReverseShot = false;
        reverseShotCoroutine = null;
    }

    // Coroutine for the shield powerup effect, which makes the player invincible and changes color for a duration
    private IEnumerator ShieldTimer() {
        isShielded = true;
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < shieldDuration) {
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            elapsed += 0.3f;
        }
        spriteRenderer.color = Color.white;
        isShielded = false;
        isInvincible = false;
        shieldCoroutine = null;
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

    // Helper method to fire a bullet in a given direction, using homing bullets if the homing powerup is active, and also firing reverse bullets if that powerup is active
    private void FireBullet(Vector2 direction) {
        if (isHoming && homingBulletPrefab != null) {
            Rigidbody2D bullet = Instantiate(homingBulletPrefab, bulletSpawn.position, Quaternion.identity);
            bullet.linearVelocity = direction.normalized * 8f;
            if (isReverseShot) {
                Rigidbody2D reverseBullet = Instantiate(homingBulletPrefab, bulletSpawn.position, Quaternion.identity);
                reverseBullet.linearVelocity = -direction.normalized * 8f;
            }
        } else {
            Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            Vector2 shipVelocity = shipRigidbody.linearVelocity;
            float shipForwardSpeed = Mathf.Max(0f, Vector2.Dot(shipVelocity, (Vector2)transform.up));
            bullet.linearVelocity = direction.normalized * shipForwardSpeed;
            bullet.AddForce(bulletSpeed * direction.normalized, ForceMode2D.Impulse);
            if (isReverseShot) {
                Rigidbody2D reverseBullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
                reverseBullet.linearVelocity = -direction.normalized * shipForwardSpeed;
                reverseBullet.AddForce(bulletSpeed * -direction.normalized, ForceMode2D.Impulse);
            }
        }
    }

    // Helper method to rotate a 2D vector by a certain number of degrees
    private IEnumerator ShowPowerupText(string message) {
        if (powerupText == null) yield break;
        if (GameManager.Instance != null && GameManager.Instance.isTransitioning) yield break;
        
        powerupText.text = message;
        powerupText.gameObject.SetActive(true);
        powerupText.color = Color.white;

        yield return new WaitForSeconds(1.5f);

        float fadeDuration = 1f;
        float elapsed = 0f;
        Color startColor = powerupText.color;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            powerupText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        powerupText.gameObject.SetActive(false);
        showPowerupTextCoroutine = null;
    }

    // Hides the powerup text immediately, used during level transitions or when a new powerup is picked up
    public void HidePowerupText() {
        if (showPowerupTextCoroutine != null) {
            StopCoroutine(showPowerupTextCoroutine);
            showPowerupTextCoroutine = null;
        }
        if (powerupText != null)
            powerupText.gameObject.SetActive(false);
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

    // When player collides with an asteroid, boss bullet, or boss, handle death if not invincible or shielded
    private void OnTriggerEnter2D(Collider2D collision) {
        if (!isAlive || isInvincible) return;
        if (collision.CompareTag("Asteroid") || collision.CompareTag("BossBullet") || collision.CompareTag("Boss")) {
            isAlive = false;
            audioSource.clip = deathSound;
            audioSource.Play();

            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDied();
            else if (ClassicGameManager.Instance != null)
                ClassicGameManager.Instance.OnPlayerDied();

            Instantiate(destroyedParticles, transform.position, Quaternion.identity);
            Destroy(gameObject, deathSound.length);
        }
    }
}