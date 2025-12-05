using UnityEngine;

public class HomingProjectilePowerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] private float duration = 15f;
    [Tooltip("Should this powerup respawn after being collected?")]
    [SerializeField] private bool respawns = true;
    [SerializeField] private float respawnTime = 45f;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Feedback")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Vector3 startPosition;
    private float bobTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;
    }

    private void Update()
    {
        // Rotate the powerup
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Bob up and down
        bobTimer += Time.deltaTime * bobSpeed;
        float newY = startPosition.y + Mathf.Sin(bobTimer) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerFocus playerFocus = other.GetComponent<PlayerFocus>();
            if (playerFocus != null)
            {
                // Activate homing projectiles
                playerFocus.ActivateHomingProjectiles(duration);

                // Play effects
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                Debug.Log($"Homing Projectiles activated for {duration} seconds!");

                // Handle respawn or destruction
                if (respawns)
                {
                    StartCoroutine(RespawnCoroutine());
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine()
    {
        // Hide the powerup
        spriteRenderer.enabled = false;
        col.enabled = false;

        // Wait for respawn time
        yield return new WaitForSeconds(respawnTime);

        // Show the powerup again
        spriteRenderer.enabled = true;
        col.enabled = true;
        transform.position = startPosition;
        bobTimer = 0f;

        Debug.Log("Homing Projectile powerup respawned!");
    }
}
