using UnityEngine;

public class Powerup : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float driftSpeed = 1.5f;
    [SerializeField] private float lifetime = 10f;

    [Header("Type Sprites (optional)")]
    [SerializeField] private Sprite spreadSprite;
    [SerializeField] private Sprite burstSprite;
    [SerializeField] private Sprite fastShootSprite;
    [SerializeField] private Sprite homingSprite;

    private Vector2 driftDirection;
    private PowerupType powerupType;
    private Camera mainCamera;

    private void Start() {
        mainCamera = Camera.main;

        powerupType = (PowerupType)Random.Range(0, System.Enum.GetValues(typeof(PowerupType)).Length);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) {
            switch (powerupType) {
                case PowerupType.SpreadShot: if (spreadSprite)    sr.sprite = spreadSprite;    break;
                case PowerupType.BurstShot:  if (burstSprite)     sr.sprite = burstSprite;     break;
                case PowerupType.FastShoot:  if (fastShootSprite) sr.sprite = fastShootSprite; break;
                case PowerupType.HomingShot: if (homingSprite)    sr.sprite = homingSprite;    break;
            }
        }

        driftDirection = Random.insideUnitCircle.normalized;
        Destroy(gameObject, lifetime);
    }

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

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Player player = collision.GetComponent<Player>();
            if (player != null) {
                player.ApplyPowerup(powerupType);
                Destroy(gameObject);
            }
        }
    }
}