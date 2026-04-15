using UnityEngine;

public class HealthPack : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float driftSpeed = 1.5f;
    [SerializeField] private float lifetime = 10f;

    [Header("Visuals")]
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector2 driftDirection;

    private void Start() {
        driftDirection = Random.insideUnitCircle.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update() {
        transform.position += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);

        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + bob * Time.deltaTime,
            transform.position.z
        );

        // Wrap around screen edges
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        if (vp.x < 0) vp.x = 1;
        if (vp.x > 1) vp.x = 0;
        if (vp.y < 0) vp.y = 1;
        if (vp.y > 1) vp.y = 0;
        transform.position = Camera.main.ViewportToWorldPoint(vp);
    }

    // When player collides with the health pack, heal them once and destroy the pack
    private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.CompareTag("Player")) {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.currentLives < gm.maxLives) {
            gm.currentLives++;
            gm.UpdateLivesUI();
            Destroy(gameObject);
        }
    }
  }
}