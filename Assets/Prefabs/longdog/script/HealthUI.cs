using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Player's Health script here.")]
    [SerializeField] private Health health;               
    [Tooltip("Drag your heart Image UI elements into this list.")]
    [SerializeField] private List<Image> hearts;          

    [Header("Sprites")]
    [Tooltip("Assign your full heart sprite (e.g., a colored heart).")]
    [SerializeField] private Sprite fullHeart;            
    [Tooltip("Assign your empty heart sprite (e.g., a greyed-out outline).")]
    [SerializeField] private Sprite emptyHeart;           

    private bool _subscribed;

    private void Awake()
    {
        // Validate & prefill
        if (!ValidateSprites()) return;
        if (!ValidateHearts()) return;

        // Set initial heart sprite properties
        foreach (var img in hearts)
        {
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.sprite = fullHeart; 
        }
    }

    private void OnEnable()
    {
        // Attempt to find the Health component if it wasn't manually assigned.
        // We prioritize the manual assignment, then search up the parent chain, then search globally.
        if (health == null)
        {
            health = GetComponentInParent<Health>() ?? FindAnyObjectByType<Health>();
        }
        
        // CHECK: If we have a Health script AND we haven't subscribed yet.
        if (health != null && !_subscribed)
        {
            health.OnHealthChanged += UpdateHearts;
            health.OnDeath += HandleDeath;
            _subscribed = true;
            
            // DIAGNOSTIC LOG: Confirm success
            Debug.Log($"[HealthUI] Successfully subscribed to {health.gameObject.name}'s Health events.");
        }
        else if (health == null)
        {
            // CRITICAL ERROR: This is the most likely reason for failure if the log doesn't show success.
            Debug.LogError("[HealthUI] CRITICAL: Could not find Health component to subscribe to. Drag the player's Health script into the Inspector slot.");
        }
    }

    private void Start()
    {
        if (!ValidateSprites() || !ValidateHearts() || health == null) return;

        // Ensure the subscription was successful before proceeding
        if (!_subscribed)
        {
             Debug.LogError("[HealthUI] Start() failed because event subscription failed in OnEnable. Check console for OnEnable error.");
             return;
        }

        // Check if the number of UI hearts matches the player's max health.
        if (hearts.Count != health.MaxHP) 
        {
            Debug.LogWarning($"[HealthUI] Heart count ({hearts.Count}) does not match max HP ({health.MaxHP}). UI will only display up to {hearts.Count} HP.");
        }

        // CRITICAL STEP: Initialize display to current health. This forces the UI to sync immediately.
        UpdateHearts(health.Current, health.MaxHP);
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks/errors
        if (_subscribed && health != null)
        {
            health.OnHealthChanged -= UpdateHearts;
            health.OnDeath -= HandleDeath;
            _subscribed = false;
        }
    }

    // --- Validation Methods (omitted for brevity) ---
    private bool ValidateSprites()
    {
        if (fullHeart == null)
        {
            Debug.LogError("[HealthUI] Assign fullHeart sprite in the Inspector.");
            return false;
        }
        if (emptyHeart == null)
        {
            Debug.LogWarning("[HealthUI] emptyHeart sprite not assigned. Falling back to fading the full heart sprite.");
        }
        return true;
    }

    private bool ValidateHearts()
    {
        if (hearts == null || hearts.Count == 0)
        {
            Debug.LogError("[HealthUI] Hearts list is empty. Drag your heart Image objects into the list.");
            return false;
        }
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] == null)
            {
                Debug.LogError($"[HealthUI] hearts[{i}] is null. Assign all Image references.");
                return false;
            }
        }
        return true;
    }
    
    // --- The Heart Update Logic ---
    private void UpdateHearts(int current, int max)
    {
        // Loop through all the UI Image elements you have assigned
        for (int i = 0; i < hearts.Count; i++)
        {
            // If the current health (1, 2, 3...) is greater than the index (0, 1, 2...), the heart is full.
            if (i < current)
            {
                // Heart is FULL: Use the full sprite and make it visible.
                hearts[i].sprite = fullHeart;
                hearts[i].color = Color.white; 
            }
            else
            {
                // Heart is EMPTY
                if (emptyHeart != null)
                {
                    // If you assigned an empty heart sprite, use it.
                    hearts[i].sprite = emptyHeart;
                    hearts[i].color = Color.white;
                }
                else
                {
                    // Fallback: Keep the full sprite but fade it out
                    hearts[i].color = new Color(1f, 1f, 1f, 0.2f); // Fades to a dim, almost transparent white
                }
            }
        }
        // Diagnostic log: Confirms the update successfully ran
        Debug.Log($"HealthUI updated: Current HP is {current}/{max}");
    }

    private void HandleDeath()
    {
        Debug.Log("Player died! You can add UI effects here.");
        // Optional: Hide all hearts on death
        UpdateHearts(0, hearts.Count);
    }
}