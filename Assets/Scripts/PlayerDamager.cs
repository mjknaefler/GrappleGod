using UnityEngine;

public class PlayerDamager : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1.0f; // Time in seconds between hits

    // This flag tracks if the enemy can currently deal damage
    private bool canDamage = true;
    
    // We recommend tagging your Player GameObject with the "Player" tag in Unity
    private const string PlayerTag = "Player"; 

    // This method is called when the enemy's trigger (or another collider) hits another collider
    // We switch to OnTriggerEnter2D to work with colliders set as 'Is Trigger'.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collision object is the Player AND the enemy is ready to damage
        if (canDamage && other.gameObject.CompareTag(PlayerTag))
        {
            // Try to get the Health component from the colliding object
            Health playerHealth = other.gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                // Damage the player
                playerHealth.Damage(damageAmount);
                
                // Start the cooldown sequence
                StartCoroutine(DamageCooldownRoutine());
            }
        }
    }
    
    // Coroutine to manage the damage cooldown period
    private System.Collections.IEnumerator DamageCooldownRoutine()
    {
        // Set flag to false so the enemy cannot damage immediately again
        canDamage = false; 

        // Wait for the specified cooldown time
        yield return new WaitForSeconds(damageCooldown);

        // Reset the flag, allowing the enemy to damage the player again
        canDamage = true;
    }

    // Optional: If you want damage to occur on continuous contact (like fire/poison)
    // you would use OnTriggerStay2D with a timer instead of OnCollisionEnter2D.
}