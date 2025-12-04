using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LaserBeam : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt to player")]
    [SerializeField] private int damage = 2;
    
    [Tooltip("Should this instantly kill?")]
    [SerializeField] private bool instantKill = false;
    
    [Tooltip("Knockback force applied on hit")]
    [SerializeField] private float knockbackForce = 5f;
    
    [Header("Hit Cooldown")]
    [Tooltip("Time between hits (prevents rapid damage)")]
    [SerializeField] private float hitCooldown = 0.5f;
    
    // Tracking
    private float lastHitTime = -999f;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerHit(other);
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Also check while player is in beam (in case they grapple into it)
        if (other.CompareTag("Player"))
        {
            HandlePlayerHit(other);
        }
    }
    
    void HandlePlayerHit(Collider2D playerCollider)
    {
        // Check cooldown
        if (Time.time - lastHitTime < hitCooldown)
            return;
        
        lastHitTime = Time.time;
        
        // Get player health
        Health playerHealth = playerCollider.GetComponent<Health>();
        if (playerHealth != null)
        {
            if (instantKill)
            {
                Debug.Log("Player hit disco ball laser - INSTANT KILL!");
                playerHealth.Damage(playerHealth.MaxHP);
            }
            else
            {
                Debug.Log($"Player hit disco ball laser - {damage} damage!");
                playerHealth.Damage(damage);
            }
        }
        
        // Apply knockback away from disco ball center
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
        if (playerRb != null && knockbackForce > 0f)
        {
            // Calculate direction away from beam origin (disco ball center)
            Vector2 knockbackDir = (playerCollider.transform.position - transform.position).normalized;
            playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log($"Applied knockback force: {knockbackForce}");
        }
    }
    
    // Draw gizmo showing beam collision area
    void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Show beam danger zone
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Show more clearly when selected
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);
            
            // Draw outline
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}
