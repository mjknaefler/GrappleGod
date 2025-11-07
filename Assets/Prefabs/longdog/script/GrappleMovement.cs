using UnityEngine;
using UnityEngine.InputSystem;

// NOTE: Ensure your project has the GrappleTarget and GrappleTargetManager classes defined elsewhere, 
// as they are external dependencies for the grapple functionality.

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(DistanceJoint2D))]
public class GrappleMovement : MonoBehaviour
{
    // =========================================================================================
    //                                  MOVEMENT & JUMP FIELDS
    // =========================================================================================
    [Header("1. Standard Movement")]
    [Tooltip("The horizontal speed when not grappling.")]
    [SerializeField] private float moveSpeed = 10f;
    [Tooltip("The vertical force applied for a jump.")]
    [SerializeField] private float jumpForce = 14f;
    [Tooltip("Tuning for faster falling.")]
    [SerializeField] private float fallMultiplier = 2.0f;
    [Tooltip("Tuning for shorter jumps when jump key is released early.")]
    [SerializeField] private float lowJumpMultiplier = 2.5f;
    [Tooltip("The maximum terminal velocity when falling.")]
    [SerializeField] private float maxFallSpeed = -20f;
    [Tooltip("Set to true if the sprite should flip based on movement direction.")]
    [SerializeField] private bool flipByMovement = false;

    [Header("2. Ground Check")]
    [Tooltip("Transform marking the center point for the ground check.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Radius of the circle check to detect ground.")]
    [SerializeField] private float groundCheckRadius = 0.15f;
    [Tooltip("LayerMask defining what counts as the floor.")]
    [SerializeField] private LayerMask groundMask;
    
    // =========================================================================================
    //                                  GRAPPLE FIELDS
    // =========================================================================================
    [Header("3. Grapple")]
    [Tooltip("LayerMask defining objects that can be grappled.")]
    public LayerMask grappleLayer;
    [Tooltip("Max distance the auto-grapple system will snap to a target node.")]
    public float autoGrappleMaxDist = 9f;
    [Tooltip("Reference to the external manager that tracks nearby grapple nodes.")]
    public GrappleTargetManager targetManager;     // Assign in Inspector

    [Header("4. Swing Control")]
    public float swingForce = 16f;
    public float maxSwingSpeed = 16f;
    public float swingEaseExponent = 0.8f;
    public float tangentialSpeedCap = 12f;
    public float swingDamping = 1.2f;
    [Range(0f,1f)] public float edgePumpFade = 0.85f;
    public float gravityAssist = 0.6f;

    [Header("5. Air Control (While NOT Grappling)")]
    public float airControlForce = 6f;
    public float airMaxSpeed = 8f;
    public float airDrag = 0.1f;

    [Header("6. Angle Limit")]
    public float allowedHalfWidthDeg = 60f;
    public float softZoneDeg = 12f;
    public float returnPull = 30f;
    public float limitDamping = 0.85f;
    public float angleGuardEpsilon = 0.5f;
    
