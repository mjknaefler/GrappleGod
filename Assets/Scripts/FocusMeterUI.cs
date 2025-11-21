using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a UI Slider to display the player's current Focus level.
/// Attach this script to your HUD element (e.g., a Slider GameObject).
/// </summary>
public class FocusMeterUI : MonoBehaviour
{
    [Tooltip("The Slider component to visualize Focus.")]
    [SerializeField] private Slider focusSlider;
    
    [Tooltip("Reference to the PlayerFocus script on the player.")]
    [SerializeField] private PlayerFocus playerFocus;

    private void Awake()
    {
        if (focusSlider == null)
        {
            focusSlider = GetComponent<Slider>();
        }
        if (playerFocus == null)
        {
            // Try to find the focus script if not manually assigned
            playerFocus = FindAnyObjectByType<PlayerFocus>();
        }
        
        if (playerFocus == null)
        {
            Debug.LogError("[FocusMeterUI] PlayerFocus script not found in scene!");
            return;
        }

        // Initialize slider values
        focusSlider.minValue = 0;
        focusSlider.maxValue = playerFocus.MaxFocus; // FIX: Use MaxFocus property
        focusSlider.value = playerFocus.CurrentFocus;
    }

    private void OnEnable()
    {
        if (playerFocus != null)
        {
            playerFocus.OnFocusChanged += UpdateFocusUI;
        }
    }

    private void OnDisable()
    {
        if (playerFocus != null)
        {
            playerFocus.OnFocusChanged -= UpdateFocusUI;
        }
    }

    private void UpdateFocusUI(float currentFocus, float maxFocus)
    {
        // Update max value in case the player gets a power-up that increases focus capacity
        focusSlider.maxValue = maxFocus; 
        
        // Smoothly update the slider value
        focusSlider.value = currentFocus; 

        // Optional: Change slider color based on charge level
        if (currentFocus < 20)
        {
            // focusSlider.fillRect.GetComponent<Image>().color = Color.red; 
        }
        // 
    }
}