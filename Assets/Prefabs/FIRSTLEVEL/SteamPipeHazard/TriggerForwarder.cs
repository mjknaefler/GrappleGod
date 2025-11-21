using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    private SteamVentHazard parentScript;

    void Start()
    {
        parentScript = GetComponentInParent<SteamVentHazard>();
        if (parentScript == null)
        {
            Debug.LogError("TriggerForwarder: No SteamVentHazard found in parent!");
        }
    }

    // Keep checking while player is IN the zone
    void OnTriggerStay2D(Collider2D other)
    {
        if (parentScript != null)
        {
            parentScript.OnSteamCollision(other);
        }
    }
}