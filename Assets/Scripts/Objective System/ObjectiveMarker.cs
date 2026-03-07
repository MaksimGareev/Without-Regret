using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    public GameObject WorldIndicator;

    private void Start()
    {
        WorldIndicator.SetActive(false);
    }
}
