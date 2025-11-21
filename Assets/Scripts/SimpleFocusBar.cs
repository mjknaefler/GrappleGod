using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple, robust focus bar that updates every frame.
/// Attach to a UI Slider GameObject.
/// </summary>
public class SimpleFocusBar : MonoBehaviour
{
    [Tooltip("The Slider component (auto-finds if not set)")]
    [SerializeField] private Slider slider;
    
    [Tooltip("Reference to PlayerFocus (auto-finds if not set)")]
    [SerializeField] private PlayerFocus playerFocus;
    
    [Header("Optional Visual Feedback")]
    [Tooltip("Fill image to color based on focus level")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Color highFocusColor = Color.cyan;
    [SerializeField] private Color lowFocusColor = Color.red;
    [SerializeField] private float lowFocusThreshold = 0.3f;
    [SerializeField] private Color infiniteFocusColor = Color.yellow;
    [SerializeField] private bool pulseWhenInfinite = true;

    private void Awake()
    {
        // Auto-find components
        if (slider == null)
            slider = GetComponent<Slider>();
            
        if (playerFocus == null)
            playerFocus = FindAnyObjectByType<PlayerFocus>();
            
        if (fillImage == null && slider != null)
            fillImage = slider.fillRect?.GetComponent<Image>();
            
        // Validate
        if (slider == null)
        {
            Debug.LogError("[SimpleFocusBar] No Slider found!");
            enabled = false;
            return;
        }
        
        if (playerFocus == null)
        {
            Debug.LogError("[SimpleFocusBar] No PlayerFocus found!");
            enabled = false;
            return;
        }
        
        // Initialize slider range
        slider.minValue = 0;
        slider.maxValue = 1; // We'll use normalized values (0-1)
        slider.value = 1;
    }

    private void Update()
    {
        // Update slider every frame - simple and always correct
        float normalizedFocus = playerFocus.CurrentFocus / playerFocus.MaxFocus;
        slider.value = normalizedFocus;
        
        // Optional: Update color based on focus level
        if (fillImage != null)
        {
            // Special visual for infinite focus mode
            if (playerFocus.HasInfiniteFocus)
            {
                if (pulseWhenInfinite)
                {
                    // Pulse between infinite color and white
                    float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                    fillImage.color = Color.Lerp(infiniteFocusColor, Color.white, pulse);
                }
                else
                {
                    fillImage.color = infiniteFocusColor;
                }
            }
            else if (normalizedFocus < lowFocusThreshold)
            {
                fillImage.color = lowFocusColor;
            }
            else
            {
                fillImage.color = Color.Lerp(lowFocusColor, highFocusColor, 
                    (normalizedFocus - lowFocusThreshold) / (1f - lowFocusThreshold));
            }
        }
    }
}
