using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Name of the scene to load (must match exactly)")]
    [SerializeField] private string nextSceneName = "SECONDLEVEL";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Visual Feedback")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f); // Green

    private bool isTransitioning = false;
    private Canvas fadeCanvas;
    private Image fadeImage;

    void Start()
    {
        CreateFadeCanvas();
    }

    void CreateFadeCanvas()
    {
        // Create a canvas for fading if it doesn't exist
        GameObject canvasObj = new GameObject("FadeCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Render on top of everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create the fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Start transparent

        // Make it cover the whole screen
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Don't destroy when loading new scene
        DontDestroyOnLoad(canvasObj);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered the zone
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;
            Debug.Log($"Player reached portal! Loading {nextSceneName}...");
            StartCoroutine(FadeAndLoad());
        }
    }

    IEnumerator FadeAndLoad()
    {
        // Fade to black
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // Ensure fully opaque
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Small delay at black screen
        yield return new WaitForSeconds(0.2f);

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);

        // Note: Fade in should happen in the new scene's start
        // Or you can add a LevelStart script to fade in
    }

    // Draw gizmo in Scene view for easy placement
    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // Draw the trigger zone
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);

            // Draw outline
            Gizmos.color = new Color(0f, 1f, 0f, 1f); // Solid green
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw label when selected
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 4,
            $"→ Portal to: {nextSceneName}",
            new GUIStyle() { normal = new GUIStyleState() { textColor = Color.green } }
        );
#endif
    }
}