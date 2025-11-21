using UnityEngine;

public class EnemyMeleeHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only hurt the player
        if (!other.CompareTag("Player")) return;

        Health hp = other.GetComponent<Health>();
        if (hp != null)
        {
            hp.Damage(damage);
        }
    }
}
