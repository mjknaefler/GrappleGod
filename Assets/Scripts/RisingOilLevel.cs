using UnityEngine;

public class RisingOilLevel : MonoBehaviour
{
    [Header("Oil Level Settings")]
    [SerializeField] private float riseSpeed = 0.5f; // Units per second
    [SerializeField] private float startY = -10f; // Starting Y position (below arena)
    [SerializeField] private float maxY = 5f; // Maximum height
    [SerializeField] private int damagePerSecond = 2; // Damage when player touches oil
    [SerializeField] private float damageInterval = 1f; // How often to damage player
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer oilSpriteRenderer;
    [SerializeField] private Color oilColor = new Color(0.2f, 0.15f, 0.1f, 0.8f); // Dark brown/black
    
    [Header("Trigger Settings")]
    [SerializeField] private bool startRisingImmediately = false;
    [SerializeField] private float delayBeforeRising = 5f; // Delay before oil starts rising
    
    private bool isRising = false;
    private float damageTimer = 0f;
    private Transform playerTransform;
    private Health playerHealth;
    
    private void Start()
    {
        // Set initial position
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        
        // Set oil color if sprite renderer is assigned
        if (oilSpriteRenderer != null)
        {
            oilSpriteRenderer.color = oilColor;
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<Health>();
        }
        
        // Start rising after delay or immediately
        if (startRisingImmediately)
        {
            StartRising();
        }
        else
        {
            Invoke(nameof(StartRising), delayBeforeRising);
        }
    }
    
    private void Update()
    {
        if (isRising && transform.position.y < maxY)
        {
            // Rise slowly
            float newY = transform.position.y + (riseSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Check if player is below oil level (drowning)
            if (playerTransform != null && playerTransform.position.y < transform.position.y)
            {
                DamagePlayer();
            }
        }
    }
    
    public void StartRising()
    {
        isRising = true;
        Debug.Log("🛢️ Oil is rising! Get to higher ground!");
    }
    
    public void StopRising()
    {
        isRising = false;
    }
    
    public void SetRiseSpeed(float speed)
    {
        riseSpeed = speed;
    }
    
    private void DamagePlayer()
    {
        if (playerHealth == null) return;
        
        damageTimer += Time.deltaTime;
        
        if (damageTimer >= damageInterval)
        {
            playerHealth.Damage(damagePerSecond);
            Debug.Log($"🛢️ Player is drowning in oil! (-{damagePerSecond} HP)");
            damageTimer = 0f;
        }
    }
    
    // Optional: Call this from boss script when phase changes
    public void IncreaseRiseSpeed(float multiplier)
    {
        riseSpeed *= multiplier;
        Debug.Log($"🛢️ Oil rising faster! Speed: {riseSpeed}");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Show start and max positions in editor
        Gizmos.color = new Color(0.2f, 0.15f, 0.1f, 0.5f);
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 20f, startY, 0),
            new Vector3(transform.position.x + 20f, startY, 0)
        );
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 20f, maxY, 0),
            new Vector3(transform.position.x + 20f, maxY, 0)
        );
    }
}