    // =========================================================================================
    //                                  INPUT & STATE
    // =========================================================================================
    [Header("7. Input References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerInput playerInput;
    private SpriteRenderer sr;
    private LineRenderer lr;
    private DistanceJoint2D dj;

    // Input States
    private InputAction _move;
    private InputAction _jump;
    private Vector2 moveInput;
    private bool jumpHeld;

    // Shared States
    private bool isGrounded;
    public bool isGrappling { get; private set; }
    private Vector2 anchorPoint;
    private float ropeLength;
    
    // Momentum retention variable: tracks the current effective speed cap during the swing decay.
    private float maxMomentumCap; 

    // =========================================================================================
    //                                  UNITY LIFECYCLE
    // =========================================================================================

    private void Awake()
    {
        // Components from Movement
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;

        // Components from Grapple
        lr = GetComponent<LineRenderer>();
        dj = GetComponent<DistanceJoint2D>();
        lr.enabled = false;
        dj.enabled = false;
    }

    private void OnEnable()
    {
        // Input binding logic (from Movement.cs)
        if (moveAction != null && moveAction.action != null)
            _move = moveAction.action;
        else if (playerInput != null && playerInput.actions != null)
            _move = playerInput.actions["Move"];

        if (jumpAction != null && jumpAction.action != null)
            _jump = jumpAction.action;
        else if (playerInput != null && playerInput.actions != null)
            _jump = playerInput.actions["Jump"];

        if (_move != null)
        {
            _move.Enable();
            _move.performed += OnMovePerformed;
            _move.canceled += OnMoveCanceled;
        }

        if (_jump != null)
        {
            _jump.Enable();
            _jump.performed += OnJumpPerformed;
            _jump.canceled += OnJumpCanceled;
        }
    }

    private void OnDisable()
    {
        if (_move != null)
        {
            _move.performed -= OnMovePerformed;
            _move.canceled -= OnMoveCanceled;
            _move.Disable();
        }

        if (_jump != null)
        {
            _jump.performed -= OnJumpPerformed;
            _jump.canceled -= OnJumpCanceled;
            _jump.Disable();
        }
    }

    private void Update()
    {
        // --- Input State & Animation (Runs every frame) ---
        jumpHeld = _jump != null && _jump.IsPressed();

        // Animation updates (Simplified while grappling)
        if (!isGrappling)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("YVel", rb.linearVelocity.y);
            anim.SetBool("IsFalling", rb.linearVelocity.y < -0.1f && !isGrounded);

            if (flipByMovement)
            {
                if (rb.linearVelocity.x > 0.1f) sr.flipX = false;
                else if (rb.linearVelocity.x < -0.1f) sr.flipX = true;
            }
        }
        else
        {
            // Grappling Animation/Visuals
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, anchorPoint);
            anim.SetBool("IsGrounded", false);
            anim.SetBool("IsFalling", false);
        }

        // --- GRAPPLE INPUT (Uses legacy Input for simplicity based on original script) ---
        
        // Right-click to attach (auto to manager's current circle)
        if (Input.GetMouseButtonDown(1))
        {
            GrappleTarget best = (targetManager != null) ? targetManager.current : null;

            if (best != null)
            {
                if (Vector2.Distance(transform.position, best.transform.position) <= autoGrappleMaxDist)
                {
                    isGrappling = true;
                    anchorPoint = best.transform.position;

                    // === MOMENTUM PRESERVATION START ===
                    // 1. Calculate the tangent direction (perpendicular to the rope)
                    Vector2 initialToAnchor = (anchorPoint - (Vector2)transform.position).normalized;
                    Vector2 initialTangent = new Vector2(-initialToAnchor.y, initialToAnchor.x); 
                    
                    // 2. Calculate the speed along the tangent
                    float initialTangentialSpeed = Vector2.Dot(rb.linearVelocity, initialTangent);

                    // 3. Set the effective speed cap to whichever is higher: default cap or current speed.
                    maxMomentumCap = Mathf.Max(tangentialSpeedCap, Mathf.Abs(initialTangentialSpeed)); 
                    // === MOMENTUM PRESERVATION END ===
                    
                    lr.enabled = true;
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, anchorPoint);

                    dj.enabled = true;
                    dj.autoConfigureDistance = false;
                    ropeLength = Vector2.Distance(transform.position, anchorPoint);
                    dj.connectedAnchor = anchorPoint;
                    dj.distance = ropeLength;
                }
            }
        }

