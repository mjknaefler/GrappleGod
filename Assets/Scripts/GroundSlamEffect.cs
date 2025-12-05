using UnityEngine;

/// <summary>
/// Creates a ground slam shockwave effect that expands outward and damages players in range.
/// Attach to a prefab and instantiate when boss slams the ground.
/// </summary>
public class GroundSlamEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float expansionSpeed = 10f;
    [SerializeField] private float maxRadius = 8f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Impact Settings")]
    [SerializeField] private int damage = 2;
    [SerializeField] private LayerMask targetLayers; // Set to "Player" layer
    [SerializeField] private bool damageOnce = true; // Each target hit only once
    
    [Header("Components")]
    [SerializeField] private SpriteRenderer shockwaveRenderer;
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private ParticleSystem dustParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private AudioClip shockwaveSound;
    
    private float currentRadius = 0f;
    private float timer = 0f;
    private bool hasDealtDamage = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Auto-find sprite renderer if not assigned
        if (shockwaveRenderer == null)
            shockwaveRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Play impact effects
        if (impactParticles != null)
            impactParticles.Play();
            
        if (dustParticles != null)
            dustParticles.Play();
        
        // Play sounds
        if (audioSource != null)
        {
            if (slamSound != null)
                audioSource.PlayOneShot(slamSound);
            if (shockwaveSound != null)
                audioSource.PlayOneShot(shockwaveSound, 0.7f);
        }
        
        // Start small
        if (shockwaveRenderer != null)
        {
            transform.localScale = Vector3.zero;
        }
        
        // Destroy after duration
        Destroy(gameObject, duration + 0.5f);
        
        Debug.Log("💥 Ground Slam Effect spawned!");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;
        
        if (progress >= 1f) return;
        
        // Expand the shockwave
        currentRadius = Mathf.Lerp(0f, maxRadius, progress);
        
        if (shockwaveRenderer != null)
        {
            // Scale up the sprite to match radius
            float scale = currentRadius * 2f; // *2 because scale affects diameter
            transform.localScale = new Vector3(scale, scale, 1f);
            
            // Fade out using alpha curve
            Color color = shockwaveRenderer.color;
            color.a = alphaCurve.Evaluate(progress);
            shockwaveRenderer.color = color;
        }
        
        // Deal damage to targets in range
        if (!hasDealtDamage || !damageOnce)
        {
            CheckForTargets();
        }
    }

    private void CheckForTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius, targetLayers);
        
        if (hits.Length > 0 && damageOnce)
        {
            // Only process damage once
            if (hasDealtDamage) return;
            hasDealtDamage = true;
        }
        
        // Track which GameObjects we've already damaged (in case player has multiple colliders)
        System.Collections.Generic.HashSet<GameObject> damagedObjects = new System.Collections.Generic.HashSet<GameObject>();
        
        foreach (Collider2D hit in hits)
        {
            // Skip if we already damaged this GameObject
            if (damagedObjects.Contains(hit.gameObject)) continue;
            
            // Try different damage methods
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(damage);
                damagedObjects.Add(hit.gameObject);
                Debug.Log($"💥 Ground Slam hit {hit.gameObject.name} for {damage} damage! (Found {hits.Length} colliders on this GameObject)");
            }
            else
            {
                // Fallback: try SendMessage
                hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                damagedObjects.Add(hit.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Show max radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRadius);
        
        // Show current radius during play
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}
