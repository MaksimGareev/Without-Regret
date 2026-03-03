using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoidProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 8f;

    private ObjectPool voidPoolPool;
    private ObjectPool enemyPoolPool;
    private VoidPoolSettings voidPoolSettings;
    private Action onExplosion;

    private float initTime;

    public void Initialize(Action onExplosion, ObjectPool voidPoolPool, ObjectPool enemyPoolPool, VoidPoolSettings settings)
    {
        this.voidPoolPool = voidPoolPool;
        this.enemyPoolPool = enemyPoolPool;
        this.onExplosion = onExplosion;
        voidPoolSettings = settings;

        initTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Void projectile burst");

        if (collision.transform.CompareTag("Player"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(1);
            }
        }

        // Spawn a Void Pool from the pool and disable the projectile
        if (voidPoolPool != null)
        {
            GameObject voidPool = voidPoolPool.Get(gameObject.transform.position, Quaternion.identity);

            if (voidPool.TryGetComponent<VoidPool>(out var pool))
            {
                // Update void pool with necessary parameters and give it the enemy pool + self pool
                pool.Initialize(voidPoolSettings, enemyPoolPool, voidPoolPool);
            }
            else
            {
                Debug.LogError("Void Pool prefab lacks the Void Pool component.");
            }
        }
        else
        {
            Debug.LogError("Void Pool pool is null");
        }

        gameObject.SetActive(false);
        onExplosion?.Invoke();
    }

    private void Update()
    {
        // If the projectile has been alive for too long, disable it (just in case)
        if (Time.time > (initTime + lifetime))
        {
            gameObject.SetActive(false);
            onExplosion?.Invoke();
        }
    }
}
