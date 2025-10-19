using UnityEngine;
using System; // Make sure this is here for 'Action'

public class Health : MonoBehaviour
{
    // Your 'maxHP' field is here
    [SerializeField] private int maxHP;

    // This lets other scripts (like your UI) read the current health,
    // but not set it. This is why it's not in the Inspector.
    public int Current { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        // Subscribe the Die method to the OnDeath event
        OnDeath += Die; 
        
        Current = maxHP;
    }

    private void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        OnDeath -= Die;
    }

    // --- FIX WAS HERE ---
    // Removed the stray '_' and added the opening curly brace '{'
    public void Damage(int amount)
    {
        if (amount < 0) return; // Don't take negative damage

        Current -= amount;
        
        if (Current <= 0)
        {
            Current = 0;
            // Fire the OnDeath event
            OnDeath?.Invoke();
        }

        // Fire the health changed event
        OnHealthChanged?.Invoke(Current, maxHP);
    }

    public void Heal(int amount)
    {
        if (amount < 0) return;

        Current += amount;
        if (Current > maxHP)
        {
            Current = maxHP;
        }
        
        OnHealthChanged?.Invoke(Current, maxHP);
    }
    
    public void ResetHP()
    {
        Current = maxHP;
        OnHealthChanged?.Invoke(Current, maxHP);
    }
    
    private void Die()
    {
        // This is the visual feedback!
        Debug.Log(gameObject.name + " has died.");
        
        // Remove the object from the game
        Destroy(gameObject);
    }
}