using UnityEngine;

public class InstantDeathHazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("Tag of objects that should die when touching this hazard")]
    [SerializeField] private string targetTag = "Player";
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showWarningGizmo = true;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that touched us is the player (or target)
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"{other.name} touched hazard and died instantly!");
            
            // Get the Health component and kill them instantly
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                // Deal massive damage to instantly kill
                health.Damage(health.MaxHP);
            }
            else
            {
                // Fallback: just destroy the object if no health component
                Debug.LogWarning($"{other.name} has no Health component! Destroying directly.");
                Destroy(other.gameObject);
            }
        }
    }
    
    // Optional: Also handle if using physics colliders instead of triggers
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            Debug.Log($"{collision.gameObject.name} collided with hazard and died instantly!");
            
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(health.MaxHP);
            }
            else
            {
                Debug.LogWarning($"{collision.gameObject.name} has no Health component! Destroying directly.");
                Destroy(collision.gameObject);
            }
        }
    }
    
    // Draw a red warning box in the editor
    private void OnDrawGizmos()
    {
        if (showWarningGizmo)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Transparent red
            
            // Get the collider bounds
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}
