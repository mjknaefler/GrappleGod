using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomingProjectileIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerFocus playerFocus;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private float pulseSpeed = 2f;

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

        // Subscribe to events
        if (playerFocus != null)
        {
            playerFocus.OnHomingProjectilesChanged += OnHomingStatusChanged;
        }

        // Start hidden
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerFocus != null)
        {
            playerFocus.OnHomingProjectilesChanged -= OnHomingStatusChanged;
        }
    }

    private void Update()
    {
        if (playerFocus == null) return;

        if (playerFocus.HasHomingProjectiles)
        {
            // Show the indicator
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            // Pulse the icon
            if (iconImage != null)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                iconImage.color = Color.Lerp(inactiveColor, activeColor, pulse);
            }

            // Update timer text
            if (timerText != null)
            {
                float timeRemaining = playerFocus.GetHomingTimeRemaining();
                timerText.text = Mathf.Ceil(timeRemaining).ToString("F0") + "s";
            }
        }
        else
        {
            // Hide the indicator
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
    }

    private void OnHomingStatusChanged(bool isActive, float timeRemaining)
    {
        if (isActive)
        {
            Debug.Log($"Homing Projectiles UI: Activated - {timeRemaining}s remaining");
        }
        else
        {
            Debug.Log("Homing Projectiles UI: Deactivated");
        }
    }
}
