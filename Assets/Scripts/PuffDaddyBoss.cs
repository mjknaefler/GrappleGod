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
    [SerializeField] private GameObject moneyRainPrefab;
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
        
        Debug.Log("🎵 PUFF DADDY: Musical Note Barrage!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("Attack1");
        
        yield return new WaitForSeconds(0.3f); // Wind-up
        
        if (player != null && musicalNoteProjectile != null)
        {
            int noteCount = currentPhase == BossPhase.Phase2 ? 5 : 3;
            float spreadAngle = 30f;
            
            for (int i = 0; i < noteCount; i++)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                angle += (i - noteCount / 2) * spreadAngle;
                
                GameObject note = Instantiate(musicalNoteProjectile, transform.position, Quaternion.Euler(0, 0, angle));
                
                Rigidbody2D noteRb = note.GetComponent<Rigidbody2D>();
                if (noteRb != null)
                {
                    Vector2 fireDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
                    noteRb.linearVelocity = fireDirection * 10f;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return new WaitForSeconds(0.5f); // Recovery
        currentState = BossState.Idle;
    }

    private IEnumerator ChampagneBottleThrow()
    {
        currentState = BossState.Attacking;
        attackTimer = attackCooldown;
        
        Debug.Log("🍾 PUFF DADDY: Champagne Bottle Throw!");
        PlaySound(attackSound);
        
        if (anim != null)
            anim.SetTrigger("Attack2");
        
        yield return new WaitForSeconds(0.4f); // Wind-up
        
        if (player != null && champagneBottlePrefab != null)
        {
            // Lob bottle at player with arc
            Vector2 direction = (player.position - transform.position).normalized;
            GameObject bottle = Instantiate(champagneBottlePrefab, transform.position + Vector3.up, Quaternion.identity);
            
            Rigidbody2D bottleRb = bottle.GetComponent<Rigidbody2D>();
            if (bottleRb != null)
            {
                bottleRb.gravityScale = 1f;
                bottleRb.linearVelocity = direction * 8f + Vector2.up * 5f; // Arc trajectory
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
        
        Debug.Log("💰 PUFF DADDY: IT'S ALL ABOUT THE BENJAMINS!");
        PlaySound(laughSound);
        
        if (anim != null)
            anim.SetTrigger("Attack3");
        
        yield return new WaitForSeconds(0.5f);
        
        // Rain money projectiles from above
        if (moneyRainPrefab != null && player != null)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 spawnPos = player.position + new Vector3(Random.Range(-5f, 5f), 8f, 0f);
                GameObject money = Instantiate(moneyRainPrefab, spawnPos, Quaternion.identity);
                
                Rigidbody2D moneyRb = money.GetComponent<Rigidbody2D>();
                if (moneyRb != null)
                {
                    moneyRb.gravityScale = 1f;
                }
                
                yield return new WaitForSeconds(0.15f);
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
