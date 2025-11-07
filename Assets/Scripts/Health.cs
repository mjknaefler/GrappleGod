using UnityEngine;
using System;
using System.Collections; // Required for Coroutines (Respawn)

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("The starting and maximum health of the entity.")]
    [SerializeField] private int maxHP;

    [Header("Respawn Settings")]
    [Tooltip("If checked, the entity (Player) will respawn. If unchecked (Enemy), it will be destroyed.")]
    [SerializeField] private bool shouldRespawn = false;
    [Tooltip("The Transform (empty GameObject) where the entity will respawn.")]
    [SerializeField] private Transform respawnPoint; 
    [Tooltip("Time in seconds before the player respawns after death.")]
    [SerializeField] private float respawnDelay = 1.0f;

    // Public property to read Max HP
    public int MaxHP => maxHP; 

    // Current health, visible in Inspector for debugging
    [field: SerializeField]
    public int Current { get; private set; }

    private Rigidbody2D rb;
    private Collider2D mainCollider; // Added to disable physics interaction
    private SpriteRenderer sr;      // Added to hide the player

    // Events
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>(); // Get the renderer component
        
        // Subscribe the Die method to the OnDeath event
        OnDeath += Die; 
        
        Current = maxHP;
    }

    private void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        OnDeath -= Die;
    }

    public void Damage(int amount)
    {
        if (amount < 0) return;

        Current -= amount;
        
        if (Current <= 0)
        {
            Current = 0;
            OnDeath?.Invoke();
        }

        OnHealthChanged?.Invoke(Current, maxHP);
    }

    public void Heal(int amount)
    {
        if (amount < 0) return;

        Current += amount;
        if (Current > maxHP)
        {
            Current = maxHP;
        }
        
        OnHealthChanged?.Invoke(Current, maxHP);
    }
    
    public void ResetHP()
    {
        Current = maxHP;
        OnHealthChanged?.Invoke(Current, maxHP);
    }
    
    // Handles the death event and routes based on the shouldRespawn flag
    private void Die()
    {
        if (shouldRespawn)
        {
            if (respawnPoint == null)
            {
                Debug.LogError("Respawn Point is not assigned! Cannot respawn.");
                // Fallback to destroying if setup is bad
                Destroy(gameObject); 
                return;
            }
            Debug.Log(gameObject.name + " has died. Starting respawn countdown.");
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // If not respawning (Enemy), destroy the object immediately.
            Debug.Log(gameObject.name + " has died and is being destroyed.");
            Destroy(gameObject);
        }
    }

    // Coroutine to handle the delayed teleport and state reset
    private IEnumerator RespawnRoutine()
    {
        // 1. Disable interaction and visual components (but keep script active!)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // Stop physics interactions
        }
        if (mainCollider != null) mainCollider.enabled = false;
        if (sr != null) sr.enabled = false; // Hide the player

        // 2. Wait for the delay
        yield return new WaitForSeconds(respawnDelay);

        // 3. Teleport and Reset
        transform.position = respawnPoint.position; 
        ResetHP(); // Heals the player back to full health

        // 4. Reactivate the player
        if (rb != null) rb.simulated = true; // Re-enable physics
        if (mainCollider != null) mainCollider.enabled = true;
        if (sr != null) sr.enabled = true; // Show the player

        Debug.Log(gameObject.name + " respawned at " + respawnPoint.name);
    }
}