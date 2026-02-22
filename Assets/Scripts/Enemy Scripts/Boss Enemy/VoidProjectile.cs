using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VoidProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 8f;

    private GameObject voidPoolPrefab;
    private VoidPoolSettings voidPoolSettings;
    private Action onExplosion;

    private float initTime;

    public void Initialize(Action onExplosion, GameObject voidPoolPrefab, VoidPoolSettings settings)
    {
        this.voidPoolPrefab = voidPoolPrefab;
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

        // Spawn a Void Pool and disable the projectile
        if (voidPoolPrefab != null)
        {
            GameObject voidPool = Instantiate(voidPoolPrefab, gameObject.transform.position, Quaternion.identity);
            if (voidPool.TryGetComponent<VoidPool>(out var pool))
            {
                pool.Initialize(voidPoolSettings);
            }
            else
            {
                Debug.LogError("Void Pool prefab lacks the Void Pool component.");
            }
        }
        else
        {
            Debug.LogError("Void Pool prefab is null");
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
