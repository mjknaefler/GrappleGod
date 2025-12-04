using UnityEngine;

public class TeslaCoilHazard : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("How long warning lights blink before arc activates (seconds)")]
    [SerializeField] private float warningDuration = 1.5f;
    
    [Tooltip("How long the electric arc stays active (seconds)")]
    [SerializeField] private float arcActiveDuration = 2f;
    
    [Tooltip("How long the safe period lasts (seconds)")]
    [SerializeField] private float safeDuration = 2f;
    
    [Header("Damage Settings")]
    [Tooltip("Should the arc instantly kill the player?")]
    [SerializeField] private bool instantKill = true;
    
    [Tooltip("Damage dealt if not instant kill")]
    [SerializeField] private int damage = 999;
    
    [Header("Visual References")]
    [Tooltip("The sprite renderer for left tower warning light")]
    [SerializeField] private SpriteRenderer leftWarningLight;
    
    [Tooltip("The sprite renderer for right tower warning light")]
    [SerializeField] private SpriteRenderer rightWarningLight;
    
    [Tooltip("Warning light ON sprite (bright orange)")]
    [SerializeField] private Sprite warningLightOn;
    
    [Tooltip("Warning light OFF sprite (dim)")]
    [SerializeField] private Sprite warningLightOff;
    
    [Tooltip("The GameObject containing the electric arc (will be enabled/disabled)")]
    [SerializeField] private GameObject electricArc;
    
    [Tooltip("The box collider on the arc GameObject (for collision detection)")]
    [SerializeField] private BoxCollider2D arcCollider;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip arcActiveSound;
    [SerializeField] private AudioSource audioSource;
    
    // State tracking
    private enum HazardState { Safe, Warning, Active }
    private HazardState currentState = HazardState.Safe;
    private float stateTimer = 0f;
    private bool hasHitThisCycle = false;
    
    // Warning blink
    private float blinkTimer = 0f;
    private float blinkInterval = 0.2f; // Blink every 0.2 seconds
    
    void Start()
    {
        // Start in safe state
        SetState(HazardState.Safe);
    }
    
    void Update()
    {
        stateTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case HazardState.Safe:
                if (stateTimer >= safeDuration)
                {
                    SetState(HazardState.Warning);
                }
                break;
                
            case HazardState.Warning:
                // Blink warning lights
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    ToggleWarningLights();
                }
                
                if (stateTimer >= warningDuration)
                {
                    SetState(HazardState.Active);
                }
                break;
                
            case HazardState.Active:
                if (stateTimer >= arcActiveDuration)
                {
                    SetState(HazardState.Safe);
                }
                break;
        }
    }
    
    void SetState(HazardState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        blinkTimer = 0f;
        
        switch (newState)
        {
            case HazardState.Safe:
                // Turn everything off
                DeactivateArc();
                SetWarningLights(false);
                hasHitThisCycle = false;
                break;
                
            case HazardState.Warning:
                // Start blinking lights, arc still off
                DeactivateArc();
                SetWarningLights(true);
                
                // Play warning sound
                if (audioSource != null && warningSound != null)
                {
                    audioSource.PlayOneShot(warningSound);
                }
                break;
                
            case HazardState.Active:
                // Turn on arc, stop blinking
                ActivateArc();
                SetWarningLights(true); // Keep lights solid ON
                hasHitThisCycle = false; // Reset hit flag
                
                // Play arc sound
                if (audioSource != null && arcActiveSound != null)
                {
                    audioSource.PlayOneShot(arcActiveSound);
                }
                
                Debug.Log("Tesla Coil: Arc ACTIVE - instant death zone!");
                break;
        }
    }
    
    void ActivateArc()
    {
        if (electricArc != null)
            electricArc.SetActive(true);
        
        if (arcCollider != null)
            arcCollider.enabled = true;
    }
    
    void DeactivateArc()
    {
        if (electricArc != null)
            electricArc.SetActive(false);
        
        if (arcCollider != null)
            arcCollider.enabled = false;
    }
    
    void SetWarningLights(bool on)
    {
        if (leftWarningLight != null)
        {
            leftWarningLight.sprite = on ? warningLightOn : warningLightOff;
        }
        
        if (rightWarningLight != null)
        {
            rightWarningLight.sprite = on ? warningLightOn : warningLightOff;
        }
    }
    
    void ToggleWarningLights()
    {
        if (leftWarningLight != null)
        {
            bool isOn = leftWarningLight.sprite == warningLightOn;
            SetWarningLights(!isOn);
        }
    }
    
    // Called by TriggerForwarder when player enters arc
    public void OnPlayerEnterArc(Collider2D playerCollider)
    {
        if (currentState == HazardState.Active && !hasHitThisCycle)
        {
            hasHitThisCycle = true;
            
            Health playerHealth = playerCollider.GetComponent<Health>();
            if (playerHealth != null)
            {
                if (instantKill)
                {
                    Debug.Log($"Player hit Tesla Arc! Instant death.");
                    playerHealth.Damage(playerHealth.MaxHP);
                }
                else
                {
                    Debug.Log($"Player hit Tesla Arc! Taking {damage} damage.");
                    playerHealth.Damage(damage);
                }
            }
        }
    }
    
    // Called by TriggerForwarder every frame player is in arc
    public void OnPlayerStayInArc(Collider2D playerCollider)
    {
        // Check if player entered while arc was off, then arc turned on
        if (currentState == HazardState.Active && !hasHitThisCycle)
        {
            OnPlayerEnterArc(playerCollider);
        }
    }
    
    // Gizmo to show the hazard zone in editor
    void OnDrawGizmos()
    {
        if (arcCollider != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan
            Gizmos.matrix = arcCollider.transform.localToWorldMatrix;
            Gizmos.DrawCube(arcCollider.offset, arcCollider.size);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (arcCollider != null)
        {
            // Show danger zone more clearly when selected
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red
            Gizmos.matrix = arcCollider.transform.localToWorldMatrix;
            Gizmos.DrawCube(arcCollider.offset, arcCollider.size);
            
            // Draw label
            #if UNITY_EDITOR
            Vector3 labelPos = arcCollider.transform.position + Vector3.up * (arcCollider.size.y / 2 + 1);
            UnityEditor.Handles.Label(labelPos, 
                $"Tesla Coil\nWarning: {warningDuration}s\nActive: {arcActiveDuration}s\nSafe: {safeDuration}s");
            #endif
        }
    }
}
