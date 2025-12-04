using UnityEngine;

/// <summary>
/// Forwards trigger collision events from the Electric Arc to the parent TeslaCoilHazard script.
/// Attach this to the ElectricArc GameObject that has the Box Collider 2D.
/// </summary>
public class ArcTriggerForwarder : MonoBehaviour
{
    [Tooltip("Auto-finds parent TeslaCoilHazard if not set")]
    [SerializeField] private TeslaCoilHazard parentHazard;
    
    void Awake()
    {
        // Auto-find parent if not assigned
        if (parentHazard == null)
        {
            parentHazard = GetComponentInParent<TeslaCoilHazard>();
            
            if (parentHazard == null)
            {
                Debug.LogError($"ArcTriggerForwarder on {gameObject.name} couldn't find TeslaCoilHazard parent!");
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && parentHazard != null)
        {
            Debug.Log($"Player entered Tesla Arc zone");
            parentHazard.OnPlayerEnterArc(other);
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && parentHazard != null)
        {
            parentHazard.OnPlayerStayInArc(other);
        }
    }
}
