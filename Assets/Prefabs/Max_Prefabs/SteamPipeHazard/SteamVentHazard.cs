using UnityEngine;

public class SteamVentHazard : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float cycleTime = 4f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float steamDuration = 1.5f;

    [Header("Damage")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private Vector2 knockbackDirection = Vector2.down;

    [Header("References")]
    [SerializeField] private GameObject steamObject;
    [SerializeField] private AudioSource steamSound;

    [Header("Warning Lights - Set 1")]
    [SerializeField] private GameObject warningLight1_Off;
    [SerializeField] private GameObject warningLight1_On;

    [Header("Warning Lights - Set 2")]
    [SerializeField] private GameObject warningLight2_Off;
    [SerializeField] private GameObject warningLight2_On;

    [Header("Collision")]
    [SerializeField] private Collider2D steamCollider;

    private float timer;
    private bool isSteamActive = false;
    private bool isWarning = false;
    private bool hasHitThisCycle = false;

    void Start()
    {
        // Start with steam off
        if (steamObject != null)
            steamObject.SetActive(false);

        // Both lights start OFF
        SetWarningLightsOff();

        // Disable steam collider
        if (steamCollider != null)
            steamCollider.enabled = false;

        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // State machine for vent cycle
        if (timer < warningDuration)
        {
            // WARNING PHASE
            if (!isWarning)
            {
                StartWarning();
            }
            BlinkWarning();
        }
        else if (timer < warningDuration + steamDuration)
        {
            // STEAM ACTIVE PHASE
            if (!isSteamActive)
            {
                ActivateSteam();
            }
        }
        else if (timer >= cycleTime)
        {
            // RESET PHASE
            DeactivateSteam();
            timer = 0f;
        }
        else
        {
            // IDLE PHASE
            if (isSteamActive)
            {
                DeactivateSteam();
            }
        }
    }

    void StartWarning()
    {
        isWarning = true;
    }

    void BlinkWarning()
    {
        // Blink both warning lights in sync
        float blinkSpeed = 8f;
        bool showOff = Mathf.Sin(Time.time * blinkSpeed) > 0;

        // Light 1
        if (warningLight1_Off != null)
            warningLight1_Off.SetActive(showOff);
        if (warningLight1_On != null)
            warningLight1_On.SetActive(true);

        // Light 2
        if (warningLight2_Off != null)
            warningLight2_Off.SetActive(showOff);
        if (warningLight2_On != null)
            warningLight2_On.SetActive(true);
    }

    void ActivateSteam()
    {
        isSteamActive = true;
        isWarning = false;
        hasHitThisCycle = false;

        // Turn off all warning lights
        SetWarningLightsOff();

        // Show steam visual
        if (steamObject != null)
            steamObject.SetActive(true);

        // Enable steam collision
        if (steamCollider != null)
            steamCollider.enabled = true;

        // Play sound effect
        if (steamSound != null)
            steamSound.Play();

        Debug.Log("Steam vent activated!");
    }

    void DeactivateSteam()
    {
        isSteamActive = false;
        isWarning = false;
        hasHitThisCycle = false;

        // Hide steam visual
        if (steamObject != null)
            steamObject.SetActive(false);

        // Disable steam collision
        if (steamCollider != null)
            steamCollider.enabled = false;

        // Show OFF state only
        SetWarningLightsOff();
    }

    void SetWarningLightsOff()
    {
        // Light 1 - show OFF, hide ON
        if (warningLight1_Off != null)
            warningLight1_Off.SetActive(true);
        if (warningLight1_On != null)
            warningLight1_On.SetActive(false);

        // Light 2 - show OFF, hide ON
        if (warningLight2_Off != null)
            warningLight2_Off.SetActive(true);
        if (warningLight2_On != null)
            warningLight2_On.SetActive(false);
    }


    public void OnSteamCollision(Collider2D other)
    {
        // Only damage when steam is active
        if (!isSteamActive) return;

        // Prevent multiple hits in the same steam cycle
        if (hasHitThisCycle) return;

        Debug.Log($"Steam collision! Hit: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            hasHitThisCycle = true; // Mark that we hit this cycle

            // Get Health component
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Damage(damage);
                Debug.Log($"Dealt {damage} damage to player!");
            }

            // Apply knockback - OPPOSITE to player's movement direction
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Get the OPPOSITE of player's current velocity
                Vector2 knockbackDirection = -rb.linearVelocity.normalized;

                // If player is barely moving, knock them up instead
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    knockbackDirection = Vector2.up;
                }
                else
                {
                    // Add upward component for better feel (optional)
                    knockbackDirection = (knockbackDirection + Vector2.up * 0.5f).normalized;
                }

                // DIRECTLY SET velocity instead of adding force
                rb.linearVelocity = knockbackDirection * knockbackForce;

                Debug.Log($"Applied knockback! Player was moving: {rb.linearVelocity}, knocked: {knockbackDirection}");
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

        if (steamCollider != null)
        {
            Gizmos.DrawCube(steamCollider.bounds.center, steamCollider.bounds.size);
        }
    }
}
