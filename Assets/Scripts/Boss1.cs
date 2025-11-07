using System.Collections;
using UnityEngine;

public class Boss1 : MonoBehaviour
{
    [Header("Flight/Spacing")]
    [SerializeField] float preferredRadius = 4.0f; // where it wants to hover
    [SerializeField] float innerKeepOut    = 2.0f; // if closer than this, back off
    [SerializeField] float outerLeash      = 9.0f; // if farther than this, move in
    [SerializeField] float orbitSpeed      = 3.0f; // tangential speed around player
    [SerializeField] float approachSpeed   = 5.0f;
    [SerializeField] float retreatSpeed    = 7.0f;
    [SerializeField] float maxSpeed        = 7.0f;
    [SerializeField] float accel           = 20.0f;
    [SerializeField] int   orbitDir        = 1;    // 1 = clockwise, -1 = counter

    [Header("Refs")]
    [SerializeField] Animator animator;                 // assign the boss animator
    [SerializeField] Rigidbody2D rb;                    // movement body
    [SerializeField] Transform target;                  // auto-finds Player if left empty
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform hitOrigin;               // empty child in front of boss
    [SerializeField] Transform projectileSpawn;         // empty child where projectiles spawn
    [SerializeField] GameObject projectilePrefab;       // optional (for Attack 2)

    [Header("Animator State Names (must match Animator)")]
    [SerializeField] string idleState   = "Wooden Aarakocra Idle Animation";
    [SerializeField] string atk1State   = "Wooden Aarakocra Attack 1Animation";
    [SerializeField] string atk2State   = "Wooden Aarakocra Attack 2 Animation";

    [Header("Behavior")]
    [SerializeField] float chaseSpeed = 2.0f;           // (not used for flight, kept if you toggle modes)
    [SerializeField] float stopDistance = 1.25f;
    [SerializeField] float stateCrossfade = 0.05f;

    [Header("Attack 1 (Melee)")]
    [SerializeField] float atk1Range = 1.6f;
    [SerializeField] float atk1Damage = 12f;
    [SerializeField] float atk1Cooldown = 2.5f;
    [SerializeField] float atk1Telegraph = 0.25f;
    [SerializeField] Vector2 atk1BoxSize = new Vector2(1.6f, 1.0f);
    [SerializeField] Vector2 atk1BoxOffset = new Vector2(0.9f, 0.0f);

    [Header("Attack 2 (Projectile or Melee Fallback)")]
    [SerializeField] float atk2MinRange = 3.0f;
    [SerializeField] float atk2MaxRange = 8.0f;
    [SerializeField] float atk2Damage = 8f;
    [SerializeField] float atk2Cooldown = 1.4f;
    [SerializeField] float atk2Telegraph = 0.35f;
    [SerializeField] float projectileSpeed = 9f;

    float cd1, cd2;
    bool busy;
    int nextAttack = 1;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        // Keep the flyer from spinning and make hover smoother
        if (rb)
        {
            rb.gravityScale  = 0f;
            rb.linearDamping          = 4f;   // linear drag for smooth hover
            rb.angularDamping   = 0f;
            rb.constraints   = RigidbodyConstraints2D.FreezeRotation;
        }

