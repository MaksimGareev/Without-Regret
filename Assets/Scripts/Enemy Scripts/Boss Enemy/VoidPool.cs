using UnityEngine;
using UnityEngine.AI;

public class VoidPool : MonoBehaviour
{
    private VoidPoolSettings settings;
    private readonly int amountOfRingsToSubtract = 1;
    private ObjectPool enemyPooler;

    // Damage
    private float enterTime = -1;

    private void OnDisable()
    {
        enterTime = -1;
    }

    public void Initialize(VoidPoolSettings settings, ObjectPool enemyPooler, ObjectPool selfPooler)
    {
        this.settings = settings;
        this.enemyPooler = enemyPooler;

        // Spawn enemies as soon as the pool appears
        SpawnEnemies(Random.Range(settings.minEnemiesToSpawn, settings.maxEnemiesToSpawn + 1));

        // Ensure this pool is returned after its lifetime
        if (TryGetComponent<Lifetime>(out var lifetime))
        {
            lifetime.Initialize(gameObject, settings.lifetime, selfPooler);
        }
        else
        {
            gameObject.AddComponent<Lifetime>().Initialize(gameObject, settings.lifetime, selfPooler);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered void pool.");
            enterTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has exited void pool.");
            enterTime = -1;
        }
    }

    private void SpawnEnemies(int amount)
    {
        if (enemyPooler == null)
        {
            Debug.LogError("Enemy pooler not found for VoidPool.", this);
            return;
        }

        // Ensure physics sees the current transform position/rotation before reading collider bounds
        Physics.SyncTransforms();

        if (!gameObject.TryGetComponent<Collider>(out var col))
        {
            Debug.LogError("VoidPool has no Collider to define spawn area.", this);
            return;
        }

        Bounds bounds = col.bounds;
        float sampleRadius = Mathf.Max(bounds.extents.x, bounds.extents.z, 1f);

        for (int i = 0; i < amount; i++)
        {
            // Generate a random point within the bounds (world space)
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            float sampleY = bounds.center.y; // use center Y for sampling height
            Vector3 randomPoint = new Vector3(randomX, sampleY, randomZ);

            // Try to find a point on the NavMesh near the random point
            Vector3 spawnPosition;
            bool foundNavPos = NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas);
            if (foundNavPos)
            {
                spawnPosition = hit.position;
            }
            else
            {
                // fallback spawn position
                spawnPosition = new Vector3(randomX, transform.position.y + 1f, randomZ);
                Debug.LogWarning($"NavMesh.SamplePosition failed inside VoidPool bounds; falling back to world position {spawnPosition}", this);
            }

            // Get an enemy from the pool
            GameObject newEnemy = enemyPooler.Get();

            // Place the enemy. If it has a NavMeshAgent, warp it to the sampled NavMesh position.
            if (newEnemy.TryGetComponent<NavMeshAgent>(out var agent))
            {
                // ensure agent is active/enabled before warping
                if (!agent.isActiveAndEnabled)
                    agent.enabled = true;

                agent.Warp(spawnPosition);
                newEnemy.transform.rotation = Quaternion.identity;
            }
            else
            {
                newEnemy.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            }

            // Add a lifetime component for the enemy so it gets returned after a delay
            if (newEnemy.TryGetComponent<Lifetime>(out var lifetime))
            {
                lifetime.Initialize(newEnemy, settings.enemyLifetime, enemyPooler);
            }
            else
            {
                newEnemy.AddComponent<Lifetime>().Initialize(newEnemy, settings.enemyLifetime, enemyPooler);
            }
        }
    }

    private void Update()
    {
        // Damage the player if they've been in the pool for too long
        if (enterTime > -1 && Time.time > (enterTime + settings.delayBeforeDamage))
        {
            Debug.Log("Player has been hurt by the void pool.");

            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(amountOfRingsToSubtract);
            }

            // Show tutorial about the void pool
            if (InteractionTutorialManager.Instance != null && !InteractionTutorialManager.Instance.HasSeenTutorial(InteractType.BossAttack))
            {
                if(InteractionTutorialUI.Instance != null)
                {
                    InteractionTutorialUI.Instance.ShowTutorial("Void pools damage you over time! Don't stay in them for too long to avoid damage!");
                }
                InteractionTutorialManager.Instance.MarkTutorialSeen(InteractType.BossAttack);
            }

            enterTime = Time.time;
        }
    }

}

[System.Serializable]
public struct VoidPoolSettings
{
    [Tooltip("How long the pool lasts before fading")]
    public float lifetime;
    [Tooltip("How long the player must be in the pool before they take damage")]
    public float delayBeforeDamage;
    [Tooltip("Defines the minimum of the range for how many enemies to spawn from the pool.")]
    public int minEnemiesToSpawn;
    [Tooltip("Defines the maximum of the range for how many enemies to spawn from the pool.")]
    public int maxEnemiesToSpawn;
    [Tooltip("How long the enemies spawned from the void pool will last")]
    public float enemyLifetime;

    public VoidPoolSettings(float lifetime, float delayBeforeDamage, int minEnemiesToSpawn, int maxEnemiesToSpawn, float enemyLifetime)
    {
        this.lifetime = lifetime;
        this.delayBeforeDamage = delayBeforeDamage;
        this.minEnemiesToSpawn = minEnemiesToSpawn;
        this.maxEnemiesToSpawn = maxEnemiesToSpawn;
        this.enemyLifetime = enemyLifetime;
    }
}
