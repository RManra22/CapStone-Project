using UnityEngine;

public class Powerup : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float driftSpeed = 1.5f;
    [SerializeField] private float lifetime = 10f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;


    private Vector2 driftDirection;
    [SerializeField] private PowerupType powerupType;
    private Camera mainCamera;

    // On start, randomly assign a powerup type and color, set a random drift direction, and destroy after lifetime expires
    private void Start() {
        mainCamera = Camera.main;

        powerupType = (PowerupType)Random.Range(0, System.Enum.GetValues(typeof(PowerupType)).Length);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) {
            switch (powerupType) {
                case PowerupType.SpreadShot:  sr.color = new Color(0.5f, 0.5f, 1f, 1f);   break; // Blue Color
                case PowerupType.BurstShot:   sr.color = new Color(1f, 0.5f, 0.5f, 1f);   break; // Red Color
                case PowerupType.FastShoot:   sr.color = new Color(0.5f, 1f, 0.5f, 1f);   break; // Green Color
                case PowerupType.HomingShot:  sr.color = new Color(1f, 1f, 0.5f, 1f);     break; // Yellow Color
                case PowerupType.SpeedBoost:  sr.color = new Color(0.5f, 1f, 1f, 1f);     break; // Cyan Color
                case PowerupType.ReverseShot: sr.color = new Color(1f, 0.5f, 1f, 1f);     break; // Magenta Color
                case PowerupType.Shield:      sr.color = new Color(1f, 1f, 1f, 1f);       break; // No color/White
            }
        }

        driftDirection = Random.insideUnitCircle.normalized;
        Destroy(gameObject, lifetime);
    }

    // Moves in a drifting pattern and wraps around screen edges
    private void Update() {
        if (mainCamera == null) return;

        transform.position += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);

        Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);
        bool wrapped = false;
        if (vp.x < 0) { vp.x = 1; wrapped = true; }
        if (vp.x > 1) { vp.x = 0; wrapped = true; }
        if (vp.y < 0) { vp.y = 1; wrapped = true; }
        if (vp.y > 1) { vp.y = 0; wrapped = true; }
        if (wrapped)
            transform.position = mainCamera.ViewportToWorldPoint(vp);
    }

    // When player collides with the powerup, apply the effect, play sound, and destroy the powerup
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Player player = collision.GetComponent<Player>();
            if (player != null) {
                player.ApplyPowerup(powerupType);
                audioSource.clip = pickupSound;
                audioSource.Play();
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                Destroy(gameObject, pickupSound.length);
            }
        }
    }
}   