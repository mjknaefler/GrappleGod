using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Vector2 dir;

    public void Launch(Vector2 direction, float customSpeed = -1f)
    {
        dir = direction.normalized;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = dir * (customSpeed > 0f ? customSpeed : speed);
        Invoke(nameof(Kill), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit the player
        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<Health>();
            if (hp != null) hp.Damage(damage);
            Kill();
            return;
        }

        // Hit ground or anything else
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Kill();
        }
    }

    private void Kill() => Destroy(gameObject);
}
