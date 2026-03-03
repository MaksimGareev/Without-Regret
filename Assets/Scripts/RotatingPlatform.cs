using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public float rotateSpeed = 10f;
    public int direction = 1; //1 for X, -1 for X
    [Tooltip("Should only be accessed by the objective manager. Set to true when you want the platform to stop")]
    public bool ObjectiveComplete = false;

    void LateUpdate()
    {
        if (!ObjectiveComplete)
        {
            Quaternion tempRotation = Quaternion.Euler(0, rotateSpeed * Time.deltaTime, 0);
            transform.rotation = transform.rotation * tempRotation;
        }
    }
}
