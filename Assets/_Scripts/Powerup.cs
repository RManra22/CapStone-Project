using System.Collections;
using UnityEngine;

public class Powerup : MonoBehaviour {
  [Header("Movement")]
  [SerializeField] private float driftSpeed = 1.5f;

  [Header("Visuals")]
  [SerializeField] private float bobAmplitude = 0.2f;
  [SerializeField] private float bobFrequency = 2f;
  [SerializeField] private float lifetime = 10f; // despawn after this long if uncollected

  private Vector2 driftDirection;
  private Vector3 startPosition;

  private void Start() {
    // Pick a random drift direction
    driftDirection = Random.insideUnitCircle.normalized;
    startPosition = transform.position;

    Destroy(gameObject, lifetime);
  }

  private void Update() {
    // Drift across the screen
    transform.position += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);

    // Bob up and down on top of the drift
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

  private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.CompareTag("Player")) {
      Player player = collision.GetComponent<Player>();
      if (player != null) {
        player.ApplyFastShootPowerup();
        Destroy(gameObject);
      }
    }
  }
}