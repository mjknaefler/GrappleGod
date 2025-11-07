using UnityEngine;

public class GrappleTargetManager : MonoBehaviour
{
    [Header("Player & Camera")]
    public Transform player;   // assign in Inspector (or tag Player)
    public Camera cam;         // a ssign Main Camera (or leave blank)

    [Header("Selection Rules")]
    public float forwardBufferX = 1.5f;   // must be this far ahead on X
    public float maxSelectDist = 1f;      // max distance from player
    public float minVerticalAbove = 0.5f; // must be above player by this much
    public bool requireTopHalf = true;    // only show in top half of screen

    [Header("Debug (read-only)")]
    public GrappleTarget current;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!player) return;

        GrappleTarget best = null;
        float bestDistSqr = float.PositiveInfinity;
        float maxDistSqr = maxSelectDist * maxSelectDist;

        foreach (var t in GrappleTarget.All)
        {
            if (!t) continue;

            Vector3 tp = t.transform.position;
            
            if (tp.y < player.position.y + Mathf.Max(minVerticalAbove, t.verticalBiasMin)) continue;
            
            float distSqr = (tp - player.position).sqrMagnitude;
            if (distSqr > maxDistSqr) continue;

            if (requireTopHalf && cam)
            {
                Vector3 vp = cam.WorldToViewportPoint(tp);
                bool inTop = vp.z > 0f && vp.y >= 0.5f && vp.y <= 1.2f && vp.x >= -0.1f && vp.x <= 1.1f;
                if (!inTop) continue;
            }

            if (distSqr < bestDistSqr) { bestDistSqr = distSqr; best = t; }
        }

        current = best;
        foreach (var t in GrappleTarget.All)
            if (t) t.SetVisible(t == current); // exactly one visible
    }
}