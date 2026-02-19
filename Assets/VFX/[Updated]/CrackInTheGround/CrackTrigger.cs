using UnityEngine;

public class CrackTrigger : MonoBehaviour
{
    public CrackTest2 crack;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Trigger] Hit by " + other.name);
        crack.StartCrack();
    }
}