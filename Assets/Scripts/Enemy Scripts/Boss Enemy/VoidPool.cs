using UnityEngine;

public class VoidPool : MonoBehaviour
{
    private float delayBeforeDamage = 1f;
    private readonly int amountOfRingsToSubtract = 1;

    // Damage
    private float enterTime = -1;

    // Lifetime
    private float lifetime;
    private float initTime;

    public void Initialize(float lifetime, float delayBeforeDamage)
    {
        this.lifetime = lifetime;
        this.delayBeforeDamage = delayBeforeDamage;
        
        initTime = Time.time;
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

    private void Update()
    {
        float currTime = Time.time;

        // Destroy the pool once lifetime has run out
        if (currTime > (initTime + lifetime))
        {
            Destroy(gameObject);
        }

        // Damage the player if they've been in the pool for too long
        if (enterTime > -1 && currTime > (enterTime + delayBeforeDamage))
        {
            Debug.Log("Player has been hurt by the void pool.");

            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(amountOfRingsToSubtract);
            }

            enterTime = Time.time;
        }
    }

}
