using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomingProjectileIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerFocus playerFocus;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visual Settings")]
    [SerializeField] private Color unlockedColor = Color.cyan;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;
    
    private bool isUnlocked = false;
    private float fadeTimer = 0f;

    private void Awake()
    {
        // Auto-find components if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        // Try to find PlayerFocus in scene
        if (playerFocus == null)
            playerFocus = FindFirstObjectByType<PlayerFocus>();

        // Subscribe to unlock event
        if (playerFocus != null)
        {
            playerFocus.OnHomingProjectilesUnlocked += OnHomingUnlocked;
            
            // Check if already unlocked
            isUnlocked = playerFocus.HasHomingProjectiles;
        }

        // Start hidden unless already unlocked
        if (canvasGroup != null)
            canvasGroup.alpha = isUnlocked ? 1f : 0f;
            
        // Set initial colors
        if (iconImage != null)
            iconImage.color = isUnlocked ? unlockedColor : lockedColor;
            
        if (statusText != null)
            statusText.text = isUnlocked ? "UNLOCKED" : "";
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerFocus != null)
        {
            playerFocus.OnHomingProjectilesUnlocked -= OnHomingUnlocked;
        }
    }

    private void Update()
    {
        if (playerFocus == null) return;

        if (isUnlocked)
        {
            // Fade in animation when first unlocked
            if (fadeTimer < fadeInDuration)
            {
                fadeTimer += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeInDuration);
            }
            else
            {
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;
            }

            // Gentle pulse the icon to show it's active
            if (iconImage != null)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                iconImage.color = Color.Lerp(unlockedColor * 0.8f, unlockedColor, pulse);
            }
        }
    }

    private void OnHomingUnlocked()
    {
        isUnlocked = true;
        fadeTimer = 0f;
        
        if (statusText != null)
            statusText.text = "UNLOCKED";
            
        Debug.Log("🎯 Homing Projectiles UI: Permanently Unlocked!");
    }
}
