using UnityEngine;
using System; // Make sure this is here for 'Action'

public class Health : MonoBehaviour
{
    // The maximum health value, configurable in the Inspector.
    [Tooltip("The starting and maximum health of the entity.")]
    [SerializeField] private int maxHP;

    // This value displays the entity's current health in the Inspector, 
    // but it is only settable by methods within this script.
    [field: SerializeField]
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