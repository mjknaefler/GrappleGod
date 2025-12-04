using UnityEngine;

public class ChampagneLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    [Tooltip("Time between launches (seconds)")]
    [SerializeField] private float launchInterval = 3f;
    
    [Tooltip("Warning duration before launch (seconds)")]
    [SerializeField] private float warningDuration = 1f;
    
    [Tooltip("How fast the bottle shoots upward")]
    [SerializeField] private float launchVelocity = 15f;
    
    [Tooltip("How high the bottle travels before falling")]
    [SerializeField] private float maxHeight = 10f;
    
    [Header("Projectile")]
    [Tooltip("Champagne bottle prefab to spawn")]
    [SerializeField] private GameObject bottlePrefab;
    
    [Tooltip("Where bottles spawn from (above turret)")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Visual Feedback")]
    [Tooltip("Warning lights (left and right) - will blink")]
    [SerializeField] private SpriteRenderer[] warningLights;
    
    [Tooltip("Pop effect animator (plays on launch)")]
    [SerializeField] private Animator popEffectAnimator;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioSource audioSource;
    
    // State tracking
    private enum LauncherState { Safe, Warning, Firing }
    private LauncherState currentState = LauncherState.Safe;
    private float stateTimer = 0f;
    private float blinkTimer = 0f;
    private bool lightsOn = false;
    
    void Start()
    {
        // Start in safe state
        SetState(LauncherState.Safe);
        
        // Auto-find spawn point if not assigned
        if (spawnPoint == null)
        {
            spawnPoint = transform;
            Debug.LogWarning($"ChampagneLauncher: No spawn point assigned, using turret position");
        }
    }
    
    void Update()
    {
        stateTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case LauncherState.Safe:
                // Wait for launch interval
                if (stateTimer >= launchInterval)
                {
                    SetState(LauncherState.Warning);
                }
                break;
                
            case LauncherState.Warning:
                // Blink warning lights
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= 0.2f) // Blink every 0.2 seconds
                {
                    blinkTimer = 0f;
                    lightsOn = !lightsOn;
                    SetWarningLights(lightsOn);
                }
                
                if (stateTimer >= warningDuration)
                {
                    SetState(LauncherState.Firing);
                }
                break;
                
            case LauncherState.Firing:
                // Fire happens instantly, then return to safe
                LaunchBottle();
                SetState(LauncherState.Safe);
                break;
        }
    }
    
    void SetState(LauncherState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        blinkTimer = 0f;
        
        switch (newState)
        {
            case LauncherState.Safe:
                // Turn off warning lights
                SetWarningLights(false);
                break;
                
            case LauncherState.Warning:
                // Start blinking
                SetWarningLights(true);
                
                // Play warning sound
                if (audioSource != null && warningSound != null)
                {
                    audioSource.PlayOneShot(warningSound);
                }
                
                Debug.Log("Champagne Launcher: WARNING - preparing to fire!");
                break;
                
            case LauncherState.Firing:
                // Keep lights solid ON
                SetWarningLights(true);
                break;
        }
    }
    
    void SetWarningLights(bool on)
    {
        if (warningLights == null || warningLights.Length == 0) return;
        
        foreach (var light in warningLights)
        {
            if (light != null)
            {
                // Toggle visibility or color
                Color c = light.color;
                light.color = new Color(c.r, c.g, c.b, on ? 1f : 0.3f);
            }
        }
    }
    
    void LaunchBottle()
    {
        if (bottlePrefab == null)
        {
            Debug.LogError("ChampagneLauncher: No bottle prefab assigned!");
            return;
        }
        
        // Spawn bottle at spawn point
        GameObject bottle = Instantiate(bottlePrefab, spawnPoint.position, Quaternion.identity);
        
        // Configure bottle
        ChampagneBottle bottleScript = bottle.GetComponent<ChampagneBottle>();
        if (bottleScript != null)
        {
            bottleScript.Launch(launchVelocity, maxHeight);
        }
        
        // Play pop effect
        if (popEffectAnimator != null)
        {
            popEffectAnimator.SetTrigger("Pop");
        }
        
        // Play launch sound
        if (audioSource != null && launchSound != null)
        {
            audioSource.PlayOneShot(launchSound);
        }
        
        Debug.Log($"Champagne Launcher: FIRED! Bottle launched at velocity {launchVelocity}");
    }
    
    // Gizmo to show launch trajectory
    void OnDrawGizmos()
    {
        if (spawnPoint == null) return;
        
        // Show spawn point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
        
        // Show max height
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Vector3 apexPos = spawnPoint.position + Vector3.up * maxHeight;
        Gizmos.DrawWireSphere(apexPos, 0.5f);
        
        // Draw trajectory line
        Gizmos.DrawLine(spawnPoint.position, apexPos);
        Gizmos.DrawLine(apexPos, spawnPoint.position);
    }
    
    void OnDrawGizmosSelected()
    {
        // Show detailed info when selected
        #if UNITY_EDITOR
        if (spawnPoint != null)
        {
            Vector3 apexPos = spawnPoint.position + Vector3.up * maxHeight;
            UnityEditor.Handles.Label(apexPos + Vector3.up * 0.5f, 
                $"Champagne Launcher\nInterval: {launchInterval}s\nWarning: {warningDuration}s\nMax Height: {maxHeight}");
        }
        #endif
    }
}
