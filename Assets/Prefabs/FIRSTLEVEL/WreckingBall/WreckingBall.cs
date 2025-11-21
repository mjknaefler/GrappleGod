using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    [Header("Swing Settings")]
    [SerializeField] private bool useSwingMode = true; // true = pendulum, false = full rotation
    [SerializeField] private float swingAngle = 90f; // Max swing angle (each direction)
    [SerializeField] private float swingSpeed = 1f; // Speed of swing/rotation

    [Header("Damage")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float knockbackForce = 15f;

    [Header("References")]
    [SerializeField] private Transform ball; // The wrecking ball GameObject
    [SerializeField] private Transform chainParent; // Parent of chain links
    [SerializeField] private Transform rotatingArm; // The part that rotates (chain + ball parent)

    private float currentAngle = 0f;
    private bool swingingRight = true;
    private bool hasHitThisSwing = false;

    void Update()
    {
        if (useSwingMode)
        {
            // Pendulum swing back and forth
            SwingPendulum();
        }
        else
        {
            // Full 360 rotation
            RotateFull();
        }
    }

    void SwingPendulum()
    {
        // Swing back and forth like a pendulum
        if (swingingRight)
        {
            currentAngle += swingSpeed * Time.deltaTime * 60f;
            if (currentAngle >= swingAngle)
            {
                currentAngle = swingAngle;
                swingingRight = false;
                hasHitThisSwing = false; // Reset on direction change
            }
        }
        else
        {
            currentAngle -= swingSpeed * Time.deltaTime * 60f;
            if (currentAngle <= -swingAngle)
            {
                currentAngle = -swingAngle;
                swingingRight = true;
                hasHitThisSwing = false; // Reset on direction change
            }
        }

        // Apply rotation to RotatingArm only
        if (rotatingArm != null)
        {
            rotatingArm.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }

    void RotateFull()
    {
        // Continuous 360 rotation
        currentAngle += swingSpeed * Time.deltaTime * 60f;
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }

        // Apply rotation to RotatingArm only
        if (rotatingArm != null)
        {
            rotatingArm.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }

    // Gets called by the BallTriggerForwarder on the ball
    public void OnBallCollision(Collider2D other)
    {
        Debug.Log($"OnBallCollision called! Hit: {other.name}, Tag: {other.tag}, hasHitThisSwing: {hasHitThisSwing}");

        // Prevent multiple hits during same swing
        if (hasHitThisSwing)
        {
            Debug.Log("Already hit this swing, ignoring");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player confirmed! Applying damage and knockback...");
            hasHitThisSwing = true;

            // Deal damage
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Damage(damage);
                Debug.Log($"Wrecking ball dealt {damage} damage!");
            }
            else
            {
                Debug.LogWarning("Player has no Health component!");
            }

            // Apply knockback based on ball's movement direction
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null && ball != null)
            {
                // Calculate knockback direction from ball's velocity/position
                Vector2 knockbackDir;

                if (useSwingMode)
                {
                    // For pendulum: knock in direction of swing
                    float swingDirection = swingingRight ? 1f : -1f;
                    knockbackDir = new Vector2(swingDirection, 0.5f).normalized;
                    Debug.Log($"Swing mode: direction = {swingDirection}");
                }
                else
                {
                    // For rotation: knock away from pivot
                    knockbackDir = (other.transform.position - transform.position).normalized;
                    Debug.Log($"Rotation mode: knockback away from pivot");
                }

                // Apply strong knockback
                playerRb.linearVelocity = Vector2.zero; // Stop current movement
                playerRb.linearVelocity = knockbackDir * knockbackForce;

                Debug.Log($"Applied wrecking ball knockback: {knockbackDir * knockbackForce}");
            }
            else
            {
                if (playerRb == null) Debug.LogWarning("Player has no Rigidbody2D!");
                if (ball == null) Debug.LogWarning("Ball reference is null!");
            }
        }
        else
        {
            Debug.Log($"Hit object is not tagged as Player (tag is: {other.tag})");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Show swing arc
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

        // Use rotatingArm position if available, otherwise use main transform
        Vector3 center = rotatingArm != null ? rotatingArm.position : transform.position;

        if (useSwingMode)
        {
            // Draw swing arc
            float radius = Vector3.Distance(center, ball != null ? ball.position : center + Vector3.down * 3f);

            // Draw arc from -swingAngle to +swingAngle
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = Mathf.Lerp(-swingAngle, swingAngle, (float)i / segments) * Mathf.Deg2Rad;
                float angle2 = Mathf.Lerp(-swingAngle, swingAngle, (float)(i + 1) / segments) * Mathf.Deg2Rad;

                Vector3 pos1 = center + new Vector3(Mathf.Sin(angle1), -Mathf.Cos(angle1), 0) * radius;
                Vector3 pos2 = center + new Vector3(Mathf.Sin(angle2), -Mathf.Cos(angle2), 0) * radius;

                Gizmos.DrawLine(pos1, pos2);
            }
        }
        else
        {
            // Draw full circle
            if (ball != null)
            {
                float radius = Vector3.Distance(center, ball.position);
                Gizmos.DrawWireSphere(center, radius);
            }
        }
    }
}