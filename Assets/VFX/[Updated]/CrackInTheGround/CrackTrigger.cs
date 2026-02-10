using UnityEngine;

public class CrackTrigger : MonoBehaviour
{
    public CrackBlendShapeStepSnap crack;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Trigger] Hit by " + other.name);
        crack.StartCrack();
    }
}