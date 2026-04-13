using System.Collections.Generic;
using UnityEngine;

public class StyleComboManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponStyleConfig
    {
        [Tooltip("The Weapon component this config applies to.")]
        public Weapon weapon;

        [Tooltip("Base points awarded when this weapon has no recent history.")]
        public int basePoints = 30;

        [Tooltip("Absolute minimum points a hit can award regardless of history depth.")]
        public int minimumPoints = 5;
    }

    [Header("References")]
    public PointManager pointManager;

    [Header("Weapon Configs")]
    public List<WeaponStyleConfig> weaponConfigs = new List<WeaponStyleConfig>();

    [Header("History Window")]
    [Tooltip("How many recent hits the system looks back through when calculating multipliers. " +
             "Larger = slower recovery (more hits needed to push old entries out). Try 10–20.")]
    public int historyWindowSize = 12;

    [Header("Combo Streak")]
    [Tooltip("Bonus points per kill in a quick streak.")]
    public int streakBonusPerKill = 20;
    [Tooltip("Seconds between kills before the streak resets.")]
    public float streakWindow = 3f;

    [Header("Weapon Switch Bonus")]
    [Tooltip("Flat bonus on the first hit after switching to a different weapon.")]
    public int switchBonus = 15;
    [Tooltip("Seconds after switching during which the bonus applies.")]
    public float switchBonusWindow = 2f;

    // ── Internal state ───────────────────────────────────────────────

    // The shared ordered history of weapon hits, capped at historyWindowSize.
    // Index 0 = oldest, last index = most recent.
    private List<Weapon> _history = new List<Weapon>();

    private Weapon _lastUsedWeapon;
    private bool _switchBonusActive;
    private float _switchBonusTimer;
    private int _streakCount;
    private float _streakTimer;

    // ── Lifecycle ────────────────────────────────────────────────────

    void Awake()
    {
        if (pointManager == null)
            pointManager = FindFirstObjectByType<PointManager>();
    }

    void Update()
    {
        if (_switchBonusActive)
        {
            _switchBonusTimer -= Time.deltaTime;
            if (_switchBonusTimer <= 0f)
                _switchBonusActive = false;
        }

        if (_streakCount > 0)
        {
            _streakTimer -= Time.deltaTime;
            if (_streakTimer <= 0f)
            {
                _streakCount = 0;
                _streakTimer = 0f;
            }
        }
    }

    // ── Public API ───────────────────────────────────────────────────

    /// <summary>
    /// Call this when a weapon lands a hit on a damageable target.
    /// Called by HitStyleReporter — no changes to Weapon.cs needed.
    /// </summary>
    public void RegisterHit(Weapon sourceWeapon)
    {
        if (sourceWeapon == null) return;

        WeaponStyleConfig cfg = GetConfig(sourceWeapon);
        if (cfg == null) return;

        // Detect a weapon switch and activate the switch bonus
        if (_lastUsedWeapon != null && _lastUsedWeapon != sourceWeapon)
        {
            _switchBonusActive = true;
            _switchBonusTimer = switchBonusWindow;
            Debug.Log($"[Style] Switched {_lastUsedWeapon.name} → {sourceWeapon.name}");
        }
        _lastUsedWeapon = sourceWeapon;

        // Append to history, trim oldest entries to keep within the window
        _history.Add(sourceWeapon);
        while (_history.Count > historyWindowSize)
            _history.RemoveAt(0);

        // Count how many times this weapon appears in the current window
        int appearances = CountInWindow(sourceWeapon);

        // multiplier = 1 / (1 + appearances)
        // appearances=0 → ×1.0 (full), appearances=1 → ×0.5, appearances=2 → ×0.33, etc.
        // Note: the hit we just appended counts, so minimum appearances is 1 (= ×0.5 on
        // the very first use). If you want the first hit to always pay full basePoints,
        // change the formula to use (appearances - 1) instead.
        float multiplier = 1f / (1f + (appearances - 1));   // first hit = ×1.0
        int points = Mathf.RoundToInt(Mathf.Max(cfg.minimumPoints, cfg.basePoints * multiplier));

        // One-time switch bonus on first hit after swapping
        if (_switchBonusActive)
        {
            points += switchBonus;
            _switchBonusActive = false;
        }

        pointManager.AddPoints(points);

        Debug.Log($"[Style] {sourceWeapon.name} — appearances in window: {appearances}/{historyWindowSize}" +
                  $" → ×{multiplier:F2} → +{points} pts");
    }

    /// <summary>
    /// Call this when an enemy dies. Awards a streak bonus on top of the kill
    /// points already handled by SimpleEnemy.
    /// </summary>
    public void RegisterKill()
    {
        _streakCount++;
        _streakTimer = streakWindow;

        if (_streakCount > 1)
        {
            int bonus = streakBonusPerKill * (_streakCount - 1);
            pointManager.AddPoints(bonus);
            Debug.Log($"[Style] Kill streak ×{_streakCount} → +{bonus} bonus pts");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────

    int CountInWindow(Weapon w)
    {
        int count = 0;
        foreach (var entry in _history)
            if (entry == w) count++;
        return count;
    }

    WeaponStyleConfig GetConfig(Weapon w)
    {
        foreach (var cfg in weaponConfigs)
            if (cfg.weapon == w) return cfg;
        return null;
    }

    // ── Debug ────────────────────────────────────────────────────────

    [ContextMenu("Debug: Simulate Hit (first weapon)")]
    void DebugSimulateHit()
    {
        if (weaponConfigs.Count > 0)
            RegisterHit(weaponConfigs[0].weapon);
    }

    [ContextMenu("Debug: Simulate Kill")]
    void DebugSimulateKill() => RegisterKill();

    [ContextMenu("Debug: Print History")]
    void DebugPrintHistory()
    {
        var sb = new System.Text.StringBuilder("[Style] History (oldest→newest): ");
        foreach (var w in _history)
            sb.Append(w != null ? w.name : "null").Append(" ");
        Debug.Log(sb.ToString());
    }
}
