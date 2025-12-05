using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    [Header("Projectile Owner")]
    [Tooltip("Who shot this projectile? Player or Enemy?")]
    [SerializeField] private ProjectileOwner owner = ProjectileOwner.Player;

    [Header("Charge Attack State")]
    [Tooltip("Set this to true when spawning a charged projectile.")]
    public bool isCharged = false; // Used to differentiate standard vs. piercing attack
    
    public enum ProjectileOwner { Player, Enemy }
    
    private HomingProjectile homingProjectile;
    
    private void Start()
    {
        // Destroy the projectile after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
        
        // Check if this projectile has homing capability
        homingProjectile = GetComponent<HomingProjectile>();
    }

    private void Update()
    {
        // Only move with transform if homing is not active
        // If homing is active, let HomingProjectile handle movement with Rigidbody2D
        if (homingProjectile == null || !homingProjectile.IsHomingActive())
        {
            // Move the projectile forward (based on its rotation)
            transform.Translate(Vector2.right * speed * Time.deltaTime);
           // Debug.Log($"[PROJECTILE] Using transform.Translate (homing: {(homingProjectile != null ? homingProjectile.IsHomingActive().ToString() : "no component")})");
        }
        else
        {
            // Debug.Log($"[PROJECTILE] Homing is active, NOT using transform.Translate");
        }
    }

    // This runs when the projectile's trigger hits another collider (Defined ONLY ONCE)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // PLAYER PROJECTILES: Don't hit the player, only hit enemies
        if (owner == ProjectileOwner.Player)
        {
            // Skip if we hit the player (our own projectiles)
            if (other.CompareTag("Player")) return;
            
            // Try to damage Health component (regular enemies)
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(damage);
                
                if (!isCharged)
                    Destroy(gameObject);
                return;
            }
            
            // Try to damage boss with custom health system
            PuffDaddyBoss boss = other.GetComponent<PuffDaddyBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                
                if (!isCharged)
                    Destroy(gameObject);
                return;
            }
        }
        // ENEMY PROJECTILES: Don't hit enemies, only hit player
        else if (owner == ProjectileOwner.Enemy)
        {
            // Only damage the player
            if (other.CompareTag("Player"))
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(damage);
                }
                
                // Enemy projectiles always destroy on player hit
                Destroy(gameObject);
                return;
            }
            
            // Don't hit other enemies/bosses
            if (other.GetComponent<PuffDaddyBoss>() != null) return;
            if (other.GetComponent<Health>() != null && !other.CompareTag("Player")) return;
        }
        
        // Hit a wall or environment object (both types destroy)
        if (!isCharged)
        {
            // Standard projectiles are destroyed by hitting walls/non-enemies
            Destroy(gameObject);
        }
    }
}