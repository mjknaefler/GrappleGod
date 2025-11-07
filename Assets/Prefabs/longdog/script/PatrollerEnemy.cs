using UnityEngine;

public class EnemyPatrol2D : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("How fast the enemy moves horizontally.")]
    public float speed = 2f;
    [Tooltip("The starting waypoint (Transform).")]
    public Transform pointStart;
    [Tooltip("The ending waypoint (Transform).")]
    public Transform pointEnd;
    [Tooltip("Horizontal distance to the point required to trigger a turn.")]
    public float stopDistance = 0.5f; 

    [Header("Components")]
    private Rigidbody2D rb;
    private Transform currentTarget;

    // Called when the script instance is being loaded
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
             rb.freezeRotation = true; // Prevents unwanted rotation
        }

        // Determine which point is further away initially and set it as the target
        float distA = Vector2.Distance(transform.position, pointStart.position);
        float distB = Vector2.Distance(transform.position, pointEnd.position);
        
        currentTarget = (distA > distB) ? pointStart : pointEnd;
    }

    // Called once per frame
    void FixedUpdate()
    {
        if (currentTarget == null) return;

        // 1. Calculate the direction vector to the target
        // We only care about the X component for direction calculation
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        
        // 2. Apply movement velocity
        // We ensure movement is only horizontal by forcing the Y component of velocity
        // to be governed by external forces (like gravity or internal enemy behavior)
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // 3. Check for Turn Condition (X-axis only)
        // This is the CRITICAL FIX for airborne patrolling: we ignore the Y-axis position.
        
        float targetX = currentTarget.position.x;
        float currentX = transform.position.x;
        
        // Calculate the absolute horizontal distance
        float horizontalDistance = Mathf.Abs(targetX - currentX);
        
        // Determine the horizontal direction of movement
        float movementDirection = Mathf.Sign(rb.linearVelocity.x);

        if (horizontalDistance <= stopDistance)
        {
            // Check if we are moving towards the current target's X position
            if (Mathf.Sign(targetX - currentX) != movementDirection || horizontalDistance < 0.05f)
            {
                // Switch the target
                if (currentTarget == pointEnd)
                {
                    currentTarget = pointStart;
                }
                else
                {
                    currentTarget = pointEnd;
                }
                
                // Flip the sprite when changing direction
                Flip();
            }
        }
    }
    
    // Simple function to visually flip the enemy
    void Flip()
    {
        // A common way to flip a 2D sprite is to reverse the X scale
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    // Draw the stop distance and path in the editor
    private void OnDrawGizmos()
    {
        if (pointStart != null && pointEnd != null)
        {
            // Draw a line connecting the patrol points
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointStart.position, pointEnd.position);

            // Draw a sphere to visualize the stop distance area
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointStart.position, stopDistance);
            Gizmos.DrawWireSphere(pointEnd.position, stopDistance);
        }
    }
}