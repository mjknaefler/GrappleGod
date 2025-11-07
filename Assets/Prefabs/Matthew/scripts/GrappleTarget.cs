using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GrappleTarget : MonoBehaviour
{
    public static readonly List<GrappleTarget> All = new();

    [Header("Visuals")]
    public float baseScale = 0.28f;     // your circle’s resting size
    public float pulseAmplitude = 0.05f; // how much it grows/shrinks
    public float pulseSpeed = 3.0f;     // how fast it “breathes”

    
    [Header("Reveal")]
    [Tooltip("How far the player must be to consider this target.")]
    public float revealRadius = 9f;

    [Tooltip("This target must be at least this much ABOVE the player (world units).")]
    public float verticalBiasMin = 0.5f;

    [Tooltip("How fast the circle fades in/out.")]
    public float fadeSpeed = 10f;

    private SpriteRenderer sr;
    private float targetAlpha = 0f;
    private float t;


    void OnEnable()
    {
        All.Add(this);
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f); // start invisible
            transform.localScale = Vector3.one * baseScale;
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
        targetAlpha = visible ? 1f : 0f;
    }

    void Update()
    {
        if (sr == null) return;
        var c = sr.color;
        float a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        sr.color = new Color(c.r, c.g, c.b, a);

        // Pulse only when mostly visible
        t += Time.deltaTime * pulseSpeed;
        float vis = sr.color.a; // 0..1
        float pulse = Mathf.Sin(t) * pulseAmplitude;           // -amp..+amp
        float targetScale = baseScale + pulse * vis;           // scale fades with alpha
        transform.localScale = Vector3.one * targetScale;

    }
}
