using UnityEngine;
using System;

/// <summary>
/// Manages a resource pool (Focus) used to power special abilities like Charge Attack.
/// </summary>
public class PlayerFocus : MonoBehaviour
{
    [Header("Focus Settings")]
    [Tooltip("The maximum amount of Focus the player can hold.")]
    [SerializeField] private float maxFocus = 100f;
    [Tooltip("Focus consumed per second when charging an attack.")]
    [SerializeField] private float chargeRate = 50f;
    [Tooltip("Focus consumed immediately when launching the charge attack.")]
    [SerializeField] private float attackCost = 10f;

    // Public read-only property for Focus level
    public float CurrentFocus { get; private set; }
    
    // Public read-only property for Max Focus
    public float MaxFocus => maxFocus;
    
    // Public read-only property for infinite focus state
    public bool HasInfiniteFocus { get; private set; }

    // Event for HUD to subscribe to (Current Focus, Max Focus)
    public event Action<float, float> OnFocusChanged;
    
    // Event for infinite focus state changes (active, duration remaining)
    public event Action<bool, float> OnInfiniteFocusChanged;
    
    // Infinite focus tracking
    private float infiniteFocusDuration = 0f;

    private void Awake()
    {
        CurrentFocus = maxFocus;
    }
    
    private void Start()
    {
        // Trigger initial UI update
        OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
    }

    private void Update()
    {
        // Handle infinite focus timer
        if (HasInfiniteFocus)
        {
            infiniteFocusDuration -= Time.deltaTime;
            OnInfiniteFocusChanged?.Invoke(true, infiniteFocusDuration);
            
            if (infiniteFocusDuration <= 0f)
            {
                // Infinite focus expired
                HasInfiniteFocus = false;
                OnInfiniteFocusChanged?.Invoke(false, 0f);
                Debug.Log("Infinite Focus expired!");
            }
            
            // Keep focus at max during infinite mode
            CurrentFocus = maxFocus;
            OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
        }
        else
        {
            // Simple regeneration over time if not at max
            if (CurrentFocus < maxFocus)
            {
                // Regenerate 5% of charge rate per second
                CurrentFocus += chargeRate * 0.05f * Time.deltaTime;
                CurrentFocus = Mathf.Clamp(CurrentFocus, 0f, maxFocus);
                OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
            }
        }
    }

    /// <summary>
    /// Attempts to consume Focus for a sustained action (like holding a button).
    /// </summary>
    /// <returns>True if cost was successfully paid, false otherwise.</returns>
    public bool ConsumeFocusOnCharge()
    {
        // Infinite focus = always succeed without consuming
        if (HasInfiniteFocus)
            return true;
            
        float cost = chargeRate * Time.deltaTime;
        if (CurrentFocus >= cost)
        {
            CurrentFocus -= cost;
            OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to pay the fixed cost for launching the attack.
    /// </summary>
    /// <returns>True if cost was successfully paid, false otherwise.</returns>
    public bool ConsumeFocusOnLaunch()
    {
        // Infinite focus = always succeed without consuming
        if (HasInfiniteFocus)
            return true;
            
        if (CurrentFocus >= attackCost)
        {
            CurrentFocus -= attackCost;
            OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Activates infinite focus mode for a specified duration.
    /// </summary>
    /// <param name="duration">How long infinite focus lasts (in seconds)</param>
    public void ActivateInfiniteFocus(float duration)
    {
        HasInfiniteFocus = true;
        infiniteFocusDuration = duration;
        CurrentFocus = maxFocus; // Fill to max immediately
        
        OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
        OnInfiniteFocusChanged?.Invoke(true, duration);
        
        Debug.Log($"Infinite Focus activated for {duration} seconds!");
    }

    /// <summary>
    /// Instantly adds Focus (used by the Damage Syphon power-up).
    /// </summary>
    public void AddFocus(float amount)
    {
        CurrentFocus += amount;
        CurrentFocus = Mathf.Clamp(CurrentFocus, 0f, maxFocus);
        OnFocusChanged?.Invoke(CurrentFocus, maxFocus);
    }
}
