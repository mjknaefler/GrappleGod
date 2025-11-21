using UnityEngine;

/// <summary>
/// A pickup item that instantly restores a percentage of the player's Focus meter.
/// </summary>
public class DamageSyphon : MonoBehaviour
{
    [Tooltip("Focus amount to restore instantly.")]
    [SerializeField] private float focusRestoreAmount = 50f;
    
    [Tooltip("The Tag used to identify the Player.")]
    [SerializeField] private string playerTag = "Player";
    
    // Assumes this object has a Collider2D set to Is Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            PlayerFocus focus = other.GetComponent<PlayerFocus>();
            
            if (focus != null)
            {
                // Add focus to the player
                focus.AddFocus(focusRestoreAmount);
                
                // Optional: Play a sound effect or particle system for the pickup
                
                Debug.Log("Damage Syphon collected! Focus restored.");
                
                // Destroy the power-up item
                Destroy(gameObject);
            }
        }
    }
}