using UnityEngine;

public class Grapple : MonoBehaviour
{
    Rigidbody2D rb;
    LineRenderer lr;
    DistanceJoint2D dj;

    [Header("Auto-Grapple Nodes")]
    public float autoGrappleMaxDist = 9f;          // how far you’re allowed to snap
    public GrappleTargetManager targetManager;     // assign in Inspector


    [Header("Grapple")]
    public LayerMask grappleLayer;
    public bool isGrappling;
    Vector2 anchorPoint;
    float ropeLength;

    [Header("Swing Control (while grappling)")]
    public float swingForce = 16f;      // A/D pumping strength (base)
    public float maxSwingSpeed = 16f;   // overall safety cap

    [Header("Rope Feel")]
    [Tooltip("Exponent controlling how sharp swing force drops near edges. 1 = linear, <1 = softer, >1 = sharper.")]
    public float swingEaseExponent = 0.8f;

    [Header("Swing Momentum Shaping")]
    [Tooltip("Max speed along the tangent of swing (prevents slamming into limits).")]
    public float tangentialSpeedCap = 12f;
    [Tooltip("Continuous damping along the tangent (rope friction).")]
    public float swingDamping = 1.2f;
    [Tooltip("Fades A/D pumping near edges (0 = no fade, 1 = strong fade).")]
    [Range(0f,1f)] public float edgePumpFade = 0.85f;
    [Tooltip("Extra tangential force from gravity when moving downward (helps ‘whoosh’).")]
    public float gravityAssist = 0.6f;

    [Header("Air Control (while NOT grappling)")]
    public float airControlForce = 6f;   // was 10f
    public float airMaxSpeed = 8f;
    public float airDrag = 0.1f;

