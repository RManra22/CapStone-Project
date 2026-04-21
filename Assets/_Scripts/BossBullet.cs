using UnityEngine;

public class BossBullet : MonoBehaviour {
    [SerializeField] private float bulletLifetime = 1f;

    private void Awake() {
        Destroy(gameObject, bulletLifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Destroy(gameObject);
        }
    }
}