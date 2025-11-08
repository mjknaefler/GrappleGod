using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GrappleTarget : MonoBehaviour
{
    public static readonly List<GrappleTarget> All = new();

    [Header("Visuals")]
    public float baseScale = 0.28f;     // your circle's resting size
    public float pulseAmplitude = 0.05f; // how much it grows/shrinks
    public float pulseSpeed = 3.0f;     // how fast it "breathes"

    [Header("Sprite Switching")]
    [Tooltip("Sprite to use when NOT targeted (e.g., ring/outline). Leave empty to use default sprite.")]
    public Sprite idleSprite;
    
    [Tooltip("Sprite to use when targeted (e.g., filled circle). Leave empty to use default sprite.")]
    public Sprite targetedSprite;

    [Header("Visibility")]
    [Tooltip("If true, grapple points are always visible. If false, they fade in/out.")]
    public bool alwaysVisible = true;
    
    [Tooltip("Alpha value when not targeted (0-1).")]
    [Range(0f, 1f)]
    public float idleAlpha = 0.5f;
    
    [Tooltip("Alpha value when targeted (0-1).")]
    [Range(0f, 1f)]
    public float targetedAlpha = 1f;
    
    [Header("Reveal (only used if alwaysVisible = false)")]
    [Tooltip("How far the player must be to consider this target.")]
    public float revealRadius = 9f;

    [Tooltip("This target must be at least this much ABOVE the player (world units).")]
    public float verticalBiasMin = 0.5f;

    [Tooltip("How fast the circle fades in/out.")]
    public float fadeSpeed = 10f;

    private SpriteRenderer sr;
    private float targetAlpha = 0f;
    private float t;
    private bool isTargeted = false;
    private Sprite defaultSprite;


    void OnEnable()
    {
        All.Add(this);
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Store the default sprite if we don't have custom sprites set
            defaultSprite = sr.sprite;
            
            var c = sr.color;
            // If always visible, start at idle alpha, otherwise start invisible
            float startAlpha = alwaysVisible ? idleAlpha : 0f;
            sr.color = new Color(c.r, c.g, c.b, startAlpha);
            
            // Set initial sprite
            if (idleSprite != null)
                sr.sprite = idleSprite;
            
            transform.localScale = Vector3.one * baseScale;
            targetAlpha = startAlpha;
            t = 0f;
        }
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    // The manager sets desired visibility; we smoothly fade to that alpha.
    public void SetVisible(bool visible)
    {
        isTargeted = visible;
        
        // Switch sprites based on targeted state
        if (sr != null)
        {
            if (visible && targetedSprite != null)
            {
                // Switch to filled circle when targeted
                sr.sprite = targetedSprite;
            }
            else if (!visible && idleSprite != null)
            {
                // Switch to ring/outline when not targeted
                sr.sprite = idleSprite;
            }
            else if (defaultSprite != null)
            {
                // Fallback to default sprite if custom sprites not set
                sr.sprite = defaultSprite;
            }
        }
        
        if (alwaysVisible)
        {
            // When always visible, only change between idle and targeted alpha
            targetAlpha = visible ? targetedAlpha : idleAlpha;
        }
        else
        {
            // Original behavior: fade in/out completely
            targetAlpha = visible ? 1f : 0f;
        }
    }

    void Update()
    {
        if (sr == null) return;
        
        var c = sr.color;
        float a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        sr.color = new Color(c.r, c.g, c.b, a);

        // Pulse/breathe effect
        t += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.Sin(t) * pulseAmplitude;
        
        if (alwaysVisible)
        {
            // Only pulse when targeted
            float pulseAmount = isTargeted ? pulse : 0f;
            float targetScale = baseScale + pulseAmount;
            transform.localScale = Vector3.one * targetScale;
        }
        else
        {
            // Original behavior: pulse scales with visibility
            float vis = sr.color.a;
            float targetScale = baseScale + pulse * vis;
            transform.localScale = Vector3.one * targetScale;
        }
    }
}
