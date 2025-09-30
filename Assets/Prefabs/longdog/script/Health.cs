using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    public int Current { get; private set; }
    public System.Action OnDeath;

    private void Awake() => Current = maxHP;

    public void Damage(int amount)
    {
        if (Current <= 0) return;
        Current -= amount;
        if (Current <= 0) OnDeath?.Invoke();
    }
}
