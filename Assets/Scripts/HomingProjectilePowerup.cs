using UnityEngine;

public class HomingProjectilePowerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    [Tooltip("This is a permanent upgrade - no respawn needed")]
    [SerializeField] private bool permanentUpgrade = true;

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
                // Unlock homing projectiles permanently
                playerFocus.UnlockHomingProjectiles();

                // Play effects
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                Debug.Log($"🎯 Homing Projectiles UPGRADE COLLECTED!");

                // Destroy the powerup (it's a permanent upgrade, no respawn)
                Destroy(gameObject);
            }
        }
    }
}
