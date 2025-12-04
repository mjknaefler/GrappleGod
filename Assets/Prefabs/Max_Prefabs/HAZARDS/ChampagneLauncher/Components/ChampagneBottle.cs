using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ChampagneBottle : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt to player")]
    [SerializeField] private int damage = 2;
    
    [Tooltip("Should this instantly kill?")]
    [SerializeField] private bool instantKill = false;
    
    [Header("Motion Settings")]
    [Tooltip("How fast the bottle spins (degrees per second)")]
    [SerializeField] private float spinSpeed = 360f;
    
    [Tooltip("How long to pause at apex (seconds)")]
    [SerializeField] private float apexPauseDuration = 0.5f;
    
    [Tooltip("Gravity scale for falling")]
    [SerializeField] private float fallGravity = 3f;
    
    [Header("Lifetime")]
    [Tooltip("Auto-destroy after this many seconds")]
    [SerializeField] private float maxLifetime = 10f;
    
    // Components
    private Rigidbody2D rb;
    private bool hasHit = false;
    private float lifetime = 0f;
    
    // Motion state
    private enum BottleState { Rising, Pausing, Falling }
    private BottleState currentState = BottleState.Rising;
    private float apexTimer = 0f;
    private float launchHeight;
    private float maxHeight;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Configure rigidbody
        rb.gravityScale = 0f; // We'll handle gravity manually
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Don't let physics rotate
    }
    
    public void Launch(float velocity, float targetMaxHeight)
    {
        launchHeight = transform.position.y;
        maxHeight = targetMaxHeight;
        
        // Launch straight up
        rb.linearVelocity = Vector2.up * velocity;
        currentState = BottleState.Rising;
        
        Debug.Log($"Bottle launched! Velocity: {velocity}, Target height: {targetMaxHeight}");
    }
    
    void Update()
    {
        // Lifetime check
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Debug.Log("Bottle lifetime expired - destroying");
            Destroy(gameObject);
            return;
        }
        
        // Spin the bottle continuously
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        
        // State machine for vertical motion
        switch (currentState)
        {
            case BottleState.Rising:
                // Check if we've reached apex
                float currentHeight = transform.position.y - launchHeight;
                
                if (rb.linearVelocity.y <= 0f || currentHeight >= maxHeight)
                {
                    // Reached peak!
                    rb.linearVelocity = Vector2.zero;
                    currentState = BottleState.Pausing;
                    apexTimer = 0f;
                    Debug.Log("Bottle reached apex - pausing");
                }
                break;
                
            case BottleState.Pausing:
                // Hold at apex while spinning
                apexTimer += Time.deltaTime;
                rb.linearVelocity = Vector2.zero; // Keep frozen in place
                
                if (apexTimer >= apexPauseDuration)
                {
                    // Start falling
                    currentState = BottleState.Falling;
                    rb.gravityScale = fallGravity;
                    Debug.Log("Bottle starting to fall");
                }
                break;
                
            case BottleState.Falling:
                // Gravity pulls it down (handled by rigidbody)
                // Check if it's gone below launch height (returned to ground)
                if (transform.position.y < launchHeight - 2f)
                {
                    Debug.Log("Bottle returned to ground - destroying");
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only damage player once
        if (hasHit) return;
        
        if (other.CompareTag("Player"))
        {
            hasHit = true;
            
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                if (instantKill)
                {
                    Debug.Log("Champagne bottle hit player - INSTANT KILL!");
                    playerHealth.Damage(playerHealth.MaxHP);
                }
                else
                {
                    Debug.Log($"Champagne bottle hit player - {damage} damage!");
                    playerHealth.Damage(damage);
                }
            }
            
            // Destroy bottle on hit
            Destroy(gameObject);
        }
    }
    
    // Optional: Destroy if goes off-screen
    void OnBecameInvisible()
    {
        // Small delay to avoid destroying immediately on spawn
        if (lifetime > 1f)
        {
            Debug.Log("Bottle went off-screen - destroying");
            Destroy(gameObject);
        }
    }
    
    // Draw gizmo showing current state
    void OnDrawGizmos()
    {
        Color stateColor = Color.white;
        
        switch (currentState)
        {
            case BottleState.Rising:
                stateColor = Color.green;
                break;
            case BottleState.Pausing:
                stateColor = Color.yellow;
                break;
            case BottleState.Falling:
                stateColor = Color.red;
                break;
        }
        
        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
