using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public float rotateSpeed = 10f;
    public int direction = 1; //1 for X, -1 for X

    void LateUpdate()
    {
        Quaternion tempRotation = Quaternion.Euler(0, rotateSpeed * Time.deltaTime, 0);
        transform.rotation = transform.rotation * tempRotation;
    }
}
