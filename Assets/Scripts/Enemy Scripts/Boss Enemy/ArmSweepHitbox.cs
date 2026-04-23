using System.Collections;
using UnityEngine;

public class ArmSweepHitbox : MonoBehaviour
{
    [SerializeField] int damage = 1;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(damage);
            }
        }
    }
}
