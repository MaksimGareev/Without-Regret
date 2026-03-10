using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    public GameObject WorldIndicator;

    private void DisableIndicator()
    {
        WorldIndicator.SetActive(false);
    }

    private void EnableIndicator()
    {
        WorldIndicator.SetActive(true);
    }
}
