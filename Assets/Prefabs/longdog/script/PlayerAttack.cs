using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab; // Your fireball/bullet prefab
    [SerializeField] private Transform firePoint;       // Empty child object where it spawns

    // Components
    private Animator anim;
    private Movement movement;

    // Cooldown logic
    private float cooldownTimer = 0f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        // Simple cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // This method is called by the 'PlayerInput' component
    // when you press the "Attack" button.
    public void OnAttack(InputValue value)
    {
        // 1. Check if the cooldown is ready
        if (cooldownTimer <= 0)
        {
            // 2. Trigger the attack animation
            anim.SetTrigger("Attack"); // You must create a Trigger named "Attack" in your Animator

            // 3. Shoot the projectile
            if (projectilePrefab != null && firePoint != null)
            {
                // Create the projectile at the fire point's position and rotation
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }
            else
            {
                Debug.LogWarning("Projectile Prefab or Fire Point is not set!");
            }

            // 4. Reset the cooldown timer
            cooldownTimer = attackCooldown;
        }
    }
}