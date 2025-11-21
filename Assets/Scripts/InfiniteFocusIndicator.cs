using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a visual indicator when infinite focus is active, with a countdown timer.
/// Attach this to a UI element (Image or Panel).
/// </summary>
public class InfiniteFocusIndicator : MonoBehaviour
{
    [Tooltip("Reference to PlayerFocus (auto-finds if not set)")]
    [SerializeField] private PlayerFocus playerFocus;
    
    [Header("UI Elements")]
    [Tooltip("The image/panel that shows when infinite focus is active")]
    [SerializeField] private GameObject indicatorPanel;
    [Tooltip("Optional text to show remaining time")]
    [SerializeField] private TextMeshProUGUI timerText;
    [Tooltip("Optional image to fill/drain as a progress bar")]
    [SerializeField] private Image progressBar;
    
    [Header("Visual Settings")]
    [Tooltip("Should the indicator pulse/flash?")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Color pulseColor1 = Color.cyan;
    [SerializeField] private Color pulseColor2 = Color.white;
    
    private Image indicatorImage;
    private float totalDuration;

    private void Awake()
    {
        if (playerFocus == null)
        {
            playerFocus = FindAnyObjectByType<PlayerFocus>();
        }
        
        if (indicatorPanel == null)
        {
            indicatorPanel = gameObject;
        }
        
        indicatorImage = indicatorPanel.GetComponent<Image>();
        
        // Start hidden
        if (indicatorPanel != null)
        {
            indicatorPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerFocus != null)
        {
            playerFocus.OnInfiniteFocusChanged += OnInfiniteFocusChanged;
        }
    }

    private void OnDisable()
    {
        if (playerFocus != null)
        {
            playerFocus.OnInfiniteFocusChanged -= OnInfiniteFocusChanged;
        }
    }

    private void Update()
    {
        // Pulse effect when active
        if (enablePulse && indicatorPanel != null && indicatorPanel.activeSelf && indicatorImage != null)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            indicatorImage.color = Color.Lerp(pulseColor1, pulseColor2, t);
        }
    }

    private void OnInfiniteFocusChanged(bool isActive, float timeRemaining)
    {
        if (indicatorPanel != null)
        {
            indicatorPanel.SetActive(isActive);
        }
        
        if (isActive)
        {
            totalDuration = timeRemaining;
        }
        
        // Update timer text
        if (timerText != null)
        {
            if (isActive)
            {
                timerText.text = $"INFINITE FOCUS: {timeRemaining:F1}s";
            }
            else
            {
                timerText.text = "";
            }
        }
        
        // Update progress bar
        if (progressBar != null && isActive && totalDuration > 0)
        {
            progressBar.fillAmount = timeRemaining / totalDuration;
        }
    }
}
