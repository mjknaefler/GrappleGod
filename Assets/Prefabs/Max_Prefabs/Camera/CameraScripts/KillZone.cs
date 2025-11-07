using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pr = other.GetComponent<PlayerRespawn>();
        if (pr != null && Checkpoint.Current != null) pr.Respawn(Checkpoint.Current);
    }
}
