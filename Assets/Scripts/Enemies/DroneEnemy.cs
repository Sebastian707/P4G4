using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneEnemy : SimpleEnemy
{
    private enum DroneState { Idle, Chase, Shooting, PostShot }
    private DroneState _state = DroneState.Idle;

    [Header("Drone – References")]
    public Transform player;
    public GameObject hellOrbPrefab;
    public GameObject explosionPrefab;
    public Transform firePoint;

    private Rigidbody _rb;

    [Header("Drone – Flight")]
    public float hoverHeight = 2.5f;
    public float hoverSpring = 40f;
    public float hoverDamping = 6f;
    public float chaseSpeed = 6f;
    public float maxSpeed = 10f;

    [Header("Drone – Circling")]
    public float orbitRadius = 6f;
    public float orbitSpeed = 4f;

    [Header("Drone – Direction Drift")]
    public float directionChangeMinInterval = 3f;
    public float directionChangeMaxInterval = 8f;
    public float directionFlipChance = 0.6f;

    [Header("Drone – Shooting")]
    public float attackRange = 12f;
    public float attackCooldown = 4f;
    public float chargeUpDuration = 1.2f;
    public float orbSpreadAngle = 15f;

    [Header("Drone – Post Shot Pause")]
    public float postShotPauseMin = 0.4f;
    public float postShotPauseMax = 1.4f;

    private static readonly List<SimpleEnemy> _activeEnemies = new List<SimpleEnemy>();
    private int _slotIndex = 0;

    private float _attackTimer;
    private float _targetOrbitAngle = 0f;
    private float _strafeSign = 1f;
    private float _directionChangeTimer;

    protected new void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.linearDamping = 3f;
        _rb.angularDamping = 5f;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        RegisterSlot();

        _strafeSign = Random.value > 0.5f ? 1f : -1f;
        _attackTimer = Random.Range(attackCooldown * 0.3f, attackCooldown);
        ScheduleNextDirectionChange();

        StartCoroutine(IdleToChase());
    }

    private void OnDestroy()
    {
        UnregisterSlot();
    }

    private void RegisterSlot()
    {
        _activeEnemies.RemoveAll(e => e == null);

        _slotIndex = _activeEnemies.Count;
        _activeEnemies.Add(this);

        RebuildSlots();
    }

    private void UnregisterSlot()
    {
        _activeEnemies.Remove(this);
        RebuildSlots();
    }
    private static void RebuildSlots()
    {
        int count = _activeEnemies.Count;
        for (int i = 0; i < count; i++)
        {
            if (_activeEnemies[i] is DroneEnemy drone)
            {
                drone._slotIndex = i;

                drone._targetOrbitAngle = (360f / count) * i;
            }
        }
    }
    private void FixedUpdate()
    {
        ApplyHover();
        ClampSpeed();
    }

    private void Update()
    {
        if (player == null) return;

        FacePlayer();

        if (_state != DroneState.Chase) return;

        _attackTimer -= Time.deltaTime;

        _directionChangeTimer -= Time.deltaTime;
        if (_directionChangeTimer <= 0f)
        {
            if (Random.value < directionFlipChance)
                _strafeSign *= -1f;
            ScheduleNextDirectionChange();
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && _attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            TransitionTo(DroneState.Shooting);
        }
        else
        {
            ApplyOrbitMovement();
        }
    }

    private void FacePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
    }

    private void ApplyOrbitMovement()
    {
        int total = Mathf.Max(1, _activeEnemies.Count);

        float slotBaseAngle = (360f / total) * _slotIndex;

        float halfArc = (360f / total) * 0.45f;
        _targetOrbitAngle += _strafeSign * orbitSpeed * Time.deltaTime * 10f;

        float minAngle = slotBaseAngle - halfArc;
        float maxAngle = slotBaseAngle + halfArc;
        _targetOrbitAngle = ClampAngle(_targetOrbitAngle, minAngle, maxAngle);

        Vector3 targetOffset = new Vector3(
            Mathf.Sin(_targetOrbitAngle * Mathf.Deg2Rad) * orbitRadius,
            0f,
            Mathf.Cos(_targetOrbitAngle * Mathf.Deg2Rad) * orbitRadius
        );

        Vector3 targetPos = player.position + targetOffset;
        targetPos.y = transform.position.y;

        Vector3 toTarget = targetPos - transform.position;
        float dist = toTarget.magnitude;

        Vector3 force = toTarget.normalized * Mathf.Clamp(dist * chaseSpeed, 0f, chaseSpeed * 2f);
        _rb.AddForce(force, ForceMode.Acceleration);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        while (angle < min) angle += 360f;
        while (angle > min + 360f) angle -= 360f;

        float normMax = max;
        while (normMax < min) normMax += 360f;

        return Mathf.Clamp(angle, min, normMax);
    }

    private void ScheduleNextDirectionChange()
    {
        _directionChangeTimer = Random.Range(directionChangeMinInterval, directionChangeMaxInterval);
    }

    private void TransitionTo(DroneState next)
    {
        _state = next;
        if (_state == DroneState.Shooting)
            StartCoroutine(ShootRoutine());
    }

    private IEnumerator IdleToChase()
    {
        yield return new WaitForSeconds(dissolveSpawnDuration + 0.2f);
        TransitionTo(DroneState.Chase);
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(chargeUpDuration);

        if (player != null)
        {
            Vector3 dir = (player.position - GetFireOrigin()).normalized;
            FireOrb(dir);
        }

        _state = DroneState.PostShot;
        yield return new WaitForSeconds(Random.Range(postShotPauseMin, postShotPauseMax));

        if (Random.value < 0.4f)
            _strafeSign *= -1f;

        TransitionTo(DroneState.Chase);
    }

    public new void ApplyDamage(Weapon weapon, float amount)
    {
        currentHealth -= amount;
        GetComponent<BossBar>()?.OnBossDamaged();

        if (currentHealth <= 0f)
            Detonate(transform.position);
    }

    private void ApplyHover()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
            hoverHeight * 2f, ~0, QueryTriggerInteraction.Ignore))
        {
            float error = hoverHeight - hit.distance;
            float force = (error * hoverSpring) - (_rb.linearVelocity.y * hoverDamping);
            _rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
        }
        else
        {
            _rb.AddForce(Vector3.up * hoverSpring * 0.2f, ForceMode.Acceleration);
        }
    }

    private void ClampSpeed()
    {
        Vector3 flat = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        if (flat.magnitude > maxSpeed)
        {
            flat = flat.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(flat.x, _rb.linearVelocity.y, flat.z);
        }
    }

    private Vector3 GetFireOrigin() => firePoint != null ? firePoint.position : transform.position;

    private void FireOrb(Vector3 direction)
    {
        if (hellOrbPrefab == null) return;

        var orb = Instantiate(hellOrbPrefab, GetFireOrigin(), Quaternion.LookRotation(direction));
        if (orb.TryGetComponent<Projectile>(out var proj))
            proj.owner = gameObject;
    }

    private void Detonate(Vector3 point)
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, point, Quaternion.identity);
        Destroy(gameObject);
    }

    public void SetOrbitAngle(float degrees) => _targetOrbitAngle = degrees;
}