using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [Header("Platform Settings")]
    [Tooltip("The platform child object that should remain when enemy dies")]
    [SerializeField] private Transform platform;
    
    private Health health;
    
    void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            // Subscribe to death event
            health.OnDeath += HandleDeath;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }
    
    void HandleDeath()
    {
        // Detach platform before enemy is destroyed
        if (platform != null)
        {
            platform.SetParent(null); // Remove from enemy's hierarchy
            Debug.Log("Platform detached from enemy!");
        }
    }
}
