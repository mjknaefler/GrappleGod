using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackFreeAim : MonoBehaviour
{
    [Header("Normal Attack")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float firePointRadius = 0.75f;
    [SerializeField] private bool rotateProjectileToAim = true;
    [SerializeField] private Transform visualRoot;
    [Tooltip("Animation speed multiplier for normal attack (1 = normal, 2 = 2x faster)")]
    [SerializeField] private float normalAttackAnimSpeed = 1.5f;

    [Header("Secondary Attack (Hold)")]
    [SerializeField] private GameObject secondaryProjectilePrefab;
    [Tooltip("How long to hold before triggering secondary attack (in seconds)")]
    [SerializeField] private float holdThreshold = 0.5f;
    [SerializeField] private float secondaryCooldown = 1.0f;
    [Tooltip("Optional: Different animation trigger for secondary attack")]
    [SerializeField] private string secondaryAttackTrigger = "SecondaryAttack";
    [Tooltip("Name of the charge animation state to freeze on first frame")]
    [SerializeField] private string chargeAnimationState = "ChargeAttack";
    [Tooltip("The normalized time (0-1) to freeze at during charge. 0 = first frame")]
    [SerializeField] private float chargeFrameTime = 0f;

    [Header("Focus System")]
    [Tooltip("Reference to the PlayerFocus component (auto-finds if not set)")]
    [SerializeField] private PlayerFocus playerFocus;

    private Animator anim;
    private float cooldownTimer = 0f;
    private float secondaryCooldownTimer = 0f;
    private Camera mainCam;
    private SpriteRenderer[] srs;
    private float baseScaleX = 1f;
    
    // Charge tracking
    private bool isHoldingAttack = false;
    private float holdTime = 0f;
    private bool isPlayingChargeAnimation = false;
    private bool isPlayingAttackAnimation = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
        if (visualRoot == null) visualRoot = transform;
        srs = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        baseScaleX = Mathf.Abs(visualRoot.localScale.x);
        
        // Auto-find PlayerFocus if not assigned
        if (playerFocus == null)
        {
            playerFocus = GetComponent<PlayerFocus>();
        }
        
        if (firePoint == null)
        {
            var fp = new GameObject("FirePoint");
            firePoint = fp.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(firePointRadius, 0f, 0f);
        }
    }

    private void Update()
    {
        UpdateFirePointToMouse();
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (secondaryCooldownTimer > 0f) secondaryCooldownTimer -= Time.deltaTime;
        
        // Ensure animation speed is normal when not holding AND not playing attack animation
        if (!isHoldingAttack && !isPlayingAttackAnimation && anim != null)
        {
            // Make sure animation isn't stuck frozen
            if (anim.speed != 1f)
            {
                anim.speed = 1f;
            }
            
            // Clear any lingering attack triggers (only if they exist)
            ResetTriggerSafe("Attack");
            if (!string.IsNullOrEmpty(secondaryAttackTrigger))
            {
                ResetTriggerSafe(secondaryAttackTrigger);
            }
        }
        
        // Track hold time for secondary attack
        if (isHoldingAttack)
        {
            holdTime += Time.deltaTime;
            
            // Only consume focus if we've passed the threshold (actually charging)
            if (holdTime >= holdThreshold && playerFocus != null)
            {
                bool hasFocus = playerFocus.ConsumeFocusOnCharge();
                if (!hasFocus)
                {
                    // Out of focus - auto-release
                    Debug.Log("Out of focus! Auto-releasing charge attack.");
                    HandleAttackRelease();
                    return;
                }
            }
            
            // Freeze animation on first frame while charging
            if (isPlayingChargeAnimation && anim != null)
            {
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                
                // Check if we're in the charge animation state
                if (stateInfo.IsName(chargeAnimationState))
                {
                    // Freeze at the specified frame (default: first frame)
                    anim.speed = 0f;
                    anim.Play(chargeAnimationState, 0, chargeFrameTime);
                }
            }
            
            // FALLBACK: Check if button was released using Mouse.current
            // This handles cases where InputValue.isPressed doesn't trigger on release
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
            {
                Debug.Log($"FALLBACK: Detected mouse release via Mouse.current - hold time: {holdTime:F2}s");
                HandleAttackRelease();
            }
        }
    }

    private void UpdateFirePointToMouse()
    {
        if (mainCam == null) return;
        Vector2 ms = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        float z = mainCam.orthographic ? 0f : Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 mw = mainCam.ScreenToWorldPoint(new Vector3(ms.x, ms.y, z));
        Vector2 pp = transform.position;
        Vector2 toMouse = (Vector2)mw - pp;
        if (toMouse.sqrMagnitude < 0.0001f) return;
        Vector2 dir = toMouse.normalized;
        Vector3 newPos = (Vector3)(pp + dir * firePointRadius);
        newPos.z = transform.position.z;
        firePoint.position = newPos;
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        UpdateFacing(dir.x);
    }

    private void UpdateFacing(float aimX)
    {
        if (Mathf.Abs(aimX) < 0.0001f) return;
        bool faceRight = aimX >= 0f;
        if (srs != null && srs.Length > 0)
        {
            for (int i = 0; i < srs.Length; i++) srs[i].flipX = !faceRight;
        }
        else
        {
            var s = visualRoot.localScale;
            s.x = baseScaleX * (faceRight ? 1f : -1f);
            visualRoot.localScale = s;
        }
    }

    public void OnAttack(InputValue value)
    {
        Debug.Log($"OnAttack called - isPressed: {value.isPressed}");
        
        // Handle button press
        if (value.isPressed)
        {
            // Button pressed - start tracking hold time
            Debug.Log("Attack button PRESSED - starting hold timer");
            isHoldingAttack = true;
            holdTime = 0f;
            
            // Start the charge animation and freeze it on first frame
            if (anim != null && !string.IsNullOrEmpty(chargeAnimationState))
            {
                anim.Play(chargeAnimationState, 0, chargeFrameTime);
                anim.speed = 0f; // Freeze immediately
                isPlayingChargeAnimation = true;
            }
        }
        else
        {
            // Button released via Input System callback
            Debug.Log($"Attack button RELEASED (via callback) - hold time was: {holdTime:F2}s");
            HandleAttackRelease();
        }
    }
    
    private void HandleAttackRelease()
    {
        Debug.Log($"[HandleAttackRelease] Called - isHoldingAttack: {isHoldingAttack}, holdTime: {holdTime:F3}");
        
        // Prevent double-firing by checking if we're still in attack state
        if (!isHoldingAttack)
        {
            Debug.LogWarning("[HandleAttackRelease] Attack already handled, SKIPPING to prevent double-fire");
            return;
        }
        
        // IMMEDIATELY mark as no longer holding to prevent any other calls
        isHoldingAttack = false;
        
        // Resume animation speed
        if (anim != null) anim.speed = 1f;
        isPlayingChargeAnimation = false;
        
        if (holdTime >= holdThreshold)
        {
            // Held long enough - fire secondary attack
            Debug.Log($"[HandleAttackRelease] Held {holdTime:F2}s >= {holdThreshold:F2}s threshold - Firing SECONDARY");
            FireSecondaryAttack();
        }
        else
        {
            // Quick tap - fire normal attack
            Debug.Log($"[HandleAttackRelease] Quick tap {holdTime:F2}s < {holdThreshold:F2}s threshold - Firing NORMAL");
            FireNormalAttack();
        }
        
        // Reset hold time
        holdTime = 0f;
    }
    
    private void FireNormalAttack()
    {
        if (cooldownTimer > 0f)
        {
            Debug.Log("Normal attack on cooldown");
            return;
        }
        
        Debug.Log("=== FIRING NORMAL ATTACK ===");
        
        // Mark that we're playing an attack animation
        isPlayingAttackAnimation = true;
        
        // Ensure animator is at normal speed and trigger the attack
        if (anim != null)
        {
            // Set animation speed for faster light attack
            anim.speed = normalAttackAnimSpeed;
            Debug.Log($"Set animation speed to {normalAttackAnimSpeed}x for light attack");
            
            // Only reset triggers that actually exist in the animator
            ResetTriggerSafe("Attack");
            if (!string.IsNullOrEmpty(secondaryAttackTrigger) && secondaryAttackTrigger != "Attack")
            {
                ResetTriggerSafe(secondaryAttackTrigger);
            }
            if (!string.IsNullOrEmpty(chargeAnimationState) && chargeAnimationState != "Attack")
            {
                ResetTriggerSafe(chargeAnimationState);
            }
            
            // Now set the attack trigger
            anim.SetTrigger("Attack");
            
            // Start coroutine to reset trigger and speed after animation
            StartCoroutine(ResetAttackTriggerAfterFrame());
        }
        
        if (projectilePrefab != null && firePoint != null)
        {
            Quaternion rot = rotateProjectileToAim ? firePoint.rotation : Quaternion.identity;
            Instantiate(projectilePrefab, firePoint.position, rot);
            Debug.Log("Spawned normal projectile");
        }
        
        cooldownTimer = attackCooldown;
    }
    
    // Safely reset a trigger only if it exists
    private void ResetTriggerSafe(string triggerName)
    {
        if (anim == null || string.IsNullOrEmpty(triggerName)) return;
        
        if (HasAnimatorParameter(triggerName))
        {
            anim.ResetTrigger(triggerName);
        }
    }
    
    private System.Collections.IEnumerator ResetAttackTriggerAfterFrame()
    {
        // Wait for the animator to consume the trigger
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            Debug.Log("Reset Attack trigger after frames");
        }
        
        // Wait a bit longer then reset animation speed back to normal
        yield return new WaitForSeconds(0.2f);
        
        if (anim != null && !isHoldingAttack)
        {
            anim.speed = 1f;
            Debug.Log("Reset animation speed back to 1.0");
        }
        
        // Mark that attack animation is finished
        isPlayingAttackAnimation = false;
    }
    
    private void FireSecondaryAttack()
    {
        if (secondaryCooldownTimer > 0f)
        {
            Debug.Log("Secondary attack on cooldown");
            return;
        }
        
        // Check if we have enough focus to launch the attack
        if (playerFocus != null)
        {
            bool canLaunch = playerFocus.ConsumeFocusOnLaunch();
            if (!canLaunch)
            {
                Debug.Log("Not enough focus to launch charge attack!");
                return;
            }
        }
        
        // Resume animation to play the rest of the charge animation
        if (anim != null)
        {
            anim.speed = 1f; // Resume animation playback
            
            // If using a different trigger for completion, set it
            if (!string.IsNullOrEmpty(secondaryAttackTrigger) && HasAnimatorParameter(secondaryAttackTrigger))
            {
                anim.SetTrigger(secondaryAttackTrigger);
            }
            // Otherwise the charge animation will continue playing from where it was frozen
        }
        
        if (secondaryProjectilePrefab != null && firePoint != null)
        {
            Quaternion rot = rotateProjectileToAim ? firePoint.rotation : Quaternion.identity;
            GameObject proj = Instantiate(secondaryProjectilePrefab, firePoint.position, rot);
            
            // Make sure projectile has velocity (in case it uses Rigidbody2D)
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = firePoint.right; // Direction based on firePoint rotation
                float projSpeed = 10f; // Default speed
                
                // Try to get speed from Projectile script
                Projectile projScript = proj.GetComponent<Projectile>();
                if (projScript != null)
                {
                    // Use reflection to get the speed value
                    var speedField = typeof(Projectile).GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (speedField != null)
                    {
                        projSpeed = (float)speedField.GetValue(projScript);
                    }
                }
                
                rb.linearVelocity = direction * projSpeed;
                Debug.Log($"Set secondary projectile velocity: {rb.linearVelocity}");
            }
            
            Debug.Log("Fired secondary attack!");
        }
        else if (projectilePrefab != null && firePoint != null)
        {
            // Fallback to normal projectile if secondary not set
            Quaternion rot = rotateProjectileToAim ? firePoint.rotation : Quaternion.identity;
            Instantiate(projectilePrefab, firePoint.position, rot);
            Debug.Log("Fired secondary attack (using normal projectile as fallback)");
        }
        
        secondaryCooldownTimer = secondaryCooldown;
    }
    
    private bool HasAnimatorParameter(string paramName)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}
