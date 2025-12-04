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
        if (platform != null)
        {
            platform.SetParent(null);

            // FORCE the Ground layer (by index)
            int groundLayer = LayerMask.NameToLayer("Ground");

            if (groundLayer != -1)
            {
                platform.gameObject.layer = groundLayer;
                Debug.Log($"Platform forced to Ground layer (index {groundLayer})");
            }
            else
            {
                Debug.LogError("Ground layer doesn't exist!");
            }

            // Also force collider settings
            Collider2D col = platform.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
                col.isTrigger = false;
            }
        }
    }
}
