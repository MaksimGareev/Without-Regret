using UnityEngine;

public class ResetZone : MonoBehaviour
{
    public float resetDelay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        ResettableObject resettable = other.GetComponent<ResettableObject>();

        if (resettable != null)
        {
            resettable.TriggerReset(resetDelay);
        }
    }
}
