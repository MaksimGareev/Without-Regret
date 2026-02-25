using UnityEngine;

public class CrackTrigger : MonoBehaviour
{
    public CrackTest2 crack;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
             Debug.Log("[Trigger] Hit by " + other.name);
            crack.StartCrack();
        }
       
    }
}