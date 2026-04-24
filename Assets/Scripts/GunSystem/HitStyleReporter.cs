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
    public void ApplyDamage(Weapon weapon, float amount)
    {
        // Find the weapon responsible by scanning active weapons and checking
        // which one fired most recently this frame.
        Weapon sourceWeapon = weapon;

        // Forward actual damage to the real enemy logic
        _enemy.ApplyDamage(weapon, amount);

        // Report the hit to the style system
        if (sourceWeapon != null && styleManager != null)
            styleManager.RegisterHit(sourceWeapon);
    }

    void OnEnemyDeath()
    {
        if (styleManager != null)
            styleManager.RegisterKill();
    }

}
