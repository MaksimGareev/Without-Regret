using System;
using UnityEngine;

public class BossEnemyController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] int[] healthPerPart = new int[] { 3, 3, 3 };
    [SerializeField] float timeBetweenActions = 3f;
    [SerializeField, Tooltip("A delay before the boss starts acting")] float startDelay = 1.5f;

    [Header("Void Attack Settings")]
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float voidPoolLifetime = 5f;
    [SerializeField, Tooltip("How long the player must be in the pool for before they start taking damage")]
    float voidPoolDamageDelay = 1f;

    private Rigidbody voidProjectileRigidbody;

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] GameObject voidProjectilePrefab;
    [SerializeField] GameObject voidPoolPrefab;
    [SerializeField] Transform projectileSpawn;

    [Header("Debugging")]
    [SerializeField] bool showDebugLogs = false;

    private int currentPart = 1;
    private Vector3 projectileSpawnPoint;
    private Action[] actions;
    private float timeSinceLastAction = -3f;
    private bool actionInProgress = false;

    private void Awake()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>().transform;
            Debug.LogWarning("Player reference for Boss Enemy is null, had to Find manually");
        }

        if (voidProjectilePrefab != null)
        {
            voidProjectileRigidbody = voidProjectilePrefab.GetComponent<Rigidbody>();
            voidProjectilePrefab.SetActive(false);
        }

        if (projectileSpawn != null)
        {
            projectileSpawnPoint = projectileSpawn.position;
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
        int choice = UnityEngine.Random.Range(0, actions.Length - 1);
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
        if (voidProjectilePrefab == null)
        {
            Debug.LogError("Void Projectile Prefab reference is missing.");
            return;
        }

        if (showDebugLogs) Debug.Log("Performing Void Projectile action");

        voidProjectilePrefab.GetComponent<VoidProjectile>().Initialize(EndAction, voidPoolPrefab, voidPoolLifetime); // End action when projectile hits something

        // Launch the void projectile from the spawnpoint toward the player's position
        voidProjectilePrefab.transform.SetPositionAndRotation(projectileSpawnPoint, Quaternion.identity);
        voidProjectilePrefab.SetActive(true);

        if (player != null)
        {
            Vector3 origin = projectileSpawnPoint;
            Vector3 target = player.position;

            Vector3 toTarget = target - origin;
            // compute time-to-target based on horizontal distance and a speed factor (projectileSpeed used as speed baseline)
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = toTargetXZ.magnitude;
            float time = Mathf.Clamp(horizontalDistance / Mathf.Max(0.1f, projectileSpeed), 0.25f, 3f);

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
}
