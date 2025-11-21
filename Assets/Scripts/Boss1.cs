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
    [SerializeField] float atk1Range = 2.5f;  // Increased to cover more area
    [SerializeField] float atk1Damage = 12f;
    [SerializeField] float atk1Cooldown = 2.5f;
    [SerializeField] float atk1Telegraph = 0.25f;
    [SerializeField] Vector2 atk1BoxSize = new Vector2(1.6f, 1.0f);
    [SerializeField] Vector2 atk1BoxOffset = new Vector2(0.9f, 0.0f);

    [Header("Attack 2 (Projectile or Melee Fallback)")]
    [SerializeField] float atk2MinRange = 2.5f;  // Lowered to match atk1 max
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
        // Initialize cooldowns so boss doesn't attack immediately on spawn
        cd1 = atk1Cooldown;
        cd2 = atk2Cooldown;
        
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
        
        // Auto-set playerLayer if not configured
        if (playerLayer.value == 0)
        {
            Debug.LogWarning("Boss1: playerLayer not set! Auto-detecting from target...");
            if (target != null)
            {
                playerLayer = 1 << target.gameObject.layer;
                Debug.Log($"Boss1: Set playerLayer to layer '{LayerMask.LayerToName(target.gameObject.layer)}' (mask: {playerLayer.value})");
            }
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
        // INVERTED: sprite faces left by default, so flip the logic
        s.x = Mathf.Abs(s.x) * (target.position.x >= transform.position.x ? -1 : 1);
        transform.localScale = s;

        // hard-lock orientation every frame (prevents random spinning)
        transform.rotation = Quaternion.identity;
        if (rb) rb.angularVelocity = 0f;

        Vector2 toPlayer = (Vector2)(target.position - transform.position);
        float   dist     = toPlayer.magnitude;

        // DEBUG: Log every second to see what's happening
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Boss State - Distance: {dist:F2}, Busy: {busy}, NextAttack: {nextAttack}, CD1: {cd1:F2}, CD2: {cd2:F2}");
        }

        // ---- ATTACK SELECTION (strictly alternating) ----
        // Only attempt attacks if NOT busy
        if (!busy)
        {
            // Attack 1: Close range melee
            if (nextAttack == 1 && cd1 <= 0f && dist <= atk1Range)
            {
                Debug.Log($"<color=red>STARTING Attack 1!</color> Distance: {dist:F2}");
                StartCoroutine(DoAttack1());
                nextAttack = 2;
            }
            // Attack 2: Mid-range projectile
            else if (nextAttack == 2 && cd2 <= 0f && dist >= atk2MinRange && dist <= atk2MaxRange)
            {
                Debug.Log($"<color=blue>STARTING Attack 2!</color> Distance: {dist:F2}");
                StartCoroutine(DoAttack2());
                nextAttack = 1;
            }
            // Don't fall back to other attack - wait for correct alternation
        }

        // Stop movement when busy (attacking)
        if (busy) 
        { 
            if (rb) rb.linearVelocity = Vector2.zero; 
            return; 
        }

        // ---- MOVEMENT: simplified approach/orbit/retreat ----
        const float band = 0.5f;
        Vector2 desiredVel = Vector2.zero;

        if (dist > preferredRadius + band)
        {
            // APPROACH: move toward player
            desiredVel = toPlayer.normalized * approachSpeed;
        }
        else if (dist < innerKeepOut)
        {
            // TOO CLOSE: back off
            desiredVel = (-toPlayer).normalized * retreatSpeed;
        }
        else
        {
            // IN ORBIT RANGE: circle around player
            Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized * Mathf.Sign(orbitDir);
            desiredVel = tangent * orbitSpeed;
            
            // Add gentle push toward preferred radius
            float radialErr = dist - preferredRadius;
            if (Mathf.Abs(radialErr) > 0.3f)
            {
                desiredVel += (-Mathf.Sign(radialErr) * toPlayer.normalized) * (Mathf.Abs(radialErr) * 0.5f);
            }
        }

        // Smoothly accelerate toward desired velocity
        if (rb)
        {
            Vector2 v = Vector2.MoveTowards(rb.linearVelocity, desiredVel, accel * Time.deltaTime);
            // Clamp to max speed
            if (v.magnitude > maxSpeed) v = v.normalized * maxSpeed;
            rb.linearVelocity = v;
        }

        // idle anim if nearly stopped
        if (animator && rb && rb.linearVelocity.sqrMagnitude < 0.01f)
            CrossfadeSafe(idleState);
    }


    IEnumerator DoAttack1()
    {
        busy = true; 
        cd1 = atk1Cooldown; // Set cooldown at start
        if (rb) rb.linearVelocity = Vector2.zero;
        
        // FIX: CAPTURE direction NOW before telegraph
        float attackDir = Mathf.Sign(transform.localScale.x);
        
        CrossfadeSafe(atk1State);
        yield return new WaitForSeconds(atk1Telegraph);
        
        // Use captured direction
        PerformAttack1(attackDir);
        
        yield return new WaitForSeconds(0.15f);
        busy = false;
    }

    IEnumerator DoAttack2()
    {
        busy = true; 
        cd2 = atk2Cooldown; // Set cooldown at start
        if (rb) rb.linearVelocity = Vector2.zero;
        
        // FIX: CAPTURE direction NOW before telegraph
        float attackDir = Mathf.Sign(transform.localScale.x);
        
        CrossfadeSafe(atk2State);
        yield return new WaitForSeconds(atk2Telegraph);
        
        // Use captured direction
        PerformAttack2(attackDir);
        
        yield return new WaitForSeconds(0.15f);
        busy = false;
    }

    // Melee hit - now takes direction parameter
    public void PerformAttack1(float dir)
    {
        Vector2 origin = (hitOrigin ? (Vector2)hitOrigin.position : (Vector2)transform.position);
        Vector2 center = origin + new Vector2(atk1BoxOffset.x * dir, atk1BoxOffset.y);

        // Check what we're actually hitting
        Collider2D[] allHits = Physics2D.OverlapBoxAll(center, atk1BoxSize, 0f);
        Debug.Log($"<color=yellow>Attack1 PerformAttack!</color> Center: {center}, Direction: {dir}, Box Size: {atk1BoxSize}");
        Debug.Log($"  Found {allHits.Length} colliders at attack position:");
        foreach (var c in allHits)
        {
            Debug.Log($"    - {c.name} on layer {LayerMask.LayerToName(c.gameObject.layer)} (Layer #{c.gameObject.layer})");
        }

        // Now check with the layer mask
        Collider2D hit = Physics2D.OverlapBox(center, atk1BoxSize, 0f, playerLayer);
        Debug.Log($"  PlayerLayer mask value: {playerLayer.value} (checking layers: {GetLayerNames(playerLayer)})");
        Debug.Log($"  Result with layer filter: {(hit ? hit.name : "NONE")}");
        
        if (hit) 
        {
            Debug.Log($"<color=green>✓ HIT PLAYER: {hit.name}! Sending {atk1Damage} damage</color>");
            hit.SendMessage("ApplyDamage", atk1Damage, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.Log($"<color=red>✗ NO HIT - Layer mismatch or player not in range!</color>");
        }
    }

    string GetLayerNames(LayerMask mask)
    {
        string result = "";
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                if (result.Length > 0) result += ", ";
                result += LayerMask.LayerToName(i);
            }
        }
        return result;
    }

    // Projectile (if prefab set) else melee fallback using same box
    public void PerformAttack2(float dir)
    {
        if (projectilePrefab && projectileSpawn)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
            
            // FIX: Use captured direction if no target
            Vector2 shootDir = (target ? ((Vector2)(target.position - projectileSpawn.position)).normalized
                                  : new Vector2(dir, 0f));

            if (proj.TryGetComponent<Rigidbody2D>(out var prb))
                prb.linearVelocity = shootDir * projectileSpeed;

            var dmg = proj.GetComponent<SimpleProjectileDamage>();
            if (!dmg) dmg = proj.AddComponent<SimpleProjectileDamage>();
            dmg.damage = atk2Damage;
            dmg.playerLayer = playerLayer;
        }
        else
        {
            // fallback: reuse melee box with captured direction
            PerformAttack1(dir);
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
        Vector2 origin = (hitOrigin ? (Vector2)hitOrigin.position : (Vector2)transform.position);
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 center = origin + new Vector2(atk1BoxOffset.x * dir, atk1BoxOffset.y);
        Gizmos.DrawWireCube(center, atk1BoxSize);
    }
}
