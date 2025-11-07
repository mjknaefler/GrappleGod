using UnityEngine;

public class EnemyPatrol2D : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float speed = 2f;
    public Transform pointStart;
    public Transform pointEnd;
    public float stopDistance = 0.5f; // Distance to consider a point reached

    [Header("Components")]
    private Rigidbody2D rb;
    private Transform currentTarget;

    // Called when the script instance is being loaded
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Start by moving towards the end point
        currentTarget = pointEnd;
    }

    // Called once per frame
    void Update()
    {
        // 1. Calculate the direction to the target
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // 2. Move the enemy using Rigidbody2D
        rb.linearVelocity = direction * speed;

        // 3. Check if the enemy has reached the current target
        if (Vector2.Distance(transform.position, currentTarget.position) < stopDistance)
        {
            // Switch the target to the other point
            if (currentTarget == pointEnd)
            {
                currentTarget = pointStart;
            }
            else
            {
                currentTarget = pointEnd;
            }
            
            // Optional: Flip the sprite when changing direction
            Flip();
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