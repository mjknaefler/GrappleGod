using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackFreeAim : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float firePointRadius = 0.75f;
    [SerializeField] private bool rotateProjectileToAim = true;
    [SerializeField] private Transform visualRoot;

    private Animator anim;
    private float cooldownTimer = 0f;
    private Camera mainCam;
    private SpriteRenderer[] srs;
    private float baseScaleX = 1f;

    // 🔊 NEW
    private PlayerAudio playerAudio;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
        if (visualRoot == null) visualRoot = transform;
        srs = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        baseScaleX = Mathf.Abs(visualRoot.localScale.x);

        // 🔊 FIND PlayerAudio (even if script is on a child)
        playerAudio = GetComponentInParent<PlayerAudio>();

        if (firePoint == null)
        {
            var fp = new GameObject("FirePoint");
            firePoint = fp.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(firePointRadius, 0f, 0f);
        }
    }

    private void Update()
    {
        UpdateFirePointToMouse();
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    private void UpdateFirePointToMouse()
    {
        if (mainCam == null) return;
        Vector2 ms = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        float z = mainCam.orthographic ? 0f : Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 mw = mainCam.ScreenToWorldPoint(new Vector3(ms.x, ms.y, z));
        Vector2 pp = transform.position;
        Vector2 toMouse = (Vector2)mw - pp;
        if (toMouse.sqrMagnitude < 0.0001f) return;
        Vector2 dir = toMouse.normalized;
        Vector3 newPos = (Vector3)(pp + dir * firePointRadius);
        newPos.z = transform.position.z;
        firePoint.position = newPos;
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        UpdateFacing(dir.x);
    }

    private void UpdateFacing(float aimX)
    {
        if (Mathf.Abs(aimX) < 0.0001f) return;
        bool faceRight = aimX >= 0f;
        if (srs != null && srs.Length > 0)
        {
            for (int i = 0; i < srs.Length; i++) srs[i].flipX = !faceRight;
        }
        else
        {
            var s = visualRoot.localScale;
            s.x = baseScaleX * (faceRight ? 1f : -1f);
            visualRoot.localScale = s;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (cooldownTimer > 0f) return;

        if (anim != null) anim.SetTrigger("Attack");

        // 🔊 PLAY SHOOT SOUND
        if (playerAudio != null)
            playerAudio.PlayShoot();

        if (projectilePrefab != null && firePoint != null)
        {
            Quaternion rot = rotateProjectileToAim ? firePoint.rotation : Quaternion.identity;
            Instantiate(projectilePrefab, firePoint.position, rot);
        }

        cooldownTimer = attackCooldown;
    }
}
