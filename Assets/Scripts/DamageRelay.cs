using UnityEngine;

public class DamageRelay : MonoBehaviour
{
    [SerializeField] Health health;  // drag your existing Health component here

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
    }

    // Boss1 and SimpleProjectileDamage both call this via SendMessage
    public void ApplyDamage(float amount)
    {
        if (!health) return;
        int dmg = Mathf.CeilToInt(amount);
        health.Damage(dmg);
    }

    // overload in case something sends int
    public void ApplyDamage(int amount)
    {
        if (!health) return;
        health.Damage(amount);
    }
}
