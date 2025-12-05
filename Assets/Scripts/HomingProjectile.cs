using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("Homing Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D rb;
    private Transform target;
    private bool homingEnabled = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("HomingProjectile requires a Rigidbody2D component!");
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        
        // Make sure gravity is off
        rb.gravityScale = 0f;
        
        Debug.Log($"HomingProjectile initialized. Homing: {homingEnabled}, Enemy Layer: {enemyLayer.value}");
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Only home if enabled
        if (homingEnabled)
        {
            Debug.Log($"[HOMING UPDATE] Enabled: {homingEnabled}, Target: {(target != null ? target.name : "NULL")}");
            
            // 1. Find a target if we don't have one
            if (target == null)
            {
                FindNearestEnemy();
            }

            // 2. Movement Logic
            if (target != null)
            {
                // Calculate direction to target
                Vector2 direction = (Vector2)target.position - rb.position;
                direction.Normalize();

                // Rotate smoothly towards target
                float rotateAmount = Vector3.Cross(direction, transform.right).z;
                rb.angularVelocity = -rotateAmount * rotateSpeed;
                
             //   Debug.Log($"[HOMING] Rotating towards {target.name}, angular velocity: {rb.angularVelocity}");
            }
        }
        else
        {
           // Debug.Log($"[HOMING UPDATE] NOT ENABLED - homingEnabled: {homingEnabled}");
        }

        // Always move forward
        rb.linearVelocity = transform.right * speed;
    }
    
    public void EnableHoming()
    {
        homingEnabled = true;
     //   Debug.Log($"Homing enabled on projectile! Enemy Layer: {enemyLayer.value}, Detection Radius: {detectionRadius}");
    }
    
    public bool IsHomingActive()
    {
        return homingEnabled;
    }

    private void FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Debug.Log($"Searching for enemies... Found {enemies.Length} enemies in radius {detectionRadius}");
        
        float shortestDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider2D enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            Debug.Log($"Found enemy: {enemy.name} at distance {dist}");
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearest = enemy.transform;
            }
        }
        
        target = nearest;
        if (target != null)
        {
            Debug.Log($"Target locked: {target.name}");
        }
        else
        {
            Debug.Log("No target found!");
        }
    }
    
    // Standard trigger damage logic (copy-paste from your standard projectile or reuse components)
    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.Damage(1); // Or whatever damage amount
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}