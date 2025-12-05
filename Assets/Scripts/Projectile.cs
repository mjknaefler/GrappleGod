using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    [Header("Charge Attack State")]
    [Tooltip("Set this to true when spawning a charged projectile.")]
    public bool isCharged = false; // Used to differentiate standard vs. piercing attack
    
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
            Debug.Log($"[PROJECTILE] Using transform.Translate (homing: {(homingProjectile != null ? homingProjectile.IsHomingActive().ToString() : "no component")})");
        }
        else
        {
            Debug.Log($"[PROJECTILE] Homing is active, NOT using transform.Translate");
        }
    }

    // This runs when the projectile's trigger hits another collider (Defined ONLY ONCE)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Try to get the Health component
        Health health = other.GetComponent<Health>();
        
        if (health != null)
        {
            // We hit an entity with Health!
            health.Damage(damage);

            // Standard Attack: Destroy on impact
            if (!isCharged)
            {
                Destroy(gameObject);
            }
            // Charge Attack: Does NOT destroy on impact (it pierces)
            // It will continue until its lifeTime expires or it hits an environment object.
        }
        else if (!isCharged && !other.CompareTag("Player"))
        {
            // Standard projectiles are destroyed by hitting walls/non-enemies
            Destroy(gameObject);
        }
    }
}