using UnityEngine;

[RequireComponent(typeof(SimpleEnemy))]
public class HitStyleReporter : MonoBehaviour, IDamageable
{
    [Tooltip("Leave blank to auto-find in scene.")]
    public StyleComboManager styleManager;

    private SimpleEnemy _enemy;

    void Awake()
    {
        _enemy = GetComponent<SimpleEnemy>();

        if (styleManager == null)
            styleManager = FindFirstObjectByType<StyleComboManager>();

        _enemy.OnDeath += OnEnemyDeath;
    }

    void OnDestroy()
    {
        if (_enemy != null)
            _enemy.OnDeath -= OnEnemyDeath;
    }

    /// <summary>
    /// This component is also IDamageable so it intercepts the weapon's raycast
    /// BEFORE SimpleEnemy.ApplyDamage is called. We forward the call, then report.
    /// 
    /// IMPORTANT: Because both HitStyleReporter and SimpleEnemy implement IDamageable,
    /// the Weapon's GetComponentInParent<IDamageable>() will find whichever is first.
    /// Make sure HitStyleReporter is ABOVE SimpleEnemy in the Inspector component list
    /// (drag it higher) so it is returned first. It forwards the call to SimpleEnemy.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        // Find the weapon responsible by scanning active weapons and checking
        // which one fired most recently this frame.
        Weapon sourceWeapon = FindShootingWeapon();

        // Forward actual damage to the real enemy logic
        _enemy.ApplyDamage(amount);

        // Report the hit to the style system
        if (sourceWeapon != null && styleManager != null)
            styleManager.RegisterHit(sourceWeapon);
    }

    void OnEnemyDeath()
    {
        if (styleManager != null)
            styleManager.RegisterKill();
    }

    /// <summary>
    /// Finds the Weapon that just fired by comparing timeSinceLastShot across
    /// all active weapons. The one closest to 0 fired this frame.
    /// This is zero-coupling — no changes to Weapon.cs at all.
    /// </summary>
    Weapon FindShootingWeapon()
    {
        // Find all active weapons in the scene
        Weapon[] weapons = FindObjectsByType<Weapon>(FindObjectsSortMode.None);

        Weapon closest = null;
        float bestTime = float.MaxValue;

        foreach (var w in weapons)
        {
            // Access timeSinceLastShot via reflection to stay zero-change on Weapon.cs
            // We use a cached FieldInfo for performance.
            float t = GetTimeSinceLastShot(w);
            if (t < bestTime)
            {
                bestTime = t;
                closest = w;
            }
        }

        // Only accept it if it fired very recently (within this frame + small buffer)
        return (bestTime < Time.deltaTime * 2f + 0.016f) ? closest : null;
    }

    // ── Reflection cache for zero-change weapon access ────────────────

    private static System.Reflection.FieldInfo _timeSinceLastShotField;

    static float GetTimeSinceLastShot(Weapon w)
    {
        if (_timeSinceLastShotField == null)
        {
            _timeSinceLastShotField = typeof(Weapon).GetField(
                "timeSinceLastShot",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
        }

        if (_timeSinceLastShotField == null) return float.MaxValue;
        return (float)_timeSinceLastShotField.GetValue(w);
    }
}
