using UnityEngine;

public class BallTriggerForwarder : MonoBehaviour
{
    private WreckingBall parentScript;
    
    void Start()
    {
        // Get the WreckingBall script from parent hierarchy
        parentScript = GetComponentInParent<WreckingBall>();
        if (parentScript == null)
        {
            Debug.LogError("BallTriggerForwarder: No WreckingBall script found in parent hierarchy!");
        }
        else
        {
            Debug.Log("BallTriggerForwarder: Successfully found WreckingBall script!");
        }
    }
    
    // Use OnTriggerStay2D to continuously check while player is in contact
    void OnTriggerStay2D(Collider2D other)
    {
        if (parentScript != null)
        {
            parentScript.OnBallCollision(other);
        }
        else
        {
            Debug.LogWarning("BallTriggerForwarder: Parent script is null!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Ball trigger entered by: {other.name}, Tag: {other.tag}");
    }
}
