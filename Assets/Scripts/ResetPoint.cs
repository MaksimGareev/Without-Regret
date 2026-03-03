using UnityEngine;

// Used for marking the FallResetTrigger's reset point in the scene.
// The FallResetTrigger will use this to populate the array of reset points
public class ResetPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
