using UnityEngine;

public class SuicideBomberEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float triggerRange = 1.5f;

    [Header("Damage")]
    public int damageAmount = 1;

    [Header("Explosion Timing")]
    public float armTime = 0.5f;        // must live this long before exploding
    public float destroyDelay = 0.7f;   // time to let Death animation play

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isExploding = false;
    private float spawnTime;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
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

        // Move toward player
        Vector2 dir = (player.position - transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = dir * moveSpeed;
        else
            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;

        // Too early to explode? bail out
        if (Time.time < spawnTime + armTime) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= triggerRange)
        {
            StartExplosion();
        }
    }

    private void StartExplosion()
    {
        if (isExploding) return;
        isExploding = true;

        Debug.Log("SuicideBomberEnemy: Exploding!");

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Damage player once
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.Damage(damageAmount);

        // Play Death animation
        if (animator != null)
            animator.SetTrigger("Death");

        // Destroy this enemy after the animation
        Destroy(gameObject, destroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
