using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DamageTester : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private int healAmount = 1;

    void Awake()
    {
        // --- THIS IS THE FIXED LINE ---
        if (health == null) health = FindAnyObjectByType<Health>();
        
        Debug.Assert(health != null, "[DamageTester] No Health found in scene.");
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null || health == null) return;

        if (k.kKey.wasPressedThisFrame) health.Damage(damageAmount); // K = damage
        if (k.jKey.wasPressedThisFrame) health.Heal(healAmount);     // J = heal
        if (k.rKey.wasPressedThisFrame) health.ResetHP();            // R = reset
    }
}