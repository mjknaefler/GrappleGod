using System.Collections;
using UnityEngine;

/// <summary>
/// Flying boss that hovers around the player and alternates between melee and ranged attacks.
/// Simplified version with clear behavior states and better debugging.
/// </summary>
public class Boss1_v2 : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float hoverDistance = 4.0f;        // How far from player to hover
    [SerializeField] float moveSpeed = 5.0f;            // How fast boss moves
    [SerializeField] float orbitSpeed = 2.0f;           // How fast boss circles player
    [SerializeField] int orbitDirection = 1;            // 1 = clockwise, -1 = counter-clockwise
    
    [Header("Combat")]
    [SerializeField] float meleeRange = 2.5f;           // Close attack range
    [SerializeField] float diveBombMinRange = 2.5f;     // Dive bomb minimum range
    [SerializeField] float diveBombMaxRange = 8.0f;     // Dive bomb maximum range
    [SerializeField] float meleeDamage = 12f;
    [SerializeField] float diveBombDamage = 15f;        // Dive bomb hits harder!
    [SerializeField] float attackCooldown = 2.0f;       // Time between ANY attacks
    [SerializeField] float telegraphTime = 0.3f;        // Warning before attack hits
    
    [Header("Dive Bomb Settings")]
    [SerializeField] float diveBombSpeed = 15f;         // How fast boss dives
    [SerializeField] float diveBombRadius = 1.5f;       // Hit radius around impact point
    [SerializeField] float diveBombRecoveryTime = 0.5f; // Time to fly back up
    
    [Header("Attack Hitboxes")]
    [SerializeField] Vector2 meleeHitboxSize = new Vector2(2f, 1.5f);
    [SerializeField] float meleeHitboxDistance = 1.0f;  // How far in front of boss
    
    [Header("References")]
    [SerializeField] Transform target;                   // Player - auto-finds if empty
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    
    [Header("Animation State Names")]
    [SerializeField] string idleAnimation = "Wooden Aarakocra Idle Animation";
    [SerializeField] string meleeAnimation = "Wooden Aarakocra Attack 1Animation";
    [SerializeField] string diveBombAnimation = "Wooden Aarakocra Attack 2 Animation";
    
    [Header("Debug")]
    [SerializeField] bool showDebugGizmos = true;
    [SerializeField] bool showDebugLogs = false;
    
    // State
    enum BossState { Moving, Attacking, DiveBombing }
    BossState currentState = BossState.Moving;
    float attackCooldownTimer = 0f;
    bool useDiveBomb = false;  // Alternates between melee and dive bomb
    Vector2 diveBombStartPosition;  // Where boss was before dive
    
    void Awake()
    {
        // Auto-find components
        if (!target) 
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
            
            // Auto-add DamageRelay if missing (helpful for setup)
            if (player && !player.GetComponent<DamageRelay>())
            {
                Debug.LogWarning($"Player {player.name} is missing DamageRelay component! Please add it manually in Unity Editor.");
            }
        }
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        
        // Setup physics for flying
        if (rb)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 5f;  // Smooth stopping
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Start with cooldown so boss doesn't attack immediately
        attackCooldownTimer = attackCooldown;
        
        if (showDebugLogs) Debug.Log("Boss1_v2 initialized");
    }
    
    void Update()
    {
        if (!target) return;
        
        // Always face the player
        FaceTarget();
        
        // Update cooldown
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
        
        // State machine
        switch (currentState)
        {
            case BossState.Moving:
                UpdateMovement();
                TryStartAttack();
                break;
                
            case BossState.Attacking:
                // Don't move while attacking
                if (rb) rb.linearVelocity = Vector2.zero;
                break;
                
            case BossState.DiveBombing:
                // Movement handled in dive bomb coroutine
                break;
        }
        
        // Update animation
        UpdateAnimation();
    }
    
    void FaceTarget()
    {
        // Flip sprite to face player
        Vector3 scale = transform.localScale;
        if (target.position.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x) * -1;  // Face right (sprite faces left by default)
        else
            scale.x = Mathf.Abs(scale.x);       // Face left
        transform.localScale = scale;
        
        // Lock rotation
        transform.rotation = Quaternion.identity;
    }
    
    void UpdateMovement()
    {
        if (!rb) return;
        
        Vector2 toPlayer = target.position - transform.position;
        float distance = toPlayer.magnitude;
        
        // Orbit around player at hover distance
        Vector2 desiredPosition = (Vector2)target.position - toPlayer.normalized * hoverDistance;
        
        // Add circular motion
        float angle = orbitSpeed * Time.deltaTime * orbitDirection;
        Vector2 offset = toPlayer.normalized * hoverDistance;
        Vector2 rotatedOffset = new Vector2(
            offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle),
            offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle)
        );
        desiredPosition = (Vector2)target.position - rotatedOffset;
        
        // Move toward desired position
        Vector2 moveDirection = (desiredPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = moveDirection * moveSpeed;
        
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Boss Moving - Distance: {distance:F2}, Cooldown: {attackCooldownTimer:F2}");
        }
    }
    
    void TryStartAttack()
    {
        if (attackCooldownTimer > 0) return;
        if (currentState != BossState.Moving) return;
        
        float distance = Vector2.Distance(transform.position, target.position);
        
        // Check if in melee range
        if (distance <= meleeRange && !useDiveBomb)
        {
            StartCoroutine(PerformMeleeAttack());
            useDiveBomb = true;  // Next time use dive bomb
        }
        // Check if in dive bomb range
        else if (distance >= diveBombMinRange && distance <= diveBombMaxRange && useDiveBomb)
        {
            StartCoroutine(PerformDiveBombAttack());
            useDiveBomb = false;  // Next time use melee
        }
        // If preferred attack is out of range, try the other one
        else if (distance <= meleeRange)
        {
            StartCoroutine(PerformMeleeAttack());
            useDiveBomb = true;
        }
        else if (distance >= diveBombMinRange && distance <= diveBombMaxRange)
        {
            StartCoroutine(PerformDiveBombAttack());
            useDiveBomb = false;
        }
    }
    
    IEnumerator PerformMeleeAttack()
    {
        currentState = BossState.Attacking;
        attackCooldownTimer = attackCooldown;
        
        // Capture direction NOW
        float attackDirection = Mathf.Sign(transform.localScale.x);
        
        if (showDebugLogs) Debug.Log($"<color=red>Melee Attack Started!</color> Direction: {attackDirection}");
        
        // Play melee animation
        if (animator)
        {
            animator.CrossFade(meleeAnimation, 0.1f);
        }
        
        // Telegraph
        yield return new WaitForSeconds(telegraphTime);
        
        // Perform hit check
        Vector2 hitboxCenter = (Vector2)transform.position + Vector2.right * (meleeHitboxDistance * -attackDirection);
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, meleeHitboxSize, 0f);
        
        if (showDebugLogs)
        {
            Debug.Log($"Melee hitbox at {hitboxCenter}, found {hits.Length} colliders");
        }
        
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (showDebugLogs) Debug.Log($"<color=yellow>Found player collider: {hit.name}</color>");
                
                // Try DamageRelay component first
                var damageRelay = hit.GetComponent<DamageRelay>();
                if (damageRelay)
                {
                    damageRelay.ApplyDamage(meleeDamage);
                    if (showDebugLogs) Debug.Log($"<color=green>✓ Hit player with melee via DamageRelay! Damage: {meleeDamage}</color>");
                    break;
                }
                
                // Try Health component directly
                var health = hit.GetComponent<Health>();
                if (health)
                {
                    health.Damage((int)meleeDamage);
                    if (showDebugLogs) Debug.Log($"<color=green>✓ Hit player with melee via Health! Damage: {meleeDamage}</color>");
                    break;
                }
                
                // Last resort - SendMessage
                hit.SendMessage("ApplyDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
                Debug.LogWarning($"<color=orange>Using SendMessage fallback. Add DamageRelay or Health component to {hit.name}!</color>");
                break;
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        currentState = BossState.Moving;
    }
    
    IEnumerator PerformDiveBombAttack()
    {
        currentState = BossState.DiveBombing;
        attackCooldownTimer = attackCooldown;
        
        if (showDebugLogs) Debug.Log($"<color=cyan>DIVE BOMB ATTACK!</color>");
        
        // Save starting position to return to
        diveBombStartPosition = transform.position;
        
        // Play dive bomb animation
        if (animator)
        {
            animator.CrossFade(diveBombAnimation, 0.1f);
        }
        
        // Telegraph - hover in place
        yield return new WaitForSeconds(telegraphTime);
        
        // DIVE DOWN toward player!
        Vector2 targetPosition = target.position;
        float diveTime = 0f;
        float maxDiveTime = 1.5f;  // Max time to reach target
        
        while (diveTime < maxDiveTime)
        {
            if (!rb) break;
            
            Vector2 toTarget = targetPosition - (Vector2)transform.position;
            float distance = toTarget.magnitude;
            
            // Stop if we're close enough
            if (distance < 0.5f) break;
            
            // Dive toward target
            rb.linearVelocity = toTarget.normalized * diveBombSpeed;
            
            diveTime += Time.deltaTime;
            yield return null;
        }
        
        // IMPACT! Check for hits in radius
        if (rb) rb.linearVelocity = Vector2.zero;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, diveBombRadius);
        if (showDebugLogs) Debug.Log($"Dive bomb impact! Found {hits.Length} colliders in radius");
        
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (showDebugLogs) Debug.Log($"<color=yellow>Found player in dive bomb radius!</color>");
                
                // Try DamageRelay component first
                var damageRelay = hit.GetComponent<DamageRelay>();
                if (damageRelay)
                {
                    damageRelay.ApplyDamage(diveBombDamage);
                    if (showDebugLogs) Debug.Log($"<color=green>✓ Dive bomb hit via DamageRelay! Damage: {diveBombDamage}</color>");
                    break;
                }
                
                // Try Health component directly
                var health = hit.GetComponent<Health>();
                if (health)
                {
                    health.Damage((int)diveBombDamage);
                    if (showDebugLogs) Debug.Log($"<color=green>✓ Dive bomb hit via Health! Damage: {diveBombDamage}</color>");
                    break;
                }
                
                // Last resort - SendMessage
                hit.SendMessage("ApplyDamage", diveBombDamage, SendMessageOptions.DontRequireReceiver);
                Debug.LogWarning($"<color=orange>Using SendMessage fallback for dive bomb.</color>");
                break;
            }
        }
        
        // Recovery - fly back up to starting position
        yield return new WaitForSeconds(0.2f);
        
        float recoveryTime = 0f;
        while (recoveryTime < diveBombRecoveryTime)
        {
            if (!rb) break;
            
            Vector2 toStart = diveBombStartPosition - (Vector2)transform.position;
            float distance = toStart.magnitude;
            
            if (distance < 0.5f) break;
            
            rb.linearVelocity = toStart.normalized * moveSpeed;
            
            recoveryTime += Time.deltaTime;
            yield return null;
        }
        
        if (rb) rb.linearVelocity = Vector2.zero;
        currentState = BossState.Moving;
    }
    
    void UpdateAnimation()
    {
        if (!animator) return;
        
        // Animation is handled in attack coroutines now
        // Just play idle when moving
        if (currentState == BossState.Moving)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimation))
            {
                animator.CrossFade(idleAnimation, 0.1f);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // Draw hover distance circle
        Gizmos.color = Color.yellow;
        if (target)
        {
            Gizmos.DrawWireSphere(target.position, hoverDistance);
            
            // Draw melee range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, meleeRange);
            
            // Draw dive bomb ranges
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target.position, diveBombMinRange);
            Gizmos.DrawWireSphere(target.position, diveBombMaxRange);
        }
        
        // Draw melee hitbox
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        float dir = transform.localScale.x;
        Vector2 hitboxCenter = (Vector2)transform.position + Vector2.right * (meleeHitboxDistance * -Mathf.Sign(dir));
        Gizmos.DrawWireCube(hitboxCenter, meleeHitboxSize);
        
        // Draw dive bomb impact radius
        if (currentState == BossState.DiveBombing)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, diveBombRadius);
        }
    }
}
