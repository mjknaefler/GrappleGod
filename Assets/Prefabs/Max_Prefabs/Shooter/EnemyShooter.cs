using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform player; 
    [SerializeField] private bool autoFindPlayer = true; // Automatically find player by tag
    
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint; // Where bullets spawn from (gun tip)
    [SerializeField] private float shootInterval = 4f; // Time between shots
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float detectionRange = 15f; // How far enemy can detect player
    
    [Header("Weapon Visual")]
    [SerializeField] private SpriteRenderer weaponRenderer; // The gun sprite renderer
    [SerializeField] private bool flipWeaponWithDirection = true;
    
    private float shootTimer;
    
    void Start()
    {
        // Auto-find player if enabled
        if (autoFindPlayer && player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("EnemyShooter: No player found! Make sure player has 'Player' tag.");
            }
        }
        
        shootTimer = shootInterval; // Start ready to shoot
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Face the player
            FacePlayer();
            
            // Handle shooting timer
            shootTimer -= Time.deltaTime;
            
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval; // Reset timer
            }
        }
    }
    
    void FacePlayer()
    {
        // Determine if player is to the left or right
        if (player.position.x < transform.position.x)
        {
            // Player is to the left
            transform.localScale = new Vector3(-1, 1, 1); // Flip enemy to face left
        }
        else
        {
            // Player is to the right
            transform.localScale = new Vector3(1, 1, 1); // Face right
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("EnemyShooter: BulletPrefab or FirePoint not assigned!");
            return;
        }
        
        // Calculate direction to player
        Vector2 direction = (player.position - firePoint.position).normalized;
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // Set bullet velocity
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        
        // Rotate bullet to face direction (optional, for visual effect)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
