using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;

    [Header("Fall Tuning")]
    [SerializeField] private float fallMultiplier = 2.0f;    // faster fall
    [SerializeField] private float lowJumpMultiplier = 2.5f; // short hop if jump released
    [SerializeField] private float maxFallSpeed = -20f;      // terminal velocity (negative)

    [Header("Input (new Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerInput playerInput;
    private SpriteRenderer sr;

    private InputAction _move;
    private InputAction _jump;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        sr = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        // Resolve Move
        if (moveAction != null && moveAction.action != null)
            _move = moveAction.action;
        else if (playerInput != null && playerInput.actions != null)
            _move = playerInput.actions["Move"];

        // Resolve Jump
        if (jumpAction != null && jumpAction.action != null)
            _jump = jumpAction.action;
        else if (playerInput != null && playerInput.actions != null)
            _jump = playerInput.actions["Jump"];

        if (_move != null)
        {
            _move.Enable();
            _move.performed += OnMovePerformed;
            _move.canceled  += OnMoveCanceled;
        }

        if (_jump != null)
        {
            _jump.Enable();
            _jump.performed += OnJumpPerformed;
            _jump.canceled  += OnJumpCanceled; // for short-hop
        }
    }

    private void OnDisable()
    {
        if (_move != null)
        {
            _move.performed -= OnMovePerformed;
            _move.canceled  -= OnMoveCanceled;
            _move.Disable();
        }

        if (_jump != null)
        {
            _jump.performed -= OnJumpPerformed;
            _jump.canceled  -= OnJumpCanceled; // make sure we unsubscribe
            _jump.Disable();
        }
    }

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
        // Only jump if grounded
        if (!isGrounded) return;

        // Set upward velocity
        var v = rb.linearVelocity; // if needed, replace with rb.velocity
        v.y = jumpForce;
        rb.linearVelocity = v;

        // Animator (optional)
        anim.SetTrigger("Jump");
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        // Let Update keep jumpHeld live; this is just a safety clear
        jumpHeld = false;
    }

    private void FixedUpdate()
    {
        // Horizontal move
        Vector2 normalizedInput = moveInput.normalized;
        rb.linearVelocity = new Vector2(normalizedInput.x * moveSpeed, rb.linearVelocity.y);

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        // Better jump feel / fall tuning
        Vector2 v = rb.linearVelocity; // or rb.velocity
        if (v.y < -0.01f)
        {
            // Faster fall
            v.y += Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (v.y > 0.01f && !jumpHeld)
        {
            // Short hop when jump is released early
            v.y += Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Clamp terminal velocity
        if (v.y < maxFallSpeed) v.y = maxFallSpeed;

        rb.linearVelocity = v;
    }

    private void Update()
    {
        // Keep a live "jump held" read (works even if canceled missfires)
        jumpHeld = _jump != null && _jump.IsPressed();

        // Animator parameters
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("YVel", rb.linearVelocity.y);              // optional
        anim.SetBool("IsFalling", rb.linearVelocity.y < -0.1f && !isGrounded); // optional

        // Flip sprite to face movement
        if (rb.linearVelocity.x > 0.1f)
            sr.flipX = false;
        else if (rb.linearVelocity.x < -0.1f)
            sr.flipX = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
