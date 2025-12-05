using UnityEngine;
using System.Collections;

public class PuffDaddyBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float phase2HealthThreshold = 0.5f; // 50% health
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float floatHeight = 2f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private Transform[] movePoints; // Patrol points across arena
    
    [Header("Attack Settings - Phase 1")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private GameObject musicalNoteProjectile;
    [SerializeField] private GameObject champagneBottlePrefab;
    
    [Header("Attack Settings - Phase 2")]
    [SerializeField] private float phase2AttackCooldown = 1.5f;
    [Tooltip("Oil droplet prefab for Oil Rain attack")]
    [SerializeField] private GameObject moneyRainPrefab; // Still named moneyRainPrefab for backwards compatibility
    [SerializeField] private GameObject soundwaveProjectile;
    [SerializeField] private int phase2ProjectileCount = 8; // More aggressive
    
    [Header("Special Attacks")]
    [SerializeField] private GameObject microphoneSlamEffect;
    [SerializeField] private float shockwaveRadius = 8f;
    [SerializeField] private int shockwaveDamage = 2;
    
    [Header("Arena Settings")]
    [SerializeField] private float aggroRadius = 15f;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float arenaRadius = 10f;
    [SerializeField] private RisingOilLevel risingOil; // Reference to rising oil system
    
    [Header("Audio")]
    [SerializeField] private AudioClip phase1Music;
    [SerializeField] private AudioClip phase2Music;
    [SerializeField] private AudioClip laughSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    // State
    private enum BossPhase { Phase1, Transition, Phase2 }
    private enum BossState { Idle, Moving, Attacking, SpecialAttack, Hurt, Dead }
    
    private BossPhase currentPhase = BossPhase.Phase1;
    private BossState currentState = BossState.Idle;
    private float currentHealth;
    private bool isAggro = false;
    private bool isInvulnerable = false;
    
    // Attack tracking
    private float attackTimer = 0f;
    private int attackPattern = 0;
    private Transform player;
    private Vector3 startPosition;
    private int currentMovePoint = 0;
    private float floatOffset = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        currentHealth = maxHealth;
        startPosition = transform.position;
        
        if (rb != null)
        {
            rb.gravityScale = 0f; // Boss floats
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (arenaCenter == null)
            arenaCenter = transform;
            
        // Play phase 1 music
        if (audioSource != null && phase1Music != null)
        {
            audioSource.clip = phase1Music;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        Debug.Log("💎 PUFF DADDY BOSS: Initialized. Waiting for player...");
    }

    private void Update()
    {
        if (currentState == BossState.Dead) return;
        
        // Check aggro
        if (!isAggro && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= aggroRadius)
            {
                isAggro = true;
                Debug.Log("🎤 PUFF DADDY: You can't stop... WON'T STOP!");
                PlaySound(laughSound);
            }
        }
        
        if (!isAggro) return;
        
        // Update attack timer
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
        
        // Floating animation (DISABLED - set to 0 to stop bobbing)
        // floatOffset += Time.deltaTime * floatSpeed;
        float bobHeight = 0f; // Was: Mathf.Sin(floatOffset) * floatHeight * 0.3f;
        
        // State machine
        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;
            case BossState.Moving:
                UpdateMovement(bobHeight);
                break;
            case BossState.Attacking:
                // Handled by coroutines
                break;
            case BossState.SpecialAttack:
                // Handled by coroutines
                break;
        }
        
        // Face player
        if (player != null && currentState != BossState.Hurt)
        {
            bool shouldFlip = player.position.x < transform.position.x;
            spriteRenderer.flipX = shouldFlip;
        }
    }

    private void UpdateIdle()
    {
        // Float in place (bobbing disabled)
        Vector3 targetPos = startPosition;
        // targetPos.y += Mathf.Sin(floatOffset) * floatHeight * 0.3f;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 2f);
        
        // Try to attack
        if (attackTimer <= 0f && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= arenaRadius)
            {
                ChooseAttack();
            }
            else
            {
                // Move closer to player
                currentState = BossState.Moving;
            }
        }
    }

    private void UpdateMovement(float bobHeight)
    {
        if (player == null)
        {
            currentState = BossState.Idle;
            return;
        }
        
        // Move toward player but maintain distance
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Optimal attack range
        float optimalRange = arenaRadius * 0.6f;
        
        if (distanceToPlayer > optimalRange + 1f)
        {
            // Move closer
            Vector3 targetPos = transform.position + (Vector3)directionToPlayer * moveSpeed * Time.deltaTime;
            targetPos.y += bobHeight;
            transform.position = targetPos;
        }
        else if (distanceToPlayer < optimalRange - 1f)
        {
            // Move away
            Vector3 targetPos = transform.position - (Vector3)directionToPlayer * moveSpeed * Time.deltaTime;
            targetPos.y += bobHeight;
            transform.position = targetPos;
        }
        else
        {
            // In range, attack
            currentState = BossState.Idle;
        }
    }

    private void ChooseAttack()
    {
        if (currentPhase == BossPhase.Phase1)
        {
            // Phase 1: Alternating pattern
            switch (attackPattern % 3)
            {
                case 0:
                    StartCoroutine(MusicalNoteBarrage());
                    break;
                case 1:
                    StartCoroutine(ChampagneBottleThrow());
                    break;
                case 2:
                    StartCoroutine(MicrophoneSlam());
                    break;
            }
            attackPattern++;
        }
        else if (currentPhase == BossPhase.Phase2)
        {
            // Phase 2: More aggressive, random attacks
            int randomAttack = Random.Range(0, 4);
            switch (randomAttack)
            {
                case 0:
                    StartCoroutine(MoneyRainAttack());
                    break;
                case 1:
                    StartCoroutine(SoundwaveBlast());
                    break;
                case 2:
                    StartCoroutine(MusicalNoteBarrage()); // Faster version
                    break;
                case 3:
                    StartCoroutine(MicrophoneSlam()); // Larger radius
                    break;
            }
        }
    }

    // ===== PHASE 1 ATTACKS =====
    
    private IEnumerator MusicalNoteBarrage()
    {
        currentState = BossState.Attacking;
        attackTimer = attackCooldown;
        
        Debug.Log("🎵 PUFF DADDY: BIG Musical Note!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("Attack1");
        
        yield return new WaitForSeconds(0.5f); // Longer wind-up for big attack
        
        if (player != null && musicalNoteProjectile != null)
        {
            // Shoot ONE BIG projectile
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            GameObject bigNote = Instantiate(musicalNoteProjectile, transform.position, Quaternion.Euler(0, 0, angle));
            
            // Make it BIG and POWERFUL
            bigNote.transform.localScale = Vector3.one * (currentPhase == BossPhase.Phase2 ? 3f : 2f);
            
            Rigidbody2D noteRb = bigNote.GetComponent<Rigidbody2D>();
            if (noteRb != null)
            {
                float speed = currentPhase == BossPhase.Phase2 ? 12f : 8f;
                noteRb.linearVelocity = direction * speed;
            }
            
            // Increase damage if projectile has Projectile component
            Projectile proj = bigNote.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetDamage(currentPhase == BossPhase.Phase2 ? 3 : 2);
            }
        }
        
        yield return new WaitForSeconds(0.5f); // Recovery
        currentState = BossState.Idle;
    }

    private IEnumerator ChampagneBottleThrow()
    {
        currentState = BossState.Attacking;
        attackTimer = attackCooldown;
        
        Debug.Log("🍾 PUFF DADDY: Champagne Bottle Spray!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("Attack2");
        
        yield return new WaitForSeconds(0.4f); // Wind-up
        
        if (champagneBottlePrefab != null)
        {
            // Throw bottles in random directions all around with WIDE SPREAD!
            int bottleCount = currentPhase == BossPhase.Phase2 ? 12 : 8;
            
            for (int i = 0; i < bottleCount; i++)
            {
                // Random angle for chaotic spread
                float angle = Random.Range(0f, 360f);
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
                
                // Larger random spawn offset for wider starting spread
                Vector2 spawnOffset = Random.insideUnitCircle * 1.5f;
                Vector3 spawnPos = transform.position + new Vector3(spawnOffset.x, spawnOffset.y + 1f, 0);
                
                GameObject bottle = Instantiate(champagneBottlePrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                
                // Disable Projectile's transform movement
                Projectile proj = bottle.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.usePhysicsOnly = true;
                }
                
                Rigidbody2D bottleRb = bottle.GetComponent<Rigidbody2D>();
                if (bottleRb != null)
                {
                    // Force dynamic body type
                    bottleRb.bodyType = RigidbodyType2D.Dynamic;
                    bottleRb.gravityScale = 0.6f; // Same gravity for all
                    
                    // Same speed and upward force for consistent spread
                    float speed = 8f;
                    float upwardForce = 8f;
                    
                    Vector2 velocity = direction * speed + Vector2.up * upwardForce;
                    
                    // Set velocity
                    bottleRb.linearVelocity = velocity;
                    
                    // Add random spin for variety
                    bottleRb.angularVelocity = Random.Range(-300f, 300f);
                }
                
                yield return new WaitForSeconds(0.08f); // Quick succession
            }
        }
        
        yield return new WaitForSeconds(0.4f); // Recovery
        currentState = BossState.Idle;
    }

    private IEnumerator MicrophoneSlam()
    {
        currentState = BossState.SpecialAttack;
        attackTimer = attackCooldown * 1.5f; // Longer cooldown for special
        
        Debug.Log("🎤 PUFF DADDY: Microphone Slam!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("SpecialAttack");
        
        yield return new WaitForSeconds(0.5f); // Telegraph
        
        // Slam ground creating shockwave effect
        // The GroundSlamEffect prefab handles damage detection automatically
        if (microphoneSlamEffect != null)
        {
            GameObject slamEffect = Instantiate(microphoneSlamEffect, transform.position, Quaternion.identity);
            
            // Configure the effect based on current phase
            GroundSlamEffect effectScript = slamEffect.GetComponent<GroundSlamEffect>();
            if (effectScript != null && currentPhase == BossPhase.Phase2)
            {
                // Phase 2: Larger radius
                // You can access and modify the effect's maxRadius if needed
                Debug.Log("🔥 PHASE 2 SUPER SLAM with larger radius!");
            }
        }
        
        // Camera shake (optional - you'd need a camera shake script)
        // CameraShake.Instance?.Shake(0.5f, 0.3f);
        
        yield return new WaitForSeconds(0.8f); // Recovery
        currentState = BossState.Idle;
    }

    // ===== PHASE 2 ATTACKS =====
    
    private IEnumerator MoneyRainAttack()
    {
        currentState = BossState.Attacking;
        attackTimer = phase2AttackCooldown;
        
        Debug.Log("�️ PUFF DADDY: OIL RAIN!");
        PlaySound(laughSound);
        
        if (anim != null)
            anim.SetTrigger("Attack3");
        
        yield return new WaitForSeconds(0.3f);
        
        // Rain oil droplets from above in a wide spread
        if (moneyRainPrefab != null && player != null)
        {
            int dropletCount = 20; // More droplets for oil rain effect
            float rainWidth = 10f; // Wide area
            float rainHeight = 10f; // Height above player
            
            for (int i = 0; i < dropletCount; i++)
            {
                // Spawn droplets in a wide area above the player
                Vector3 spawnPos = player.position + new Vector3(
                    Random.Range(-rainWidth, rainWidth), 
                    rainHeight + Random.Range(0f, 2f), // Slight height variation
                    0f
                );
                
                GameObject oil = Instantiate(moneyRainPrefab, spawnPos, Quaternion.identity);
                
                // Make oil droplets smaller
                oil.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);
                
                // Disable Projectile's transform movement for physics-based falling
                Projectile proj = oil.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.usePhysicsOnly = true;
                    Debug.Log($"🛢️ Oil droplet spawned at {spawnPos}, usePhysicsOnly={proj.usePhysicsOnly}");
                }
                else
                {
                    Debug.LogWarning("🛢️ Oil prefab is missing Projectile component!");
                }
                
                Rigidbody2D oilRb = oil.GetComponent<Rigidbody2D>();
                if (oilRb != null)
                {
                    oilRb.bodyType = RigidbodyType2D.Dynamic;
                    oilRb.gravityScale = Random.Range(1.5f, 2.5f); // Varied fall speeds
                    
                    // Slight horizontal drift
                    oilRb.linearVelocity = new Vector2(Random.Range(-1f, 1f), 0f);
                    
                    Debug.Log($"🛢️ Oil RB2D velocity set to: {oilRb.linearVelocity}, gravity: {oilRb.gravityScale}");
                }
                else
                {
                    Debug.LogWarning("🛢️ Oil prefab is missing Rigidbody2D component!");
                }
                
                yield return new WaitForSeconds(0.08f); // Quick succession
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        currentState = BossState.Idle;
    }

    private IEnumerator SoundwaveBlast()
    {
        currentState = BossState.Attacking;
        attackTimer = phase2AttackCooldown;
        
        Debug.Log("🔊 PUFF DADDY: Soundwave Blast!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("Attack1");
        
        yield return new WaitForSeconds(0.3f);
        
        // Fire projectiles in all directions
        if (soundwaveProjectile != null)
        {
            for (int i = 0; i < phase2ProjectileCount; i++)
            {
                float angle = (360f / phase2ProjectileCount) * i;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                
                GameObject wave = Instantiate(soundwaveProjectile, transform.position, Quaternion.Euler(0, 0, angle));
                
                Rigidbody2D waveRb = wave.GetComponent<Rigidbody2D>();
                if (waveRb != null)
                {
                    waveRb.linearVelocity = direction * 12f;
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        currentState = BossState.Idle;
    }

    // ===== DAMAGE & HEALTH =====
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentState == BossState.Dead) return;
        
        currentHealth -= damage;
        Debug.Log($"💎 PUFF DADDY took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        PlaySound(hurtSound);
        StartCoroutine(FlashRed());
        
        // Check for phase transition
        if (currentPhase == BossPhase.Phase1 && currentHealth <= maxHealth * phase2HealthThreshold)
        {
            StartCoroutine(TransitionToPhase2());
        }
        // Check for death
        else if (currentHealth <= 0)
        {
            StartCoroutine(Death());
        }
        else
        {
            // Brief hurt state
            StartCoroutine(HurtState());
        }
    }

    private IEnumerator HurtState()
    {
        BossState previousState = currentState;
        currentState = BossState.Hurt;
        
        if (anim != null)
            anim.SetTrigger("Hurt");
        
        yield return new WaitForSeconds(0.3f);
        
        currentState = previousState;
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }
    }

    private IEnumerator TransitionToPhase2()
    {
        currentPhase = BossPhase.Transition;
        currentState = BossState.SpecialAttack;
        isInvulnerable = true;
        
        Debug.Log("💎💎💎 PUFF DADDY: YOU THINK YOU CAN DEFEAT THE BAD BOY?!");
        PlaySound(laughSound);
        
        if (anim != null)
            anim.SetTrigger("PhaseTransition");
        
        // Change music
        if (audioSource != null && phase2Music != null)
        {
            audioSource.Stop();
            audioSource.clip = phase2Music;
            audioSource.Play();
        }
        
        // Visual effects (add particle effects, screen shake, etc.)
        yield return new WaitForSeconds(2f);
        
        currentPhase = BossPhase.Phase2;
        isInvulnerable = false;
        attackCooldown = phase2AttackCooldown;
        moveSpeed *= 1.3f; // Faster movement
        
        // Make oil rise faster in Phase 2!
        if (risingOil != null)
        {
            risingOil.IncreaseRiseSpeed(2f); // 2x faster rise speed
            Debug.Log("🛢️ The oil rises faster! Get to higher ground!");
        }
        
        Debug.Log("🔥 PHASE 2 ACTIVATED! Boss is now more aggressive!");
        
        currentState = BossState.Idle;
    }

    private IEnumerator Death()
    {
        currentState = BossState.Dead;
        isInvulnerable = true;
        
        Debug.Log("💀 PUFF DADDY: I'll be back... in the remix...");
        PlaySound(deathSound);
        
        if (anim != null)
            anim.SetTrigger("Death");
        
        // Stop music
        if (audioSource != null)
            audioSource.Stop();
        
        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        yield return new WaitForSeconds(3f);
        
        // Trigger victory
        Debug.Log("🎉 BOSS DEFEATED! Level Complete!");
        
        // You can trigger level completion here
        // LevelManager.Instance?.BossDefeated();
        
        Destroy(gameObject, 2f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ===== GIZMOS FOR DEBUGGING =====
    
    private void OnDrawGizmosSelected()
    {
        // Aggro radius
        Gizmos.color = isAggro ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        
        // Arena radius
        Vector3 center = arenaCenter != null ? arenaCenter.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, arenaRadius);
        
        // Shockwave radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
        
        // Move points
        if (movePoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in movePoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}