        // Release with right-click up
        if (Input.GetMouseButtonUp(1) && isGrappling)
        {
            JumpRelease();
        }
    }

    private void FixedUpdate()
    {
        // 1. Ground Check (Always run for animation/jump readiness)
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        float h = Input.GetAxisRaw("Horizontal"); // Used for swinging and air control

        if (isGrappling)
        {
            // =========================================================================
            //                         GRAPPLE SWING PHYSICS
            // =========================================================================

            // Rope directions
            Vector2 toAnchor = (anchorPoint - (Vector2)transform.position).normalized;
            Vector2 tangentCW  = new Vector2(toAnchor.y, -toAnchor.x); // clockwise (our A/D mapping)
            Vector2 tangentCCW = new Vector2(-toAnchor.y, toAnchor.x); // counter-clockwise

            // Current angle and limits
            float angleDeg = GetAngleDeg();
            float minDeg = -90f - allowedHalfWidthDeg;
            float maxDeg = -90f + allowedHalfWidthDeg;

            // Edge-aware input fade & center-weighting
            float tFromCenter = Mathf.Clamp01(Mathf.Abs(angleDeg + 90f) / allowedHalfWidthDeg);
            float ease = Mathf.Cos((angleDeg + 90f) * Mathf.Deg2Rad);
            float easeWeight = Mathf.Clamp01(Mathf.Pow(Mathf.Abs(ease), swingEaseExponent));
            float centerWeight = Mathf.Lerp(1f, 1f - Mathf.SmoothStep(0f, 1f, tFromCenter), edgePumpFade);

            float fade = 1f;
            int edge = 0;
            if (angleDeg > maxDeg - softZoneDeg && angleDeg < maxDeg + 45f) { fade = Mathf.Clamp01((maxDeg - angleDeg)/Mathf.Max(0.0001f,softZoneDeg)); edge = +1; }
            else if (angleDeg < minDeg + softZoneDeg && angleDeg > minDeg - 45f) { fade = Mathf.Clamp01((angleDeg - minDeg)/Mathf.Max(0.0001f,softZoneDeg)); edge = -1; }

            bool pushingOutward = (edge == +1 && h < 0f) || (edge == -1 && h > 0f);
            float appliedH = (pushingOutward ? h * fade : h) * centerWeight;

            // A/D pumping with rope-style ease-in/ease-out
            if (Mathf.Abs(appliedH) > 0.01f)
            {
                float dynamicForce = swingForce * easeWeight;
                rb.AddForce(tangentCW * (appliedH * dynamicForce), ForceMode2D.Force);
            }

            // Gentle inward pull as you approach the limits
            if (edge != 0 && fade < 1f)
            {
                Vector2 tangentInward = (edge == +1) ? -tangentCW : tangentCW;
                float z = 1f - fade;
                rb.AddForce(tangentInward * (returnPull * z), ForceMode2D.Force);
            }

            // Gravity assist along the tangent (keeps the ‘whoosh’ alive)
            Vector2 g = Physics2D.gravity;
            float gAlongTangent = Vector2.Dot(g, tangentCCW);
            float s = Vector2.Dot(rb.linearVelocity, tangentCCW);
            if (gAlongTangent * s > 0f)
                rb.AddForce(tangentCCW * (gAlongTangent * gravityAssist * rb.mass), ForceMode2D.Force);

            // Tangential damping & cap
            rb.AddForce(-tangentCCW * (s * swingDamping), ForceMode2D.Force);
            
            // === MOMENTUM PRESERVATION DECAY ===
            // 1. Use the momentum cap (maxMomentumCap) instead of the static tangentialSpeedCap
            if (Mathf.Abs(s) > maxMomentumCap)
            {
                float delta = (Mathf.Sign(s) * maxMomentumCap - s);
                rb.linearVelocity += tangentCCW * delta;
            }

            // 2. Gradually decay the momentum cap back to the default speed cap (tangentialSpeedCap)
            maxMomentumCap = Mathf.MoveTowards(maxMomentumCap, tangentialSpeedCap, swingDamping * Time.fixedDeltaTime); 
            // ===================================
            
            if (rb.linearVelocity.magnitude > maxSwingSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;

            // Smooth, no-bounce angle limits
            EnforceAngleLimitsNoBounce(minDeg, maxDeg);
        }
        else // isGrappling == false
        {
            // =========================================================================
            //                         STANDARD MOVEMENT PHYSICS
            // =========================================================================

            // Horizontal Movement (Air Control from Grapple.cs structure, but simplified)
            if (Mathf.Abs(h) > 0.01f)
            {
                float vx = rb.linearVelocity.x;
                // Diminish acceleration as we approach airMaxSpeed
                float remaining = Mathf.Clamp01((airMaxSpeed - Mathf.Abs(vx)) / Mathf.Max(airMaxSpeed, 0.0001f));
                rb.AddForce(new Vector2(h * airControlForce * remaining, 0f), ForceMode2D.Force);
            }

            // Standard Velocity Cap & Smoothing (Movement.cs logic for horizontal, adapted)
            Vector2 v = rb.linearVelocity;
            
            // Only apply standard horizontal movement if grounded. 
            // Otherwise, rely on air control forces applied above.
            if (isGrounded)
            {
                v.x = moveInput.normalized.x * moveSpeed;
            }
            else
            {
                 // Gentle horizontal damping + cap
                 v.x = Mathf.MoveTowards(v.x, Mathf.Clamp(v.x, -airMaxSpeed, airMaxSpeed), airDrag);
            }
            
            // Custom Fall Tuning (Movement.cs logic)
            if (v.y < -0.01f)
                v.y += Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            else if (v.y > 0.01f && !jumpHeld)
                v.y += Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

            if (v.y < maxFallSpeed) v.y = maxFallSpeed;
            rb.linearVelocity = v;
        }
    }

    // =========================================================================
    //                                  INPUT HANDLERS
    // =========================================================================
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.valueType == typeof(Vector2))
            moveInput = ctx.ReadValue<Vector2>();
        else
            moveInput = new Vector2(ctx.ReadValue<float>(), 0f);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (isGrappling)
        {
            // If jump is pressed while grappling, release the rope
            JumpRelease();
            // Add a small upward velocity boost for a jump-off effect
            var v = rb.linearVelocity;
            v.y = Mathf.Max(v.y, jumpForce * 0.5f); 
            rb.linearVelocity = v;
        }
        else if (isGrounded)
        {
            // Standard jump if grounded
            var v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;
            anim.SetTrigger("Jump");
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        jumpHeld = false;
    }

    // =========================================================================
    //                                  GRAPPLE HELPERS
    // =========================================================================
    
    // Jump Release: cut rope, KEEP current velocity unchanged
    void JumpRelease()
    {
        isGrappling = false;
        lr.enabled = false;
        dj.enabled = false;
        // rb.velocity remains unchanged by design
        // Reset the momentum cap immediately
        maxMomentumCap = tangentialSpeedCap;
    }

    void EnforceAngleLimitsNoBounce(float minDeg, float maxDeg)
    {
        float angleDeg = GetAngleDeg();
        if (angleDeg <= maxDeg + angleGuardEpsilon && angleDeg >= minDeg - angleGuardEpsilon)
            return;

        float clampedDeg = Mathf.Clamp(angleDeg, minDeg, maxDeg);
        float clampedRad = clampedDeg * Mathf.Deg2Rad;

        Vector2 boundaryDir = new Vector2(Mathf.Cos(clampedRad), Mathf.Sin(clampedRad));
        Vector2 targetPos = anchorPoint + boundaryDir * ropeLength;
        rb.position = targetPos;

        Vector2 tangentCCW = new Vector2(-Mathf.Sin(clampedRad), Mathf.Cos(clampedRad));
        float tangentialSpeed = Vector2.Dot(rb.linearVelocity, tangentCCW);

        bool atRight = Mathf.Abs(clampedDeg - maxDeg) < 0.0001f;
        bool atLeft  = Mathf.Abs(clampedDeg - minDeg) < 0.0001f;
        if (atRight && tangentialSpeed > 0f) tangentialSpeed = 0f;  // kill outward
        if (atLeft  && tangentialSpeed < 0f) tangentialSpeed = 0f;  // kill outward

        rb.linearVelocity = tangentCCW * (tangentialSpeed * limitDamping);
    }

    float GetAngleDeg()
    {
        Vector2 fromAnchor = (Vector2)transform.position - anchorPoint;
        float angleDeg = Mathf.Atan2(fromAnchor.y, fromAnchor.x) * Mathf.Rad2Deg;
        return NormalizeAngle180(angleDeg);
    }

    static float NormalizeAngle180(float angleDeg)
    {
        while (angleDeg <= -180f) angleDeg += 360f;
        while (angleDeg >   180f) angleDeg -= 360f;
        return angleDeg;
    }

    // =========================================================================
    //                                  EDITOR GIZMOS
    // =========================================================================

    private void OnDrawGizmosSelected()
    {
        // Ground Check Gizmo (from Movement.cs)
        if (groundCheck != null) 
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Grapple Rope/Anchor Gizmo
        if (isGrappling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPoint, 0.5f);
            Gizmos.DrawLine(transform.position, anchorPoint);
        }
    }
}