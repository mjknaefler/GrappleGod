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

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isExploding = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Make sure our collider is trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploding) return;
        if (!other.CompareTag("Player")) return;

        // We touched the player -> explode
        StartExplosion(other.gameObject);
    }

    private void StartExplosion(GameObject playerObj)
    {
        isExploding = true;

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
}
