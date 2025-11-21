using UnityEngine;

/// <summary>
/// A pickup that grants the player infinite focus for a limited time.
/// Attach this to a pickup GameObject with a Collider2D (set to Trigger).
/// </summary>
public class InfiniteFocusPowerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    [Tooltip("How long infinite focus lasts (in seconds)")]
    [SerializeField] private float duration = 10f;
    
    [Header("Visual & Audio (Optional)")]
    [Tooltip("Optional particle effect to spawn on pickup")]
    [SerializeField] private GameObject pickupEffect;
    [Tooltip("Optional audio clip to play on pickup")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Respawn Settings")]
    [Tooltip("Should this powerup respawn after being collected?")]
    [SerializeField] private bool respawns = true;
    [Tooltip("Time until respawn (in seconds)")]
    [SerializeField] private float respawnTime = 30f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isCollected = false;
    private float respawnTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        if (col != null)
        {
            col.isTrigger = true; // Ensure it's a trigger
        }
    }

    private void Update()
    {
        // Handle respawn timer
        if (isCollected && respawns)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        
        // Optional: Add floating/rotating animation
        if (!isCollected)
        {
            transform.Rotate(0, 0, 90f * Time.deltaTime);
            transform.position += Vector3.up * Mathf.Sin(Time.time * 2f) * 0.5f * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if already collected
        if (isCollected)
            return;
            
        // Check if it's the player
        PlayerFocus playerFocus = other.GetComponent<PlayerFocus>();
        if (playerFocus != null)
        {
            // Activate infinite focus
            playerFocus.ActivateInfiniteFocus(duration);
            
            // Play effects
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Hide the powerup
            isCollected = true;
            
            if (respawns)
            {
                // Hide but don't destroy
                respawnTimer = respawnTime;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (col != null) col.enabled = false;
                
                Debug.Log($"Infinite Focus powerup collected! Will respawn in {respawnTime}s");
            }
            else
            {
                // Destroy permanently
                Debug.Log("Infinite Focus powerup collected!");
                Destroy(gameObject);
            }
        }
    }

    private void Respawn()
    {
        isCollected = false;
        
        // Re-enable visuals and collision
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;
        
        Debug.Log("Infinite Focus powerup respawned!");
    }

    private void OnDrawGizmos()
    {
        // Draw a visual indicator in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
