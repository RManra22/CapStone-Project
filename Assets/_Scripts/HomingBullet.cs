using UnityEngine;

public class HomingBullet : MonoBehaviour {
    [SerializeField] private float homingStrength = 3f;
    [SerializeField] private float speed = 8f;

    private Transform target;
    private Rigidbody2D rb;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        FindTarget();
    }

    private void FixedUpdate() {
        if (target == null) {
            FindTarget(); // re-acquire if target was destroyed
            if (target == null) return;
        }

        Vector2 dirToTarget = ((Vector2)target.position - rb.position).normalized;
        Vector2 newVelocity = Vector2.MoveTowards(rb.linearVelocity.normalized, dirToTarget, homingStrength * Time.fixedDeltaTime) * speed;
        rb.linearVelocity = newVelocity;

        // Rotate bullet sprite to face direction of travel
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FindTarget() {
        // Find nearest asteroid or boss
        float nearestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var col in Physics2D.OverlapCircleAll(transform.position, 20f)) {
            if (col.CompareTag("Asteroid") || col.CompareTag("Boss")) {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < nearestDist) {
                    nearestDist = dist;
                    nearest = col.transform;
                }
            }
        }

        target = nearest;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Asteroid") || collision.CompareTag("Boss")) {
            Destroy(gameObject);
        }
    }
}