using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Dynamically adjusts camera zoom (orthographic size) based on player velocity.
/// Attach this to your Cinemachine Camera or Virtual Camera.
/// </summary>
public class DynamicCameraZoom : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's Rigidbody2D to track velocity.")]
    public Rigidbody2D playerRb;

    [Header("Zoom Settings")]
    [Tooltip("The default (minimum) camera size when player is stationary.")]
    public float minCameraSize = 5f;

    [Tooltip("The maximum camera size when player is at top speed.")]
    public float maxCameraSize = 8f;

    [Tooltip("The speed threshold at which max zoom is reached.")]
    public float speedThreshold = 20f;

    [Tooltip("How quickly the camera zooms in/out (lower = smoother).")]
    public float zoomSmoothTime = 0.3f;

    [Header("Advanced")]
    [Tooltip("If true, only horizontal speed affects zoom. If false, total velocity magnitude is used.")]
    public bool useHorizontalSpeedOnly = false;

    [Tooltip("Optional: Add extra zoom when grappling.")]
    public float grappleZoomBonus = 1f;

    [Tooltip("Reference to GrappleMovement to detect grappling state (optional).")]
    public GrappleMovement grappleMovement;

    private CinemachineCamera vcam;
    private float targetSize;
    private float currentVelocity; // For SmoothDamp

    void Start()
    {
        // Try to find the Cinemachine Camera component
        vcam = GetComponent<CinemachineCamera>();
        
        if (vcam == null)
        {
            Debug.LogError("DynamicCameraZoom: No CinemachineCamera found on this GameObject!");
            enabled = false;
            return;
        }

        // Auto-find player if not assigned
        if (playerRb == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerRb = player.GetComponent<Rigidbody2D>();
        }

        if (playerRb == null)
        {
            Debug.LogError("DynamicCameraZoom: No player Rigidbody2D assigned or found!");
            enabled = false;
            return;
        }

        // Auto-find GrappleMovement if not assigned
        if (grappleMovement == null && playerRb != null)
        {
            grappleMovement = playerRb.GetComponent<GrappleMovement>();
        }

        // Set initial target size
        targetSize = minCameraSize;
    }

    void LateUpdate()
    {
        if (playerRb == null || vcam == null) return;

        // Calculate player speed
        float speed = useHorizontalSpeedOnly 
            ? Mathf.Abs(playerRb.linearVelocity.x) 
            : playerRb.linearVelocity.magnitude;

        // Normalize speed to 0-1 range
        float speedRatio = Mathf.Clamp01(speed / speedThreshold);

        // Calculate base target size
        float baseTargetSize = Mathf.Lerp(minCameraSize, maxCameraSize, speedRatio);

        // Add grapple bonus if player is grappling
        if (grappleMovement != null && grappleMovement.isGrappling)
        {
            baseTargetSize += grappleZoomBonus;
        }

        targetSize = baseTargetSize;

        // Smoothly interpolate to target size
        float currentSize = vcam.Lens.OrthographicSize;
        float newSize = Mathf.SmoothDamp(currentSize, targetSize, ref currentVelocity, zoomSmoothTime);
        
        vcam.Lens.OrthographicSize = newSize;
    }

    // Optional: Draw debug info
    private void OnDrawGizmosSelected()
    {
        if (playerRb != null)
        {
            float speed = useHorizontalSpeedOnly 
                ? Mathf.Abs(playerRb.linearVelocity.x) 
                : playerRb.linearVelocity.magnitude;
            
            // This will show in Scene view when selected
            UnityEditor.Handles.Label(
                playerRb.position + Vector2.up * 2f, 
                $"Speed: {speed:F1}\nTarget Zoom: {targetSize:F1}"
            );
        }
    }
}
