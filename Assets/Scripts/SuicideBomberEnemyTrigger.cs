using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SuicideBomberEnemyTrigger : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Damage / Explosion")]
    public int damageAmount = 1;
    public float destroyDelay = 0.7f;      // time to let Death anim play
    public string deathTriggerName = "Death";
    public float armTime = 0.3f;           // delay before explosion can trigger
    public float explodeRange = 1.5f;      // distance at which it auto-explodes

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isExploding = false;
    private bool isArmed = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Make sure our collider is trigger (used for overlap, but not required for explosion anymore)
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Arm after a short delay so it doesn't instantly blow up on spawn
        Invoke(nameof(Arm), armTime);
    }

    private void Arm()
    {
        isArmed = true;
    }

    private void Update()
    {
        if (player == null || isExploding) return;

        // Move straight toward the player
        Vector2 dir = (player.position - transform.position).normalized;

        if (rb != null)
            rb.linearVelocity = dir * moveSpeed;
        else
            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;

        // ---- NEW: explode when close enough ----
        if (isArmed)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= explodeRange)
            {
                StartExplosion(player.gameObject);
            }
        }
    }

    // OnTriggerEnter is now optional; you can even delete this if you want ONLY distance-based explosion
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed) return;
        if (isExploding) return;
        if (!other.CompareTag("Player")) return;

        // If you *do* physically touch the player, also explode
        StartExplosion(other.gameObject);
    }

    private void StartExplosion(GameObject playerObj)
    {
        if (isExploding) return;
        isExploding = true;

        Debug.Log("StartExplosion CALLED on " + gameObject.name);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Damage
        Health playerHealth = playerObj.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.Damage(damageAmount);

        // Play Death animation
        if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
            animator.SetTrigger(deathTriggerName);

        // Destroy after a short delay
        Destroy(gameObject, destroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}
