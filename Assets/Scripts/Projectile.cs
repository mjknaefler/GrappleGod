using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    private void Start()
    {
        // Destroy the projectile after 'lifeTime' seconds
        // to prevent it from flying forever.
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Move the projectile forward (based on its rotation)
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    // This runs when the projectile's trigger hits another collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we hit has a Health script
        Health health = other.GetComponent<Health>();
        
        // --- FIX WAS HERE ---
        // 1. Added an opening brace {
        // 2. Removed the stray '_f'
        if (health != null)
        {
            // We hit an enemy (or player)! Deal damage.
            health.Damage(damage);

            // Destroy the projectile on impact
            Destroy(gameObject);
        }
    }
}