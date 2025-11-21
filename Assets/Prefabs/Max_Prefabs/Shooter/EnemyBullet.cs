using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 1; 
    [SerializeField] private float lifetime = 5f; // Destroy after 5 seconds

    [Header("Hit Settings")]
    [SerializeField] private bool destroyOnHit = true;

    private bool hasHit = false; // Prevent multiple hits

    void Start()
    {
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return; // Already hit something, ignore

        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            hasHit = true; // Mark as hit

            // Deal damage using Health script
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Damage(damage); // Uses Damage method
                Debug.Log($"Enemy bullet hit player for {damage} damage!");
            }

            // Destroy bullet on hit
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    // If using collision instead of triggers
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return; // Already hit something, ignore

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHit = true; // Mark as hit

            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Damage(damage);
                Debug.Log($"Enemy bullet hit player for {damage} damage!");
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