        // Ensure animations don't rotate/translate the root
        if (animator) animator.applyRootMotion = false;
    }

    void Update()
    {
        cd1 -= Time.deltaTime; 
        cd2 -= Time.deltaTime;
        if (!target) return;

        // face player (flip sprite)
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (target.position.x >= transform.position.x ? 1 : -1);
        transform.localScale = s;

        // hard-lock orientation every frame (prevents random spinning)
        transform.rotation = Quaternion.identity;
        if (rb) rb.angularVelocity = 0f;

        if (busy) { if (rb) rb.linearVelocity = Vector2.zero; return; }

        Vector2 toPlayer = (Vector2)(target.position - transform.position);
        float   dist     = toPlayer.magnitude;

        // choose attack based on range & cooldowns (alternating pattern)
        if (nextAttack == 1 && dist <= atk1Range && cd1 <= 0f && !busy)
        {
            cd1 = atk1Cooldown; // Set cooldown IMMEDIATELY to prevent spam
            StartCoroutine(DoAttack1());
            nextAttack = 2;   // next time, try attack 2
        }
        else if (nextAttack == 2 && dist >= atk2MinRange && dist <= atk2MaxRange && cd2 <= 0f && !busy)
        {
            cd2 = atk2Cooldown; // Set cooldown IMMEDIATELY to prevent spam
            StartCoroutine(DoAttack2());
            nextAttack = 1;   // next time, try attack 1
        }

        // ----- flying hover/orbit movement (no gravity) -----
        Vector2 desiredVel = Vector2.zero;

        if (dist < innerKeepOut)
        {
            // too close -> back off
            desiredVel = (-toPlayer).normalized * retreatSpeed;
        }
        else if (dist > outerLeash)
        {
            // too far -> move in
            desiredVel = toPlayer.normalized * approachSpeed;
        }
        else
        {
            // within band -> orbit with gentle radial correction toward preferredRadius
            Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized * Mathf.Sign(orbitDir); // perpendicular
            float radialErr = dist - preferredRadius;
            Vector2 radial  = (-Mathf.Sign(radialErr) * toPlayer.normalized)
                              * Mathf.Clamp01(Mathf.Abs(radialErr) / 2f) * 0.6f * maxSpeed;

            desiredVel = tangent * orbitSpeed + radial;
        }

        // smooth steer toward desired velocity and clamp top speed
        Vector2 v = Vector2.MoveTowards(rb ? rb.linearVelocity : Vector2.zero, desiredVel, accel * Time.deltaTime);
        if (v.magnitude > maxSpeed) v = v.normalized * maxSpeed;
        if (rb) rb.linearVelocity = v;

        // idle anim if nearly stopped
        if (animator && rb && rb.linearVelocity.sqrMagnitude < 0.01f)
            CrossfadeSafe(idleState);
    }

    IEnumerator DoAttack1()
    {
        busy = true; if (rb) rb.linearVelocity = Vector2.zero;
        
        // Capture the facing direction NOW (before the telegraph delay)
        float attackDir = Mathf.Sign(transform.localScale.x);
        
        CrossfadeSafe(atk1State);
        yield return new WaitForSeconds(atk1Telegraph);
        
        // Pass the captured direction to the attack
        PerformAttack1(attackDir);
        yield return new WaitForSeconds(0.15f);
        busy = false;
    }

    IEnumerator DoAttack2()
    {
        busy = true; if (rb) rb.linearVelocity = Vector2.zero;
        CrossfadeSafe(atk2State);
        yield return new WaitForSeconds(atk2Telegraph);
        PerformAttack2();
        yield return new WaitForSeconds(0.15f);
        busy = false;
    }

    // Melee hit
    public void PerformAttack1(float attackDirection)
    {
        Vector2 center;
        
        if (hitOrigin)
        {
            // If hitOrigin exists, it's a child transform that already flips with the boss
            // So just use its position directly without adding offset again
            center = hitOrigin.position;
        }
        else
        {
            // Fallback: use boss position + offset adjusted for the CAPTURED attack direction
            center = (Vector2)transform.position + new Vector2(atk1BoxOffset.x * attackDirection, atk1BoxOffset.y);
        }

        Collider2D hit = Physics2D.OverlapBox(center, atk1BoxSize, 0f, playerLayer);
        if (hit)
        {
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.Damage((int)atk1Damage);
                Debug.Log($"Boss dealt {atk1Damage} damage to {hit.name} (attack dir: {attackDirection})");
            }
        }
    }

    // Projectile (if prefab set) else melee fallback using same box
    public void PerformAttack2()
    {
        if (projectilePrefab && projectileSpawn)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
            Vector2 dir = (target ? ((Vector2)(target.position - projectileSpawn.position)).normalized
                                  : new Vector2(Mathf.Sign(transform.localScale.x), 0f));

            if (proj.TryGetComponent<Rigidbody2D>(out var prb))
                prb.linearVelocity = dir * projectileSpeed;

            var dmg = proj.GetComponent<SimpleProjectileDamage>();
            if (!dmg) dmg = proj.AddComponent<SimpleProjectileDamage>();
            dmg.damage = atk2Damage;
            dmg.playerLayer = playerLayer;
        }
        else
        {
            // fallback: reuse melee box with current facing direction
            float attackDir = Mathf.Sign(transform.localScale.x);
            PerformAttack1(attackDir);
        }
    }

    void CrossfadeSafe(string stateName)
    {
        if (!animator || string.IsNullOrEmpty(stateName)) return;
        animator.CrossFade(stateName, stateCrossfade, 0);
    }

    // editor visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center;
        
        if (hitOrigin)
        {
            center = hitOrigin.position;
        }
        else
        {
            float dir = Mathf.Sign(transform.localScale.x);
            center = (Vector2)transform.position + new Vector2(atk1BoxOffset.x * dir, atk1BoxOffset.y);
        }
        
        Gizmos.DrawWireCube(center, atk1BoxSize);
    }
}
