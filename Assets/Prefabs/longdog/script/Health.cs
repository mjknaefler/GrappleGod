using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    public int Current { get; private set; }

    // Fire when health changes / reaches zero
    public event Action<int,int> OnHealthChanged;  // (current, max)
    public event Action OnDeath;

    private void Awake()
    {
        Current = maxHP;
        OnHealthChanged?.Invoke(Current, maxHP);
    }

    public void Damage(int amount)
    {
        if (Current <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHP);
        if (Current <= 0) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (Current <= 0) return; // optional: prevent healing after death
        Current = Mathf.Min(maxHP, Current + amount);
        OnHealthChanged?.Invoke(Current, maxHP);
    }

    public void ResetHP()
    {
        Current = maxHP;
        OnHealthChanged?.Invoke(Current, maxHP);
    }
}