    [Header("Angle Limit (prevent loops)")]
    [Tooltip("Allowed arc centered at -90° (bottom). 60° -> [-150°, -30°] (8 to 4 o'clock).")]
    public float allowedHalfWidthDeg = 60f;
    [Tooltip("Width (deg) of soft zone near each limit.")]
    public float softZoneDeg = 12f;
    [Tooltip("Gentle inward pull inside the soft zone (force along tangent).")]
    public float returnPull = 30f;
    [Tooltip("Extra damping at the exact limit for a soft turn-around.")]
    public float limitDamping = 0.85f;
    public float angleGuardEpsilon = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lr = GetComponent<LineRenderer>();
        dj = GetComponent<DistanceJoint2D>();
        lr.enabled = false;
        dj.enabled = false;
    }

    void Update()
    {
        // Right-click to attach (auto to manager's current circle)
        if (Input.GetMouseButtonDown(1))
        {
            GrappleTarget best = (targetManager != null) ? targetManager.current : null;

            if (best != null)
            {
                // Optional safety: don't snap from too far away
                if (Vector2.Distance(transform.position, best.transform.position) <= autoGrappleMaxDist)
                {
                    isGrappling = true;
                    anchorPoint = best.transform.position;

                    lr.enabled = true;
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, anchorPoint);

                    dj.enabled = true;
                    dj.autoConfigureDistance = false; // lock rope length for stable angles
                    ropeLength = Vector2.Distance(transform.position, anchorPoint);
                    dj.connectedAnchor = anchorPoint;
                    dj.distance = ropeLength;
                }
            }
            // else: no eligible circle visible → do nothing
        }

        // Space to "jump release" (cut rope, keep current velocity exactly)
        if (isGrappling && Input.GetKeyDown(KeyCode.Space))
        {
            JumpRelease();
        }

        // Release with right-click up as before
        if (Input.GetMouseButtonUp(1))
        {
            isGrappling = false;
            lr.enabled = false;
            dj.enabled = false;
        }

        if (isGrappling)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, anchorPoint);
        }
    }


    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or arrows

        if (isGrappling)
        {
            // Rope directions
            Vector2 toAnchor = (anchorPoint - (Vector2)transform.position).normalized;
            Vector2 tangentCW  = new Vector2(toAnchor.y, -toAnchor.x); // clockwise (our A/D mapping)
            Vector2 tangentCCW = new Vector2(-toAnchor.y, toAnchor.x); // counter-clockwise

            // Current angle and limits
            float angleDeg = GetAngleDeg();
            float minDeg = -90f - allowedHalfWidthDeg; // left (8 o’clock)
            float maxDeg = -90f + allowedHalfWidthDeg; // right (4 o’clock)

            // === Edge-aware input fade & center-weighting ===
            float tFromCenter = Mathf.Clamp01(Mathf.Abs(angleDeg + 90f) / allowedHalfWidthDeg);
            float ease = Mathf.Cos((angleDeg + 90f) * Mathf.Deg2Rad); // 1 at bottom, 0 at edges
            float easeWeight = Mathf.Clamp01(Mathf.Pow(Mathf.Abs(ease), swingEaseExponent));
            float centerWeight = Mathf.Lerp(1f, 1f - Mathf.SmoothStep(0f, 1f, tFromCenter), edgePumpFade);

            float fade = 1f;
            int edge = 0; // -1 = left/min, +1 = right/max
            if (angleDeg > maxDeg - softZoneDeg && angleDeg < maxDeg + 45f) { fade = Mathf.Clamp01((maxDeg - angleDeg)/Mathf.Max(0.0001f,softZoneDeg)); edge = +1; }
            else if (angleDeg < minDeg + softZoneDeg && angleDeg > minDeg - 45f) { fade = Mathf.Clamp01((angleDeg - minDeg)/Mathf.Max(0.0001f,softZoneDeg)); edge = -1; }

            bool pushingOutward = false;
            if (edge == +1) pushingOutward = (h < 0f);     // right edge: CCW is outward
            else if (edge == -1) pushingOutward = (h > 0f);// left edge : CW  is outward

            float appliedH = (pushingOutward ? h * fade : h) * centerWeight;

            // === A/D pumping with rope-style ease-in/ease-out ===
            if (Mathf.Abs(appliedH) > 0.01f)
            {
                float dynamicForce = swingForce * easeWeight;
                rb.AddForce(tangentCW * (appliedH * dynamicForce), ForceMode2D.Force);
            }

            // Gentle inward pull as you approach the limits
            if (edge != 0 && fade < 1f)
            {
                Vector2 tangentInward = (edge == +1) ? -tangentCW : tangentCW;
                float z = 1f - fade; // 0 -> 1 at the limit
                rb.AddForce(tangentInward * (returnPull * z), ForceMode2D.Force);
            }

            // === Gravity assist along the tangent (keeps the ‘whoosh’ alive) ===
            Vector2 g = Physics2D.gravity;
            float gAlongTangent = Vector2.Dot(g, tangentCCW);
            float s = Vector2.Dot(rb.linearVelocity, tangentCCW);
            if (gAlongTangent * s > 0f)
                rb.AddForce(tangentCCW * (gAlongTangent * gravityAssist * rb.mass), ForceMode2D.Force);

            // Tangential damping & cap
            rb.AddForce(-tangentCCW * (s * swingDamping), ForceMode2D.Force);
            if (Mathf.Abs(s) > tangentialSpeedCap)
            {
                float delta = (Mathf.Sign(s) * tangentialSpeedCap - s);
                rb.linearVelocity += tangentCCW * delta;  // adjust only tangent component
            }

            if (rb.linearVelocity.magnitude > maxSwingSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;

            // Smooth, no-bounce angle limits
            EnforceAngleLimitsNoBounce(minDeg, maxDeg);
        }
        else
        {
            // Light, speed-sensitive air control (A/D)
            if (Mathf.Abs(h) > 0.01f)
            {
                float vx = rb.linearVelocity.x;
                // Diminish acceleration as we approach airMaxSpeed
                float remaining = Mathf.Clamp01((airMaxSpeed - Mathf.Abs(vx)) / Mathf.Max(airMaxSpeed, 0.0001f));
                rb.AddForce(new Vector2(h * airControlForce * remaining, 0f), ForceMode2D.Force);
            }

            // Gentle horizontal damping + cap
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.x, -airMaxSpeed, airMaxSpeed), airDrag),
                rb.linearVelocity.y
            );
        }
    }

    // --- Jump Release: cut rope, KEEP current velocity unchanged ---
    void JumpRelease()
    {
        isGrappling = false;
        lr.enabled = false;
        dj.enabled = false;
        // rb.velocity remains unchanged by design
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
}
