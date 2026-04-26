using System.Collections;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] int damage = 1;

    private Collider coll;

    private void Awake()
    {
        coll = GetComponent<Collider>();
        coll.isTrigger = true;
    }

    private void OnEnable()
    {
        coll.enabled = true;
    }

    private void OnDisable()
    {
        coll.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(damage);
            }
            else
            {
                Debug.LogWarning("TimerRing Instance null");
            }
        }
    }
}
