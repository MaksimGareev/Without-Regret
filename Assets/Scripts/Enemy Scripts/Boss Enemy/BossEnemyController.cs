using System;
using UnityEngine;

public class BossEnemyController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] int[] healthPerPart = new int[] { 3, 3, 3 };
    [SerializeField] float timeBetweenActions = 3f;
    [SerializeField, Tooltip("A delay before the boss starts acting")] float startDelay = 1.5f;

    [Header("Void Attack Settings")]
    [SerializeField, Min(0.1f)] float projectileSpeed = 5f;
    [SerializeField] VoidPoolSettings voidPoolSettings = new(5f, 1f, 1, 2, 6f);

    private Rigidbody voidProjectileRigidbody;

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject voidProjectileObject;
    [SerializeField] GameObject voidPoolPrefab;
    [SerializeField] Transform projectileSpawn;

    [Header("Debugging")]
    [SerializeField] bool showDebugLogs = false;

    private int currentPart = 1;
    private Vector3 projectileSpawnPoint;
    private Action[] actions;
    private float timeSinceLastAction = -3f;
    private bool actionInProgress = false;

    // Pools
    private ObjectPool enemyPooler;
    private ObjectPool voidPooler;

    private void Awake()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>().transform;
            Debug.LogWarning("Player reference for Boss Enemy is null, had to Find manually");
        }

        if (voidProjectileObject != null)
        {
            voidProjectileRigidbody = voidProjectileObject.GetComponent<Rigidbody>();
            voidProjectileObject.SetActive(false);
        }

        if (projectileSpawn != null)
        {
            projectileSpawnPoint = projectileSpawn.position;
        }

        if (enemyPrefab != null)
        {
            enemyPooler = new ObjectPool(enemyPrefab, 10, showDebugLogs, transform);
        }
        else
        {
            Debug.LogError("Enemy prefab for the void pool is missing");
        }

        if (voidPoolPrefab != null)
        {
            voidPooler = new ObjectPool(voidPoolPrefab, 3, showDebugLogs);
        }

        // set up the array of actions the boss can perform
        actions = new Action[] { VoidProjectile, ArmSweep, DropPillars };
    }

    private void Update()
    {
        // Do an action every (timeBetweenActions) seconds if an action is not currently in progress
        if (!actionInProgress && Time.time > (startDelay + timeSinceLastAction + timeBetweenActions))
        {
            actionInProgress = true;
            RandomAction();

            if (startDelay > 0) startDelay = 0;
        }
    }

    void RandomAction()
    {
        // Will pick an action at random once every action is fully implemented
        /*
        int choice = UnityEngine.Random.Range(0, actions.Length);
        actions[choice]();
        */

        VoidProjectile();
    }

    void EndAction()
    {
        if (showDebugLogs) Debug.Log("Action ended. Restarting timer");

        timeSinceLastAction = Time.time;
        actionInProgress = false;
    }

    void VoidProjectile()
    {
        if (voidProjectileObject == null)
        {
            Debug.LogError("Void Projectile Prefab reference is missing.");
            return;
        }

        if (showDebugLogs) Debug.Log("Performing Void Projectile action");

        // Initialize projectile to know which pools to use
        if (!voidProjectileObject.TryGetComponent<VoidProjectile>(out var voidProjectile))
        {
            Debug.LogError("VoidProjectile component missing on projectile prefab.");
            return;
        }

        voidProjectile.Initialize(EndAction, voidPooler, enemyPooler, voidPoolSettings); // End action when projectile hits something

        // Launch the void projectile from the spawnpoint toward the player's position
        voidProjectileObject.transform.SetPositionAndRotation(projectileSpawnPoint, Quaternion.identity);
        voidProjectileObject.SetActive(true);

        if (player != null)
        {
            Vector3 origin = projectileSpawnPoint;
            Vector3 target = player.position;

            Vector3 toTarget = target - origin;
            // compute time-to-target based on horizontal distance and projectileSpeed
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = toTargetXZ.magnitude;
            float time = Mathf.Clamp(horizontalDistance / projectileSpeed, 0.25f, 3f);

            // required initial velocity:
            Vector3 initialVelocity = toTarget / time - 0.5f * time * Physics.gravity;

            // apply velocity directly so the projectile follows physics and lands near target
            voidProjectileRigidbody.linearVelocity = initialVelocity;
            voidProjectileRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            Debug.LogError("Player reference is null.");
        }
    }

    void ArmSweep()
    {

    }
    
    void DropPillars()
    {

    }

    void Die()
    {
        Debug.Log("Boss' health has depleted");
        Destroy(gameObject); // Replace with death sequence later
    }

    public void TakeDamage(int value = 1)
    {
        // Take damage to the current health part
        healthPerPart[currentPart - 1] -= value;

        if (showDebugLogs) Debug.Log($"Boss took {value} damage. Current phase: {currentPart}, Current health: {healthPerPart[currentPart - 1]}");

        if (healthPerPart[currentPart - 1] <= 0)
        {
            if (currentPart >= healthPerPart.Length)
            {
                // Final part has been depleted
                Die();
            }
            else
            {
                // Transition to the next part
                currentPart++;

                if (showDebugLogs) Debug.Log("Transitioned to phase " + currentPart);
            }
        }
    }
}
