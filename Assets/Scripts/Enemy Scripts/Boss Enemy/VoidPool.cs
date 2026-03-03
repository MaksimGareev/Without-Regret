using UnityEngine;

public class VoidPool : MonoBehaviour
{
    private VoidPoolSettings settings;
    private readonly int amountOfRingsToSubtract = 1;

    // Damage
    private float enterTime = -1;
    // Lifetime
    private float initTime;

    public void Initialize(VoidPoolSettings settings)
    {
        this.settings = settings;
        
        initTime = Time.time;
        // Spawn enemies as soon as the pool appears
        SpawnEnemies(Random.Range(settings.numEnemiesToSpawn[0], settings.numEnemiesToSpawn[1] + 1));
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
        for (int i = 0; i < amount; i++)
        {
            Bounds bounds = gameObject.GetComponent<Collider>().bounds;

            // Generate a random point within the bounds
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 randomPosition = new Vector3(randomX, transform.position.y + 1, randomZ);

            // Instantiate the enemy at the random position with no rotation
            Instantiate(settings.enemyPrefab, randomPosition, Quaternion.identity);
        }
    }

    private void Update()
    {
        float currTime = Time.time;

        // Destroy the pool once lifetime has run out
        if (currTime > (initTime + settings.lifetime))
        {
            Destroy(gameObject);
        }

        // Damage the player if they've been in the pool for too long
        if (enterTime > -1 && currTime > (enterTime + settings.delayBeforeDamage))
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
    [HideInInspector] public GameObject enemyPrefab;
    [Tooltip("How long the pool lasts before fading")]
    public float lifetime;
    [Tooltip("How long the player must be in the pool before they take damage")]
    public float delayBeforeDamage;
    [Tooltip("Defines the range for how many enemies to spawn from the pool. Should only be 2 values. Element 0 = min, Element 1 = max")]
    public int[] numEnemiesToSpawn;

    public VoidPoolSettings(float lifetime, float delayBeforeDamage, GameObject enemyPrefab, int[] numEnemiesToSpawn)
    {
        this.lifetime = lifetime;
        this.delayBeforeDamage = delayBeforeDamage;
        this.enemyPrefab = enemyPrefab;
        this.numEnemiesToSpawn = numEnemiesToSpawn;
    }
}
