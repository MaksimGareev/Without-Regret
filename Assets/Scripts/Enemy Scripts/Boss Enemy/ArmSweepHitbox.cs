using System.Collections;
using UnityEngine;

public class ArmSweepHitbox : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [Tooltip("The player won't take damage from the arm for this duration after getting hit once")]
    [SerializeField] float invincibilityDuration = 2f;

    private bool canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(damage);
                canDamage = false;
                StartCoroutine(Invincibility());
            }
        }
    }

    IEnumerator Invincibility()
    {
        canDamage = false;
        yield return new WaitForSeconds(invincibilityDuration);
        canDamage = true;
    }
}
