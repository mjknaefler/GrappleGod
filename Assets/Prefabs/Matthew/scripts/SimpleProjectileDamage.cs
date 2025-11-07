using UnityEngine;

public class SimpleProjectileDamage : MonoBehaviour
{
    public float damage = 8f;
    public LayerMask playerLayer;
    public float lifetime = 6f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        other.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
