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
    
    [Header("Physics-Based Movement")]
    [Tooltip("If true, uses Rigidbody2D only (disables transform movement)")]
    public bool usePhysicsOnly = false; // For physics-based projectiles like bottles/oil
    
    public enum ProjectileOwner { Player, Enemy }
    
    private HomingProjectile homingProjectile;
    private bool canCollide = true;
    
    private void Start()
    {
        // Destroy the projectile after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
        
        // Check if this projectile has homing capability
        homingProjectile = GetComponent<HomingProjectile>();
        
        // For physics projectiles, delay collision to avoid instant wall hits
        if (usePhysicsOnly)
        {
            canCollide = false;
            Invoke(nameof(EnableCollision), 0.2f);
        }
    }
    
    private void EnableCollision()
    {
        canCollide = true;
    }

    private void Update()
    {
        // Skip transform movement if using physics only
        if (usePhysicsOnly) return;
        
        // Only move with transform if:
        // 1. Homing is not active
        // 2. Speed is greater than 0 (allows physics-based projectiles to disable this)
        // 3. Rigidbody2D doesn't have manual velocity set
        if ((homingProjectile == null || !homingProjectile.IsHomingActive()) && speed > 0)
        {
            // Check if Rigidbody2D has been manually set (like champagne bottles)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                // Rigidbody2D is handling movement, don't use transform
                return;
            }
            
            // Move the projectile forward (based on its rotation)
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    // This runs when the projectile's trigger hits another collider (Defined ONLY ONCE)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Skip collision if not ready yet (for physics projectiles)
        if (!canCollide) return;
        
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
    
    // Public method to modify damage (used by boss to make bigger projectiles do more damage)
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}