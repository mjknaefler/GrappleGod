using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;               // Drag your Player's Health here
    [SerializeField] private List<Image> hearts;          // Drag your heart Images here

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;            // Assign your full heart PNG

    private bool _subscribed;

    private void Awake()
    {
        // Validate & prefill
        if (!ValidateSprites()) return;
        if (!ValidateHearts()) return;

        // Ensure each Image has the fullHeart sprite at startup
        foreach (var img in hearts)
        {
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.sprite = fullHeart; 
        }
    }

    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponentInParent<Health>() ?? FindAnyObjectByType<Health>();
        }
        
        if (health != null && !_subscribed)
        {
            health.OnHealthChanged += UpdateHearts;
            health.OnDeath += HandleDeath;
            _subscribed = true;
        }
    }

    private void Start()
    {
        if (!ValidateSprites() || !ValidateHearts() || health == null) return;

        // Initialize display to current health
        UpdateHearts(Mathf.Clamp(health.Current, 0, hearts.Count), hearts.Count);
    }

    private void OnDisable()
    {
        if (_subscribed && health != null)
        {
            health.OnHealthChanged -= UpdateHearts;
            health.OnDeath -= HandleDeath;
            _subscribed = false;
        }
    }

    private bool ValidateSprites()
    {
        if (fullHeart == null)
        {
            Debug.LogError("[HealthUI] Assign fullHeart sprite in the Inspector.");
            return false;
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
    
    // --- THIS METHOD IS FIXED ---
    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < current)
            {
                // This is a "full" heart. Make it fully visible (white, full alpha).
                hearts[i].color = new Color(1f, 1f, 1f, 1f); 
            }
            else
            {
                // This is an "empty" heart. Make it fully invisible (zero alpha).
                hearts[i].color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player died!");
    }
}