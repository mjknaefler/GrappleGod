using UnityEngine;
using System.Collections;

public class SkeletonAttackAI : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;    // how close player must be
    public float attackCooldown = 1.0f; // time between slashes

    [Header("Attack Hitbox")]
    public Collider2D attackHitbox;     // assign the SlashHitbox collider here

    private Transform player;
    private Animator anim;
    private bool isAttacking = false;

    void Awake()
    {
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("SkeletonAttackAI: No object with tag 'Player' found.");
        }

        // Make sure hitbox starts disabled
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // If in range and not currently in an attack sequence, start one
        if (dist <= attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Trigger the slash animation
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Wait for cooldown before we allow another attack
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // Called by Animation Events on the Attack animation
    public void EnableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
        }
    }

    // Called by Animation Events on the Attack animation
    public void DisableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }
}
